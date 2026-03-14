using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CinemaAdminPanel.BusinessLogic.Interfaces;
using CinemaAdminPanel.Models;
using CinemaAdminPanel.Service.Interfaces;

namespace CinemaAdminPanel.BusinessLogic
{
	public class CinemaBusinessService : ICinemaBusinessService
	{
		private readonly ICinemaAPIClient _cinemaApiClient;

		public CinemaBusinessService(ICinemaAPIClient cinemaDataAccess)
		{
			_cinemaApiClient = cinemaDataAccess;
		}

		public async Task<IEnumerable<GetCinemaCS>?> GetAllCinemasAsync(string? city)
		{
			var cinemas = await _cinemaApiClient.GetAllCinemasAsync(city);
			return cinemas;
		}

		public async Task<GetCinemaCS?> GetCinemaByIDAsync(int cinemaID)
		{
			var cinema = await _cinemaApiClient.GetCinemaByIDAsync(cinemaID);
			return cinema;
		}
	}
}
