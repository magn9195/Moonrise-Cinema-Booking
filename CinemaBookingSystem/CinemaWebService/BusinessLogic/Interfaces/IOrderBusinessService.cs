using CinemaWebService.Models;
using CinemaWebService.Views.ViewModels;

namespace CinemaWebService.BusinessLogic.Interfaces
{
	public interface IOrderBusinessService
	{
		Task<OrderVM> BuildOrderViewModelAsync(PostTicketCS partialTicket, int movieID, int showtimeID);
	}
}
