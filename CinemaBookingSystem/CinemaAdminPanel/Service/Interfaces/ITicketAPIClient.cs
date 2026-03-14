using CinemaAdminPanel.Models;
using CinemaAdminPanel.Models.Enum;

namespace CinemaAdminPanel.Service.Interfaces
{
	public interface ITicketAPIClient
	{
		Task<float> FetchCurrentPrice(TicketTypeEnum ticketType);
		Task<int> CheckReservations();
		Task<bool> ReserveSeatsAsync(List<PostShowtimeSeatCS> reserveSeats);
		Task<bool> CreateTicketAsync(PostTicketCS ticket);
	}
}
