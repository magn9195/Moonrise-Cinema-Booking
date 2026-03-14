using CinemaAPI.BusinessLogic.Interfaces;
using CinemaAPI.Core.DatabaseLayer.Interfaces;
using CinemaAPI.Core.Models;
using CinemaAPI.Core.Models.Enum;
using CinemaAPI.DTOs;

namespace CinemaAPI.BusinessLogic
{
	public class TicketService : ITicketService
	{
		private readonly ITicketAccess _ticketAccess;

		public TicketService(ITicketAccess ticketAccess)
		{
			_ticketAccess = ticketAccess;
		}

		public async Task<float> FetchCurrentPrice(TicketTypeEnum ticketType)
		{
			return await _ticketAccess.FetchCurrentPrice(ticketType);
		}

		public async Task<int> CheckReservations()
		{
			return await _ticketAccess.CheckReservations();
		}

		public async Task<bool> ReserveSeatsAsync(List<PostShowtimeSeatDTO> seats)
		{
			List<ShowtimeSeat> seatEntities = new List<ShowtimeSeat>();
			DateTime reservedTill = DateTime.Now.AddMinutes(15);
			if (seats != null && seats.Any())
			{
				foreach (var seatDTO in seats)
				{
					var seatEntity = MapToReservedEntityShowtimeSeat(seatDTO);
					seatEntities.Add(seatEntity);
				}
			}
			return await _ticketAccess.ReserveSeatsAsync(seatEntities, reservedTill, 3000);
		}

		public async Task<bool> CreateTicketAsync(PostTicketDTO ticket)
		{
			return await _ticketAccess.CreateTicketAsync(MapToEntityTicket(ticket));
		}

		private Ticket MapToEntityTicket(PostTicketDTO ticketDTO)
		{
			Ticket ticket = new Ticket
			{
				Customer = MapToEntityCustomer(ticketDTO.Customer),
				TicketTypeQuantities = ticketDTO.TicketTypeQuantities.Select(tq => MapToEntityTicketTypeQuantity(tq)).ToList(),
				BookedSeats = ticketDTO.BookedSeats.Select(bs => MapToBookedEntityShowtimeSeat(bs)).ToList(),
				PurchaseDate = DateTime.Now,
				ExpireDate = DateTime.Now.AddDays(3)
			};
			ticket.Price = ticket.TicketTypeQuantities.Sum(ttq => ttq.PricePerTicket * ttq.Quantity);
			return ticket;
		}

		private TicketTypeQuantity MapToEntityTicketTypeQuantity(PostTicketTypeQuantityDTO ticketTypeQuantityDTO)
		{
			var price = _ticketAccess.FetchCurrentPrice(ticketTypeQuantityDTO.TicketType).Result;
			return new TicketTypeQuantity
			{
				PricePerTicket = price,
				TicketType = ticketTypeQuantityDTO.TicketType,
				Quantity = ticketTypeQuantityDTO.Quantity
			};
		}

		private ShowtimeSeat MapToReservedEntityShowtimeSeat(PostShowtimeSeatDTO showtimeSeatDTO)
		{
			return new ShowtimeSeat
			{
				SeatID = showtimeSeatDTO.SeatId,
				ShowtimeID = showtimeSeatDTO.ShowtimeId,
				Status = (SeatStatusEnum)1
			};
		}

		private ShowtimeSeat MapToBookedEntityShowtimeSeat(PostShowtimeSeatDTO showtimeSeatDTO)
		{
			return new ShowtimeSeat
			{
				SeatID = showtimeSeatDTO.SeatId,
				ShowtimeID = showtimeSeatDTO.ShowtimeId,
				Status = (SeatStatusEnum)2
			};
		}

		private Customer MapToEntityCustomer(PostCustomerDTO customer)
		{
			return new Customer
			{
				Name = customer.Name,
				Email = customer.Email,
				PhoneNo = customer.PhoneNo,
				CreationDate = DateTime.Now
			};
		}
	}
}
