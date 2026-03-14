using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CinemaAPI.Core.Models;
using CinemaAPI.Core.Models.Enum;

namespace CinemaAPI.Core.DatabaseLayer.Interfaces
{
	public interface ITicketAccess
	{
		Task<float> FetchCurrentPrice(TicketTypeEnum TicketType);
		Task<int> CheckReservations();
		Task<bool> ReserveSeatsAsync(List<ShowtimeSeat> seats, DateTime reservedTill, int artificialDelayMs);
		Task<bool> CreateTicketAsync(Ticket ticket);
	}
}
