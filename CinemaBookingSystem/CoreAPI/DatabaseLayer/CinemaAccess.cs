using CinemaAPI.Core.DatabaseLayer.Interfaces;
using CinemaAPI.Core.Models;
using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CinemaAPI.Core.DatabaseLayer
{
	public class CinemaAccess : ICinemaAccess
	{
		private readonly string _connectionString = "";

		public CinemaAccess(IConfiguration configuration)
		{
			var connectionString = configuration.GetConnectionString("DefaultConnection");
			if (connectionString != null && !string.IsNullOrWhiteSpace(connectionString))
			{
				_connectionString = connectionString;
			} 
		}

		public async Task<IEnumerable<Cinema>?> GetAllCinemasAsync(string? city)
		{
			string sql = """
				SELECT 
				c.cinemaID, c.name,
				a.addressID, a.housenumber, a.streetName,
				z.cityZipCodeID, z.zipcode, z.city,
				at.auditoriumID, at.name, at.rowNum, at.seatsPerRow
				FROM Cinema c
				JOIN Address a ON a.addressID = c.addressID
				JOIN CityZipcode z ON z.cityZipCodeID = a.cityZipCodeID
				JOIN Auditorium at ON at.cinemaID = c.cinemaID
				WHERE (@cityCinema IS NULL OR city = @cityCinema)
				""";
			using (var connection = new SqlConnection(_connectionString))
			{
				Dictionary<int, Cinema> cinemaDictionary = new Dictionary<int, Cinema>();

				var result = await connection.QueryAsync<Cinema, Address, CityZipcode, Auditorium, Cinema>(
					sql,
					(cinema, address, cityZipcode, auditorium) =>
					{
						if(!cinemaDictionary.ContainsKey(cinema.CinemaID))
						{
							cinema.Auditoriums = new List<Auditorium>();
							cinemaDictionary.Add(cinema.CinemaID, cinema);
						}

						var existingCinema = cinemaDictionary[cinema.CinemaID];

						if (auditorium != null)
						{
							if (!existingCinema.Auditoriums.Any(a => a.AuditoriumID == auditorium.AuditoriumID)) {
								existingCinema.Auditoriums.Add(auditorium);
							}
						}

						address.CityZipCode = cityZipcode;
						cinema.Address = address;
						return existingCinema;
					}, new
					{
						cityCinema = city
					},
					splitOn: "addressID, cityZipCodeID, auditoriumID"
				);
				return cinemaDictionary.Values;
			}
		}

		public async Task<Cinema?> GetCinemaByIDAsync(int cinemaID)
		{
			string sql = """
        SELECT 
        c.cinemaID, c.name,
        a.addressID, a.housenumber, a.streetName,
        z.cityZipCodeID, z.zipcode, z.city,
        at.auditoriumID, at.name, at.rowNum, at.seatsPerRow
        FROM Cinema c
        JOIN Address a ON a.addressID = c.addressID
        JOIN CityZipcode z ON z. cityZipCodeID = a. cityZipCodeID
        JOIN Auditorium at ON at.cinemaID = c.cinemaID
        WHERE c.cinemaID = @cinemaIDQuery
        """;

			using (var connection = new SqlConnection(_connectionString))
			{
				var cinemaDictionary = new Dictionary<int, Cinema>();

				var result = await connection.QueryAsync<Cinema, Address, CityZipcode, Auditorium, Cinema>(
					sql,
					(cinema, address, cityZipcode, auditorium) =>
					{
						if (!cinemaDictionary.ContainsKey(cinema.CinemaID))
						{
							cinema.Auditoriums = new List<Auditorium>();
							cinema.Address = address;
							address.CityZipCode = cityZipcode;
							cinemaDictionary.Add(cinema.CinemaID, cinema);
						}

						var existingCinema = cinemaDictionary[cinema.CinemaID];

						if (auditorium != null &&
							!existingCinema.Auditoriums.Any(a => a.AuditoriumID == auditorium.AuditoriumID))
						{
							existingCinema.Auditoriums.Add(auditorium);
						}

						return existingCinema;
					},
					new { cinemaIDQuery = cinemaID },
					splitOn: "addressID,cityZipCodeID,auditoriumID"
				);

				return cinemaDictionary.Values.FirstOrDefault();
			}
		}

		public async Task<Seat?> GetSeatByIDAsync(int seatID)
		{
			string sql = """
				SELECT 
				s.seatID, s.rowNo, s.seatNo, s.seatType, s.auditoriumID
				FROM Seat s
				WHERE s.seatID = @seatIDQuery
				""";
			using (var connection = new SqlConnection(_connectionString))
			{
				var result = await connection.QuerySingleOrDefaultAsync<Seat>(
					sql,
					new { seatIDQuery = seatID }
				);
				return result;
			}
		}

		public async Task<IEnumerable<CityZipcode>> GetAllCitiesAsync()
		{
			string sql = """
				SELECT
				cityZipcodeID, zipcode, city
				FROM cityZipcode
				""";

			using (var connection = new SqlConnection(_connectionString))
			{
				var result = await connection.QueryAsync<CityZipcode>(sql);
				return result;
			}
		}

		public async Task<CityZipcode> GetCityByNameAsync(string cityName)
		{
			string sql = """
				SELECT
				cityZipcodeID, zipcode, city
				FROM cityZipcode
				WHERE city = @cityQuery
				""";
			using (var connection = new SqlConnection(_connectionString))
			{
				var result = await connection.QuerySingleAsync<CityZipcode>(
					sql,
					new { cityQuery = cityName }
				);
				return result;
			}
		}
	}
}
