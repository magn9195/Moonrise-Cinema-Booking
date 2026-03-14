using CinemaWebService.Models;
using CinemaWebService.Models.Enum;

namespace CinemaWebService.Service.Interfaces
{
	public interface ITicketAPIClient
	{
		Task<float> FetchCurrentPrice(TicketTypeEnum ticketType);
		Task<int> CheckReservations();
		Task<bool> ReserveSeatsAsync(List<PostShowtimeSeatCS> reserveSeats);
		Task<bool> CreateTicketAsync(PostTicketCS ticket);
	}
}
