using CinemaAPI.Core.DatabaseLayer;
using CinemaAPI.Core.Models;
using CinemaAPI.Core.Models.Enum;
using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;
using Xunit;

namespace CinemaAPI.Tests.DatabaseTesting
{
	[Collection("Database Tests")]
	public class TicketAccessTests : IDisposable
	{
		private readonly ConnectionHandler _handler;
		private readonly TicketAccess _ticketAccess;
		private readonly ShowtimeAccess _showtimeAccess;
		private readonly string _connectionString;

		public TicketAccessTests(ConnectionHandler handler)
		{
			_handler = handler;
			_connectionString = _handler.ConnectionString;
			_ticketAccess = new TicketAccess(_handler.Configuration);
			_showtimeAccess = new ShowtimeAccess(_handler.Configuration);

			SeedTestData();
		}

		[Fact]
		public async Task CreateTicketAsync_WithValidData_ReturnsTrue()
		{
			// Arrange
			int showtimeId = 1;
			int seatId = 1;

			var seat = await _showtimeAccess.GetShowtimeSeatByIDAsync(showtimeId, seatId);
			Assert.NotNull(seat);
			List<ShowtimeSeat> seats = new List<ShowtimeSeat> { seat };
			bool success = await _ticketAccess.ReserveSeatsAsync(seats, DateTime.Now.AddMinutes(15), 0);
			Assert.True(success);

			var ticket = new Ticket
			{
				Customer = new Customer
				{
					Name = "Jane Smith",
					Email = "jane@example.com",
					PhoneNo = "99999999",
					CreationDate = DateTime.Now
				},
				Price = 140f,
				TicketTypeQuantities = [new TicketTypeQuantity {
						TicketType = TicketTypeEnum.Adult,
						Quantity = 1,
						PricePerTicket = 140f
					}],
				BookedSeats = [new ShowtimeSeat {
						Status = SeatStatusEnum.Booked,
						SeatID = seatId,
						ShowtimeID = showtimeId
					}],
				PurchaseDate = DateTime.Now,
				ExpireDate = DateTime.Now.AddDays(7)
			};

			// Act
			var result = await _ticketAccess.CreateTicketAsync(ticket);

			// Assert
			Assert.True(result);
		}

		[Fact]
		public async Task CreateTicketAsync_CreatesCustomerInDatabase()
		{
			// Arrange
			int showtimeId = 1;
			int seatId = 2;

			var seat = await _showtimeAccess.GetShowtimeSeatByIDAsync(showtimeId, seatId);
			Assert.NotNull(seat);
			List<ShowtimeSeat> seats = new List<ShowtimeSeat> { seat };
			bool success = await _ticketAccess.ReserveSeatsAsync(seats, DateTime.Now.AddMinutes(15), 0);
			Assert.True(success);

			var ticket = new Ticket
			{
				Customer = new Customer
				{
					Name = "Jane Smith",
					Email = "jane@example.com",
					PhoneNo = "99999999",
					CreationDate = DateTime.Now
				},
				Price = 140f,
				TicketTypeQuantities = [new TicketTypeQuantity {
				TicketType = TicketTypeEnum.Adult,
				Quantity = 1,
				PricePerTicket = 140f
			}],
				BookedSeats = [new ShowtimeSeat {
				Status = SeatStatusEnum.Booked,
				SeatID = seatId,
				ShowtimeID = showtimeId
			}],
				PurchaseDate = DateTime.Now,
				ExpireDate = DateTime.Now.AddDays(7)
			};

			// Act
			bool successTicket = await _ticketAccess.CreateTicketAsync(ticket);
			Assert.True(successTicket);

			// Assert
			using var connection = new SqlConnection(_connectionString);
			var customer = await connection.QuerySingleOrDefaultAsync<Customer>(
				"SELECT * FROM Customer WHERE Email = @Email",
				new { Email = "jane@example.com" }
			);

			Assert.NotNull(customer);
			Assert.Equal("Jane Smith", customer.Name);
			Assert.Equal("jane@example.com", customer.Email);
		}

		[Fact]
		public async Task CreateTicketAsync_CreatesTicketInDatabase()
		{
			// Arrange
			int showtimeId = 1;
			int seatId = 3;
			var expectedPrice = 120.0f;

			var seat = await _showtimeAccess.GetShowtimeSeatByIDAsync(showtimeId, seatId);
			Assert.NotNull(seat);
			List<ShowtimeSeat> seats = new List<ShowtimeSeat> { seat };
			bool success = await _ticketAccess.ReserveSeatsAsync(seats, DateTime.Now.AddMinutes(15), 0);
			Assert.True(success);

			var ticket = new Ticket
			{
				Customer = new Customer
				{
					Name = "Test User",
					Email = "test@example.com",
					PhoneNo = "99999999",
					CreationDate = DateTime.Now
				},
				Price = expectedPrice,
				TicketTypeQuantities = [new TicketTypeQuantity {
						TicketType = TicketTypeEnum.Adult,
						Quantity = 1,
						PricePerTicket = expectedPrice
					}],
				BookedSeats = [new ShowtimeSeat {
						Status = SeatStatusEnum.Booked,
						SeatID = seatId,
						ShowtimeID = showtimeId
					}],
				PurchaseDate = DateTime.Now,
				ExpireDate = DateTime.Now.AddDays(7)
			};

			// Act
			await _ticketAccess.CreateTicketAsync(ticket);

			// Assert
			using var connection = new SqlConnection(_connectionString);
			var newTicket = await connection.QuerySingleOrDefaultAsync<Ticket>(
				"SELECT * FROM Ticket WHERE Price = @Price",
				new { Price = expectedPrice }
			);

			Assert.NotNull(newTicket);
			Assert.Equal(expectedPrice, newTicket.Price);

			var ticketTypeQuantities = await connection.QueryAsync<TicketTypeQuantity>(
				"SELECT * FROM TicketTypeQuantity WHERE TicketID = @TicketID",
				new { TicketID = newTicket.TicketID }
			);

			var ttq = ticketTypeQuantities.FirstOrDefault();
			Assert.NotNull(ttq);
			Assert.Equal(TicketTypeEnum.Adult, ttq.TicketType);

			var bookedSeats = await connection.QueryAsync<ShowtimeSeat>(
				"SELECT * FROM ShowtimeSeat WHERE TicketID = @TicketID",
				new { TicketID = newTicket.TicketID }
			);

			var bookedSeat = bookedSeats.FirstOrDefault();
			Assert.NotNull(bookedSeat);
			Assert.Equal(seatId, bookedSeat.SeatID);
		}

		[Fact]
		public async Task CreateTicketAsync_UpdatesShowtimeSeatStatus()
		{
			// Arrange
			int showtimeId = 1;
			int seatId = 4;

			var seat = await _showtimeAccess.GetShowtimeSeatByIDAsync(showtimeId, seatId);
			Assert.NotNull(seat);
			List<ShowtimeSeat> seats = new List<ShowtimeSeat> { seat };
			bool success = await _ticketAccess.ReserveSeatsAsync(seats, DateTime.Now.AddMinutes(15), 0);
			Assert.True(success);

			var ticket = new Ticket
			{
				Customer = new Customer
				{
					Name = "Test User",
					Email = "test@example.com",
					PhoneNo = "99999999",
					CreationDate = DateTime.Now
				},
				Price = 100.0f,
				TicketTypeQuantities = [new TicketTypeQuantity {
						TicketType = TicketTypeEnum.Adult,
						Quantity = 1,
						PricePerTicket = 100.0f
					}],
				BookedSeats = [new ShowtimeSeat {
						Status = SeatStatusEnum.Booked,
						SeatID = seatId,
						ShowtimeID = showtimeId
					}],
				PurchaseDate = DateTime.Now,
				ExpireDate = DateTime.Now.AddDays(7)
			};

			// Act
			await _ticketAccess.CreateTicketAsync(ticket);

			// Assert
			using var connection = new SqlConnection(_connectionString);
			var updatedSeat = await connection.QuerySingleOrDefaultAsync(
				"SELECT Status, TicketID FROM ShowtimeSeat WHERE ShowtimeID = @ShowtimeId AND SeatID = @SeatId",
				new { ShowtimeId = showtimeId, SeatId = seatId }
			);

			Assert.NotNull(updatedSeat);
			Assert.Equal((int)SeatStatusEnum.Booked, updatedSeat.Status);
			Assert.NotNull(updatedSeat.TicketID);
		}

		[Fact]
		public async Task CreateTicketAsync_TransactionRollsBackOnError()
		{
			// Arrange
			int showtimeId = 99999;
			int seatId = 1;
			var ticket = new Ticket
			{
				Customer = new Customer
				{
					Name = "Test User",
					Email = "test@example.com",
					PhoneNo = "99999999",
					CreationDate = DateTime.Now
				},
				Price = 100.0f,
				TicketTypeQuantities = [new TicketTypeQuantity {
						TicketType = TicketTypeEnum.Adult,
						Quantity = 1,
						PricePerTicket = 100.0f
					}],
				BookedSeats = [new ShowtimeSeat {
						Status = SeatStatusEnum.Booked,
						SeatID = seatId,
						ShowtimeID = showtimeId
					}],
				PurchaseDate = DateTime.Now,
				ExpireDate = DateTime.Now.AddDays(7)
			};

			// Act
			var result = await _ticketAccess.CreateTicketAsync(ticket);

			// Assert
			Assert.False(result);

			using var connection = new SqlConnection(_connectionString);
			var customer = await connection.QuerySingleOrDefaultAsync<Customer>(
				"SELECT * FROM Customer WHERE Email = @Email",
				new { Email = "test@example.com" }
			);
			Assert.Null(customer);
		}

		[Fact]
		public async Task CreateTicketAsync_WithExpiredReservation_ReturnsFalse()
		{
			// Arrange
			int showtimeId = 1;
			int seatId = 5;
			using var connection = new SqlConnection(_connectionString);
			await connection.ExecuteAsync(@"
				UPDATE ShowtimeSeat 
				SET status = 1, 
				    reservedTill = @ExpiredTime
				WHERE showtimeID = @ShowtimeId 
				AND seatID = @SeatId",
				new
				{
					ExpiredTime = DateTime.Now.AddMinutes(-5),
					ShowtimeId = showtimeId,
					SeatId = seatId
				}
			);

			var ticket = new Ticket
			{
				Customer = new Customer
				{
					Name = "Test User",
					Email = "test@example.com",
					PhoneNo = "99999999",
					CreationDate = DateTime.Now
				},
				Price = 100.0f,
				TicketTypeQuantities = [new TicketTypeQuantity {
						TicketType = TicketTypeEnum.Adult,
						Quantity = 1,
						PricePerTicket = 100.0f
					}],
				BookedSeats = [new ShowtimeSeat {
						Status = SeatStatusEnum.Booked,
						SeatID = seatId,
						ShowtimeID = showtimeId
					}],
				PurchaseDate = DateTime.Now,
				ExpireDate = DateTime.Now.AddDays(7)
			};

			// Act
			var result = await _ticketAccess.CreateTicketAsync(ticket);

			// Assert
			Assert.False(result);
		}

		private void SeedTestData()
		{
			using var connection = new SqlConnection(_connectionString);
			connection.Open();
			connection.Execute("DELETE FROM ShowtimeSeat");
			connection.Execute("DELETE FROM TicketTypeQuantity");
			connection.Execute("DELETE FROM Ticket");
			connection.Execute("DELETE FROM Showtime");
			connection.Execute("DELETE FROM Seat");
			connection.Execute("DELETE FROM Auditorium");
			connection.Execute("DELETE FROM Cinema");
			connection.Execute("DELETE FROM Address");
			connection.Execute("DELETE FROM CityZipcode");
			connection.Execute("DELETE FROM MovieGenre");
			connection.Execute("DELETE FROM MovieImage");
			connection.Execute("DELETE FROM Movie");
			connection.Execute("DELETE FROM Genre");
			connection.Execute("DELETE FROM Customer");
			connection.Execute("DELETE FROM TicketType");

			connection.Execute("DBCC CHECKIDENT ('Seat', RESEED, 0)");
			connection.Execute("DBCC CHECKIDENT ('Ticket', RESEED, 0)");
			connection.Execute("DBCC CHECKIDENT ('Customer', RESEED, 0)");

			connection.Execute(@"
                INSERT INTO Genre (GenreID, Name)
                VALUES (1, 'Action'), (2, 'Drama'), (3, 'Comedy')
            ");

			connection.Execute(@"
                INSERT INTO Movie (MovieID, Title, ReleaseDate, AgeLimit, Duration, 
                    TrailerYouTubeID, Resume, Language, Subtitles, Director, MainActor)
                VALUES 
                (1, 'Test Movie 1', '2024-01-01', 13, 120, 'abc123', 'Test Resume 1', 0, 0, 'Director 1', 'Actor 1'),
                (2, 'Test Movie 2', '2024-02-01', 16, 150, 'def456', 'Test Resume 2', 1, 1, 'Director 2', 'Actor 2')
            ");

			connection.Execute(@"
                INSERT INTO MovieGenre (MovieID, GenreID)
                VALUES (1, 1), (1, 2), (2, 2), (2, 3)
            ");

			connection.Execute(@"
                INSERT INTO MovieImage (ImageID, MovieID, ImageData)
                VALUES 
                (1, 1, 0x89504E470D0A1A0A),
                (2, 1, 0x89504E470D0A1A0B),
                (3, 2, 0x89504E470D0A1A0C)
            ");

			connection.Execute(@"
                INSERT INTO CityZipcode (CityZipCodeID, Zipcode, City)
                VALUES (1, '12345', 'Test City')
            ");

			connection.Execute(@"
                INSERT INTO Address (AddressID, HouseNumber, StreetName, CityZipCodeID)
                VALUES (1, 123, 'Test Street', 1)
            ");

			connection.Execute(@"
                INSERT INTO Cinema (CinemaID, Name, AddressID)
                VALUES (1, 'Test Cinema', 1)
            ");

			connection.Execute(@"
                INSERT INTO Auditorium (AuditoriumID, Name, RowNum, seatsPerRow, CinemaID)
                VALUES (1, 'Bio1', 2, 20, 1), (2, 'Bio2', 2, 18, 1)
            ");

			connection.Execute(@"
				INSERT INTO Seat (rowNo, seatNo, seatType, auditoriumID)
				VALUES 
				(1, 1, 1, 1),
				(1, 2, 1, 1),
				(1, 3, 1, 1),
				(1, 4, 1, 1),
				(1, 5, 1, 1)
			");

			connection.Execute(@"
                INSERT INTO Showtime (ShowtimeID, ShowType, StartTime, MovieID, AuditoriumID)
                VALUES 
                (1, 0, '2024-12-01 19:00', 1, 1),
                (2, 1, '2024-12-01 21:00', 1, 1),
                (3, 0, '2024-12-02 19:00', 2, 2)
            ");

			connection.Execute(@"
				INSERT INTO TicketType (type, currentPrice, description)
				VALUES 
				(0, 140.0, 'Adult ticket'),
				(1, 100.0, 'Child ticket'),
				(2, 120.0, 'Student ticket'),
				(3, 120.0, 'Senior ticket')
			");

			connection.Execute(@"
                UPDATE ShowtimeSeat 
                SET Status = 2
                WHERE ShowtimeID = 1 AND SeatID = 3
            ");

			connection.Execute(@"
                UPDATE ShowtimeSeat 
                SET Status = 2
                WHERE ShowtimeID = 2 AND SeatID = 5
            ");
		}

		public void Dispose()
		{
			using var connection = new SqlConnection(_connectionString);
			connection.Execute("DELETE FROM ShowtimeSeat");
			connection.Execute("DELETE FROM TicketTypeQuantity");
			connection.Execute("DELETE FROM Ticket");
			connection.Execute("DELETE FROM Showtime");
			connection.Execute("DELETE FROM Seat");
			connection.Execute("DELETE FROM Auditorium");
			connection.Execute("DELETE FROM Cinema");
			connection.Execute("DELETE FROM Address");
			connection.Execute("DELETE FROM CityZipcode");
			connection.Execute("DELETE FROM MovieGenre");
			connection.Execute("DELETE FROM MovieImage");
			connection.Execute("DELETE FROM Movie");
			connection.Execute("DELETE FROM Genre");
			connection.Execute("DELETE FROM Customer");
			connection.Execute("DELETE FROM TicketType");
		}
	}
}