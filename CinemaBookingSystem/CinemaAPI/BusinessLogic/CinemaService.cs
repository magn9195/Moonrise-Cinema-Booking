using CinemaAPI.BusinessLogic.Interfaces;
using CinemaAPI.Core.DatabaseLayer;
using CinemaAPI.Core.DatabaseLayer.Interfaces;
using CinemaAPI.Core.Models;
using CinemaAPI.DTOs;

namespace CinemaAPI.BusinessLogic
{
	public class CinemaService : ICinemaService
	{
		private readonly ICinemaAccess _cinemaAccess;

		public CinemaService(ICinemaAccess cinemaAccess)
		{
			_cinemaAccess = cinemaAccess;
		}

		public async Task<List<GetCinemaDTO>?> GetAllCinemasAsync(string? city)
		{
			List<GetCinemaDTO> cinemaDTOList = new List<GetCinemaDTO>();
			var cinemas = await _cinemaAccess.GetAllCinemasAsync(city);
			if (cinemas != null && cinemas.Any())
			{
				foreach (var cinema in cinemas)
				{
					GetCinemaDTO cinemaDTO = MapToDTOCinema(cinema);
					cinemaDTOList.Add(cinemaDTO);
				}
			}
			return cinemaDTOList;
		}

		public async Task<GetCinemaDTO?> GetCinemaByIDAsync(int cinemaID)
		{
			GetCinemaDTO? cinemaDTO = null;
			var cinema = await _cinemaAccess.GetCinemaByIDAsync(cinemaID);
			if (cinema != null)
			{
				cinemaDTO = MapToDTOCinema(cinema);
			}
			return cinemaDTO;
		}

		public async Task<GetSeatDTO?> GetSeatByIDAsync(int seatID)
		{
			GetSeatDTO? seatDTO = null;
			var seat = await _cinemaAccess.GetSeatByIDAsync(seatID);
			if (seat != null)
			{
				seatDTO = MapToDTOSeat(seat);
			}
			return seatDTO;
		}

		public async Task<IEnumerable<GetCityZipcodeDTO>> GetAllCitiesAsync()
		{
			Dictionary<string ,GetCityZipcodeDTO> uniqueCitiesDictionary = [];
			var cities = await _cinemaAccess.GetAllCitiesAsync();

			if (cities != null && cities.Any())
			{
				foreach(var city in cities)
				{
					var mappedCity = MapToDTOCityZipcode(city);
					uniqueCitiesDictionary.Add(mappedCity.City, mappedCity);
				}
			}
			return uniqueCitiesDictionary.Values;
		}

		public async Task<GetCityZipcodeDTO?> GetCityByNameAsync(string cityName)
		{
			GetCityZipcodeDTO? cityDTO = null;
			var city = await _cinemaAccess.GetCityByNameAsync(cityName);
			if (city != null)
			{
				cityDTO = MapToDTOCityZipcode(city);
			}
			return cityDTO;
		}

		private GetCinemaDTO MapToDTOCinema(Cinema cinema)
		{
			return new GetCinemaDTO
			{
				CinemaID = cinema.CinemaID,
				Name = cinema.Name,
				Address = MapToDTOAddress(cinema.Address),
				Auditoriums = cinema.Auditoriums.Select(a => MapToAuditorium(a)).ToList()
			};
		}

		private GetSeatDTO MapToDTOSeat(Seat seat)
		{
			return new GetSeatDTO
			{
				SeatID = seat.SeatID,
				RowNo = seat.RowNo,
				SeatNo = seat.SeatNo,
				SeatType = seat.SeatType,
				AuditoriumID = seat.AuditoriumID
			};
		}

		private GetAddressDTO MapToDTOAddress(Address address)
		{
			return new GetAddressDTO
			{
				HouseNumber = address.HouseNumber,
				StreetName = address.StreetName,
				CityZipCode = MapToDTOCityZipcode(address.CityZipCode)
			};
		}

		private GetCityZipcodeDTO MapToDTOCityZipcode(CityZipcode cityZipCode)
		{
			return new GetCityZipcodeDTO
			{
				CityZipcodeID = cityZipCode.CityZipCodeID,
				ZipCode = cityZipCode.ZipCode,
				City = cityZipCode.City
			};
		}

		private GetAuditoriumDTO MapToAuditorium(Auditorium auditorium)
		{
			return new GetAuditoriumDTO
			{
				AuditoriumID = auditorium.AuditoriumID,
				Name = auditorium.Name,
				RowNum = auditorium.RowNum,
				SeatsPerRow = auditorium.SeatsPerRow,
				CinemaID = auditorium.CinemaID
			};
		}

	}
}
