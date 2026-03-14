using Microsoft.AspNetCore.SignalR;

namespace CinemaWebService.Service.SignalHub
{
	public class SeatBookingHub : Hub
	{
		public async Task SeatBooked(int showtimeId, int seatId)
		{
			// Broadcast to all clients except the sender
			await Clients.Others.SendAsync("ReceiveSeatBooked", showtimeId, seatId);
		}

		public async Task SeatBookedToAll(int showtimeId, int seatId)
		{
			// Broadcast to all clients including the sender
			await Clients.All.SendAsync("ReceiveSeatBooked", showtimeId, seatId);
		}

		public async Task SeatReserved(int showtimeId, int seatId)
		{
			// Broadcast to all clients except the sender
			await Clients.Others.SendAsync("ReceiveSeatReserved", showtimeId, seatId);
		}

		public async Task SeatReservedToAll(int showtimeId, int seatId)
		{
			// Broadcast to all clients including the sender
			await Clients.All.SendAsync("ReceiveSeatReserved", showtimeId, seatId);
		}
	}
}
