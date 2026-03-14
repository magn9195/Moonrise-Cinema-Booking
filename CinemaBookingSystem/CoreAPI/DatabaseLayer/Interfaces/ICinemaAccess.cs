using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CinemaAPI.Core.Models;

namespace CinemaAPI.Core.DatabaseLayer.Interfaces
{
	public interface ICinemaAccess
	{
		Task<IEnumerable<Cinema>?> GetAllCinemasAsync(string? city);
		Task<Cinema?> GetCinemaByIDAsync(int cinemaID);
		Task<Seat?> GetSeatByIDAsync(int seatID);
		Task<IEnumerable<CityZipcode>> GetAllCitiesAsync();
		Task<CityZipcode> GetCityByNameAsync(string cityName);
	}
}
