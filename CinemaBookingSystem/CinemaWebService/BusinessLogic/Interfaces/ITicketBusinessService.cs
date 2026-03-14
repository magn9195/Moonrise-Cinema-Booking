using CinemaWebService.Models;
using CinemaWebService.Models.Enum;
using CinemaWebService.Views.ViewModels;

namespace CinemaWebService.BusinessLogic.Interfaces
{
	public interface ITicketApiClient
	{
		Task<int> CheckReservations();
		Task<bool> ReserveSeatsAsync(List<int> selectedSeats, int showtimeID);
		Task<bool> CreateTicketAsync(PostTicketCS ticket);
		List<PostTicketTypeQuantityCS> GenerateTicketTypeQuantities(List<string> TicketTypes);
		List<PostShowtimeSeatCS> GeneratePostShowtimeSeats(List<int> seatIDs, int showtimeID);
		Task<ResultSeatReservationCS> ProcessSeatReservationAsync(List<int> seatIDs, int showtimeID, List<string> ticketTypes);
	}
}
