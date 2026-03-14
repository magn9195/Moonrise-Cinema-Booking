using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CinemaAdminPanel.Models;

namespace CinemaAdminPanel.BusinessLogic.Interfaces
{
	public interface ICinemaBusinessService
	{
		Task<IEnumerable<GetCinemaCS>?> GetAllCinemasAsync(string? city);
		Task<GetCinemaCS?> GetCinemaByIDAsync(int cinemaID);
	}
}
