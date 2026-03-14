using CinemaAPI.Core.DatabaseLayer.Interfaces;
using CinemaAPI.Core.Models;
using CinemaAPI.Core.Models.Enum;
using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CinemaAPI.Core.DatabaseLayer
{
	public class TicketAccess : ITicketAccess
	{
		private readonly string _connectionString = "";

		public TicketAccess(IConfiguration configuration)
		{
			var connectionString = configuration.GetConnectionString("DefaultConnection");
			if (connectionString != null && !string.IsNullOrWhiteSpace(connectionString))
			{
				_connectionString = connectionString;
			}
		}

		// Fetch the current price for a given ticket type
		public async Task<float> FetchCurrentPrice(TicketTypeEnum TicketType)
		{
			string sqlFetchPrice = """
					SELECT currentPrice 
					FROM TicketType 
					WHERE type = @ticketType
				""";

			using (var connection = new SqlConnection(_connectionString))
			{
				connection.Open();
				var price = await connection.QuerySingleAsync<float>(sqlFetchPrice, new
				{
					ticketType = TicketType
				});
				return price;
			}
		}

		// Check and release expired reservations
		public async Task<int> CheckReservations()
		{
			string sqlCheckReservations = """
					UPDATE ShowtimeSeat
					SET status = 0,
				    ticketID = NULL,
				    reservedTill = NULL
					WHERE reservedTill < GETDATE()
					AND reservedTill IS NOT NULL
					AND ticketID IS NULL;
				""";
			using (var connection = new SqlConnection(_connectionString))
			{
				connection.Open();
				var rowsAffected = await connection.ExecuteAsync(sqlCheckReservations);
				return rowsAffected;
			}
		}

		// Reserve seats for a showtime (Optimistic Concurrency)
		public async Task<bool> ReserveSeatsAsync(List<ShowtimeSeat> seats, DateTime reservedTill, int artificialDelayMs)
		{
			string sqlReadSeats = """
					SELECT showtimeID, seatID, status, ticketID, reservedTill, RowVersion
					FROM ShowtimeSeat
					WHERE showtimeID = @showtimeID 
					AND seatID = @seatID
				""";

			string sqlReserveSeats = """
					UPDATE ShowtimeSeat
					SET 
					status = @status,
					reservedTill = @reservedTill
					WHERE showtimeID = @showtimeID 
					AND seatID = @seatID
					AND RowVersion = @rowVersion;
				""";

			using (var connection = new SqlConnection(_connectionString))
			{
				await connection.OpenAsync();
				using (var transaction = await connection.BeginTransactionAsync())
				{
					try
					{
						var orderedSeats = seats.OrderBy(s => s.SeatID).ToList();
						var seatsChecked = new List<ShowtimeSeat>();

						foreach (var seat in orderedSeats)
						{
							var readSeat = await connection.QuerySingleOrDefaultAsync<ShowtimeSeat>(
								sqlReadSeats,
								new
								{
									showtimeID = seat.ShowtimeID,
									seatID = seat.SeatID
								},
								transaction: transaction);


							if (readSeat == null)
							{
								throw new InvalidOperationException($"Seat {seat.SeatID} not found.");
							}

							seatsChecked.Add(readSeat);
						}

						foreach (var checkedSeat in seatsChecked)
						{
							if (checkedSeat.TicketID != null || (checkedSeat.ReservedTill.HasValue && checkedSeat.ReservedTill.Value > DateTime.Now))
							{
								throw new InvalidOperationException($"Seat {checkedSeat.SeatID} already booked or reserved.");
							}
						}

						if (artificialDelayMs > 0)
						{
							await Task.Delay(artificialDelayMs);
						}

						for (int i = 0; i < orderedSeats.Count; i++)
						{
							var seat = orderedSeats[i];
							var checkedSeat = seatsChecked[i];

							var rowsAffected = await connection.ExecuteAsync(sqlReserveSeats, new
							{
								status = seat.Status,
								reservedTill = reservedTill,
								showtimeID = seat.ShowtimeID,
								seatID = seat.SeatID,
								rowVersion = checkedSeat.RowVersion
							}, transaction: transaction);

							if (rowsAffected == 0)
							{
								throw new InvalidOperationException($"Concurrency conflict: Seat was changed by another transaction.");
							}
						}

						await transaction.CommitAsync();
						return true;
					}
					catch (Exception ex)
					{
						await transaction.RollbackAsync();
						return false;
					}
				}
			}
		}

		// Create a ticket along with customer and booked seats
		public async Task<bool> CreateTicketAsync(Ticket ticket)
		{
			string sqlValidateSeats = """
					SELECT showtimeID, seatID, status, ticketID, reservedTill
					FROM ShowtimeSeat
					WHERE showtimeID = @showtimeID 
					AND seatID = @seatID
				""";

			string sqlCreateCustomer = """
					INSERT INTO Customer(Name, Email, PhoneNo, CreationDate) 
					OUTPUT INSERTED.customerID
					VALUES (@name, @email, @phoneNo, @creationDate)
				""";


			string sqlCreateTicket = """
					INSERT INTO Ticket(price, purchaseDate, expireDate, CustomerID) 
					OUTPUT INSERTED.ticketID 
					VALUES (@price, @purchaseDate, @expireDate, @customerID);
				""";

			string sqlInsertTicketTypeQuantity = """
					INSERT INTO TicketTypeQuantity(ticketID, ticketType, quantity, pricePerTicket)
					VALUES (@ticketID, @ticketType, @quantity, @pricePerTicket);
				""";

			string sqlSetSeatAvailability = """
					UPDATE ShowtimeSeat
					SET 
					status = @status,
					reservedTill = NULL,
					ticketID = @ticketID
					WHERE showtimeID = @showtimeID AND 
					seatID = @seatID
				""";

			using (var connection = new SqlConnection(_connectionString))
			{
				connection.Open();
				using (var transaction = await connection.BeginTransactionAsync())
				{
					try
					{
						var orderedSeats = ticket.BookedSeats.OrderBy(s => s.SeatID).ToList();

						foreach (var seat in orderedSeats)
						{
							var seatData = await connection.QuerySingleOrDefaultAsync<ShowtimeSeat>(
								sqlValidateSeats,
								new
								{
									showtimeID = seat.ShowtimeID,
									seatID = seat.SeatID
								},
								transaction: transaction);

							if (seatData == null)
							{
								throw new InvalidOperationException($"Seat not found.");
							}

							if (seatData.TicketID != null)
							{
								throw new InvalidOperationException($"Seat already booked.");
							}

							if (!seatData.ReservedTill.HasValue || seatData.ReservedTill <= DateTime.Now)
							{
								throw new InvalidOperationException($"Seat reservation expired for selected Seats.");
							}
						}

						var CustomerIDQuery = await connection.QuerySingleAsync<int>(sqlCreateCustomer, new
						{
							name = ticket.Customer.Name,
							email = ticket.Customer.Email,
							phoneNo = ticket.Customer.PhoneNo,
							creationDate = DateTime.Now

						}, transaction: transaction);

						var TicketIdQuery = await connection.QuerySingleAsync<int>(sqlCreateTicket, new
						{
							price = ticket.Price,
							purchaseDate = DateTime.Now,
							expireDate = ticket.ExpireDate,
							customerID = CustomerIDQuery
						}, transaction: transaction);

						foreach (var ttq in ticket.TicketTypeQuantities)
						{
							await connection.ExecuteAsync(sqlInsertTicketTypeQuantity, new
							{
								ticketID = TicketIdQuery,
								ticketType = ttq.TicketType,
								quantity = ttq.Quantity,
								pricePerTicket = ttq.PricePerTicket
							}, transaction: transaction);
						}

						int totalQuantity = ticket.TicketTypeQuantities.Sum(ttq => ttq.Quantity);
						int totalSeats = ticket.BookedSeats.Count;
						if (totalQuantity != totalSeats)
						{
							throw new InvalidOperationException($"Ticket type quantities ({totalQuantity}) must match number of seats ({totalSeats})");
						}

						foreach (var seat in ticket.BookedSeats)
						{
							var rowsAffected = await connection.ExecuteAsync(sqlSetSeatAvailability, new
							{
								status = seat.Status,
								ticketID = TicketIdQuery,
								showtimeID = seat.ShowtimeID,
								seatID = seat.SeatID
							}, transaction: transaction);

							if (rowsAffected == 0)
							{
								throw new InvalidOperationException($"ShowtimeSeat not found or already booked.");
							}
						}

						await transaction.CommitAsync();
						return true;
					}
					catch
					{
						await transaction.RollbackAsync();
						return false;
					}
				}
			}
		}
	}
}
