namespace CinemaWebService.Models
{
	public class ResultSeatReservationCS
	{
		public required bool Success { get; set; }
		public required List<PostShowtimeSeatCS> BookedSeats { get; set; } = [];
		public required PostTicketCS PartialTicket { get; set; }
	}
}
