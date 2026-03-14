using CinemaAPI.Core.Models;
using CinemaAPI.Core.Models.Enum;
using CinemaAPI.DTOs;

namespace CinemaAPI.BusinessLogic.Interfaces
{
	public interface ITicketService
	{
		Task<float> FetchCurrentPrice(TicketTypeEnum ticketType);
		Task<int> CheckReservations();
		Task<bool> ReserveSeatsAsync(List<PostShowtimeSeatDTO> seats);
		Task<bool> CreateTicketAsync(PostTicketDTO ticket);
	}
}
