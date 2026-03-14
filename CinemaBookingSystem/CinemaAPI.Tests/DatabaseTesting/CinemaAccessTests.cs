using CinemaAPI.Core.DatabaseLayer;
using CinemaAPI.Core.DatabaseLayer.Interfaces;
using CinemaAPI.Core.Models;
using CinemaAPI.Core.Models.Enum;
using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;
using Xunit;

namespace CinemaAPI.Tests.DatabaseTesting
{
	[Collection("Database Tests")]
	public class CinemaAccessTests : IDisposable
	{
		private readonly ConnectionHandler _handler;
		private readonly CinemaAccess _cinemaAccess;
		private readonly string _connectionString;

		public CinemaAccessTests(ConnectionHandler handler)
		{
			_handler = handler;
			_connectionString = _handler.ConnectionString;
			_cinemaAccess = new CinemaAccess(_handler.Configuration);

			SeedTestData();
		}

		[Fact]
		public async Task GetAllCities_ReturnsAllCities()
		{
			// Act
			var result = await _cinemaAccess.GetAllCitiesAsync();

			// Assert
			Assert.NotNull(result);
			Assert.True(result.Any());
		}

		[Fact]
		public async Task GetAllCities_ReturnsBothVariables()
		{
			// Act
			var result = await _cinemaAccess.GetAllCitiesAsync();
			Assert.NotNull(result);
			var instance = result.FirstOrDefault();

			// Assert
			Assert.NotNull(instance.ZipCode);
			Assert.NotNull(instance.City);
		}

		[Fact]
		public async Task GetAllCinemasAsync_ReturnsAllCities()
		{
			// Act
			var result = await _cinemaAccess.GetAllCinemasAsync(null);

			// Assert
			Assert.NotNull(result);
			Assert.True(result.Any());
		}

		[Fact]
		public async Task GetAllCinemasAsync_ReturnsAllVariables()
		{
			// Act
			var result = await _cinemaAccess.GetAllCinemasAsync(null);
			Assert.NotNull(result);
			var instance = result.FirstOrDefault();

			// Assert
			Assert.NotNull(instance.Name);
			Assert.NotNull(instance.Address);
			Assert.NotNull(instance.Address.CityZipCode);
			Assert.NotEmpty(instance.Auditoriums);
		}

		[Fact]
		public async Task GetAllCinemasAsync_WithValidCity_ReturnsCinema()
		{
			// Arrange
			string validCity = "Test City";

			// Act
			var result = await _cinemaAccess.GetAllCinemasAsync(validCity);
			Assert.NotNull(result);
			var instance = result.FirstOrDefault();

			// Assert
			Assert.NotNull(instance.Name);
			Assert.NotNull(instance.Address);
			Assert.NotNull(instance.Address.CityZipCode);
			Assert.NotEmpty(instance.Auditoriums);
		}

		[Fact]
		public async Task GetAllCinemasAsync_WithInvalidCity_ReturnsNull()
		{
			// Arrange
			string invalidCity = "Not A City";

			// Act
			var result = await _cinemaAccess.GetAllCinemasAsync(invalidCity);

			// Assert
			Assert.Empty(result);
		}

		[Fact]
		public async Task GetCinemaByIDAsync_WithValidID_ReturnsCinema()
		{
			// Arrange
			int validCinemaId = 1;

			// Act
			var result = await _cinemaAccess.GetCinemaByIDAsync(validCinemaId);

			// Assert
			Assert.NotNull(result);
			Assert.Equal(validCinemaId, result.CinemaID);
		}

		[Fact]
		public async Task GetCinemaByIDAsync_WithInvalidID_ReturnsNull()
		{
			// Arrange
			int invalidCinemaId = 99999;

			// Act
			var result = await _cinemaAccess.GetCinemaByIDAsync(invalidCinemaId);

			// Assert
			Assert.Null(result);
		}

		[Fact]
		public async Task GetCinemaByIDAsync_VerifyAllPropertiesArePopulated()
		{
			// Arrange
			int validCinemaId = 1;

			// Act
			var result = await _cinemaAccess.GetCinemaByIDAsync(validCinemaId);

			// Assert
			Assert.NotNull(result);
			Assert.Equal(validCinemaId, result.CinemaID);
			Assert.NotNull(result.Name);
			Assert.NotNull(result.Address);
			Assert.NotNull(result.Address.CityZipCode);
			Assert.NotEmpty(result.Auditoriums);
		}

		[Fact]
		public async Task GetSeatByIDAsync_WithValidID_ReturnsSeat()
		{
			// Arrange
			int validSeatId = 1;

			// Act
			var result = await _cinemaAccess.GetSeatByIDAsync(validSeatId);

			// Assert
			Assert.NotNull(result);
			Assert.Equal(validSeatId, result.SeatID);
		}

		[Fact]
		public async Task GetSeatByIDAsync_WithInvalidID_ReturnsNull()
		{
			// Arrange
			int invalidSeatId = 99999;

			// Act
			var result = await _cinemaAccess.GetSeatByIDAsync(invalidSeatId);

			// Assert
			Assert.Null(result);
		}

		[Fact]
		public async Task GetSeatByIDAsync_VerifyAllPropertiesArePopulated()
		{
			// Arrange
			int validSeatId = 1;

			// Act
			var result = await _cinemaAccess.GetSeatByIDAsync(validSeatId);

			// Assert
			Assert.NotNull(result);
			Assert.True(result.RowNo > 0);
			Assert.True(result.SeatNo > 0);
			Assert.True(result.AuditoriumID > 0);
		}

		private void SeedTestData()
		{
			using var connection = new SqlConnection(_connectionString);
			connection.Open();
			connection.Execute("DELETE FROM ShowtimeSeat");
			connection.Execute("DELETE FROM TicketTypeQuantity");
			connection.Execute("DELETE FROM Ticket");
			connection.Execute("DELETE FROM Showtime");
			connection.Execute("DELETE FROM Seat");
			connection.Execute("DELETE FROM Auditorium");
			connection.Execute("DELETE FROM Cinema");
			connection.Execute("DELETE FROM Address");
			connection.Execute("DELETE FROM CityZipcode");
			connection.Execute("DELETE FROM MovieGenre");
			connection.Execute("DELETE FROM MovieImage");
			connection.Execute("DELETE FROM Movie");
			connection.Execute("DELETE FROM Genre");
			connection.Execute("DELETE FROM Customer");
			connection.Execute("DELETE FROM TicketType");

			connection.Execute("DBCC CHECKIDENT ('Seat', RESEED, 0)");
			connection.Execute("DBCC CHECKIDENT ('Ticket', RESEED, 0)");
			connection.Execute("DBCC CHECKIDENT ('Customer', RESEED, 0)");

			connection.Execute(@"
				INSERT INTO Genre (GenreID, Name)
				VALUES (1, 'Action'), (2, 'Drama'), (3, 'Comedy')
			");

			connection.Execute(@"
				INSERT INTO Movie (MovieID, Title, ReleaseDate, AgeLimit, Duration, 
					TrailerYouTubeID, Resume, Language, Subtitles, Director, MainActor)
				VALUES 
				(1, 'Test Movie 1', '2024-01-01', 13, 120, 'abc123', 'Test Resume 1', 0, 0, 'Director 1', 'Actor 1'),
				(2, 'Test Movie 2', '2024-02-01', 16, 150, 'def456', 'Test Resume 2', 1, 1, 'Director 2', 'Actor 2')
			");

			connection.Execute(@"
				INSERT INTO MovieGenre (MovieID, GenreID)
				VALUES (1, 1), (1, 2), (2, 2), (2, 3)
			");

			connection.Execute(@"
				INSERT INTO MovieImage (ImageID, MovieID, ImageData)
				VALUES 
				(1, 1, 0x89504E470D0A1A0A),
				(2, 1, 0x89504E470D0A1A0B),
				(3, 2, 0x89504E470D0A1A0C)
			");

			connection.Execute(@"
				INSERT INTO CityZipcode (CityZipCodeID, Zipcode, City)
				VALUES (1, '12345', 'Test City')
			");

			connection.Execute(@"
				INSERT INTO Address (AddressID, HouseNumber, StreetName, CityZipCodeID)
				VALUES (1, 123, 'Test Street', 1)
			");

			connection.Execute(@"
				INSERT INTO Cinema (CinemaID, Name, AddressID)
				VALUES (1, 'Test Cinema', 1)
			");

			connection.Execute(@"
				INSERT INTO Auditorium (AuditoriumID, Name, RowNum, seatsPerRow, CinemaID)
				VALUES (1, 'Bio1', 2, 20, 1), (2, 'Bio2', 2, 16, 1)
			");

			connection.Execute(@"
				INSERT INTO Seat (rowNo, seatNo, seatType, auditoriumID)
				VALUES 
				(1, 1, 1, 1),
				(1, 2, 1, 1),
				(1, 3, 1, 1),
				(1, 4, 1, 1),
				(1, 5, 1, 1)
			");

			connection.Execute(@"
				INSERT INTO Showtime (ShowtimeID, ShowType, StartTime, MovieID, AuditoriumID)
				VALUES 
				(1, 0, '2024-12-01 19:00', 1, 1),
				(2, 1, '2024-12-01 21:00', 1, 1),
				(3, 0, '2024-12-02 19:00', 2, 2)
			");

			connection.Execute(@"
				INSERT INTO TicketType (type, currentPrice, description)
				VALUES 
				(0, 140.0, 'Adult ticket'),
				(1, 100.0, 'Child ticket'),
				(2, 120.0, 'Student ticket'),
				(3, 120.0, 'Senior ticket')
			");
		}

		public void Dispose()
		{
			using var connection = new SqlConnection(_connectionString);
			connection.Execute("DELETE FROM ShowtimeSeat");
			connection.Execute("DELETE FROM TicketTypeQuantity");
			connection.Execute("DELETE FROM Ticket");
			connection.Execute("DELETE FROM Showtime");
			connection.Execute("DELETE FROM Seat");
			connection.Execute("DELETE FROM Auditorium");
			connection.Execute("DELETE FROM Cinema");
			connection.Execute("DELETE FROM Address");
			connection.Execute("DELETE FROM CityZipcode");
			connection.Execute("DELETE FROM MovieGenre");
			connection.Execute("DELETE FROM MovieImage");
			connection.Execute("DELETE FROM Movie");
			connection.Execute("DELETE FROM Genre");
			connection.Execute("DELETE FROM Customer");
			connection.Execute("DELETE FROM TicketType");
		}
	}
}