using CinemaAPI.Core.DatabaseLayer;
using CinemaAPI.Core.DatabaseLayer.Interfaces;
using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;
using Xunit;

namespace CinemaAPI.Tests.DatabaseTesting
{
	[Collection("Database Tests")]
	public class ShowtimeAccessTests : IDisposable
	{
		private readonly ConnectionHandler _handler;
		private readonly ShowtimeAccess _showtimeAccess;
		private readonly string _connectionString;

		public ShowtimeAccessTests(ConnectionHandler handler)
		{
			_handler = handler;
			_connectionString = _handler.ConnectionString;
			_showtimeAccess = new ShowtimeAccess(_handler.Configuration);

			SeedTestData();
		}

		[Fact]
		public async Task GetAllShowtimesAsync_WithValidCity_ReturnsAllCityShowtimes()
		{
			// Arrange
			var city = "Test City";

			// Act
			var result = await _showtimeAccess.GetAllShowtimesByCityAsync(city);

			// Assert
			Assert.NotNull(result);
			Assert.NotEmpty(result);
		}

		[Fact]
		public async Task GetAllShowtimesAsync_WithInvalidCity_ReturnsEmptyList()
		{
			// Arrange
			var invalidCity = "Invalid City";

			// Act
			var result = await _showtimeAccess.GetAllShowtimesByCityAsync(invalidCity);

			// Assert
			Assert.Empty(result);
		}

		[Fact]
		public async Task GetMovieShowtimesAsync_WithValidMovieIDAndCity_ReturnsOnlyThatMoviesShowtimes()
		{
			// Arrange
			var city = "Test City";
			int movieId = 1;

			// Act
			var result = await _showtimeAccess.GetMovieShowtimesAsync(movieId, city);

			// Assert
			Assert.NotNull(result);
			Assert.NotEmpty(result);

			foreach (var showtime in result)
			{
				Assert.Equal(movieId, showtime.MovieID);
			}
		}

		[Fact]
		public async Task GetMovieShowtimesAsync_WithInvalidMovieID_ReturnsEmpty()
		{
			// Arrange
			var city = "Test City";
			int invalidMovieId = 99999;

			// Act
			var result = await _showtimeAccess.GetMovieShowtimesAsync(invalidMovieId, city);

			// Assert
			Assert.Empty(result);
		}

		[Fact]
		public async Task GetShowtimeByIDAsync_WithValidID_ReturnsShowtime()
		{
			// Arrange
			int validShowtimeId = 1;

			// Act
			var result = await _showtimeAccess.GetShowtimeByIDAsync(validShowtimeId);

			// Assert
			Assert.NotNull(result);
			Assert.Equal(validShowtimeId, result.ShowtimeID);
			Assert.NotEmpty(result.SeatAvailability);
			Assert.NotNull(result.Movie);
		}

		[Fact]
		public async Task GetShowtimeByIDAsync_WithInvalidID_ReturnsNull()
		{
			// Arrange
			int invalidShowtimeId = 99999;

			// Act
			var result = await _showtimeAccess.GetShowtimeByIDAsync(invalidShowtimeId);

			// Assert
			Assert.Null(result);
		}

		private void SeedTestData()
		{
			using var connection = new SqlConnection(_connectionString);
			connection.Open();
			connection.Execute("DELETE FROM ShowtimeSeat");
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
                VALUES (1, 'Bio1', 2, 20, 1), (2, 'Bio2', 2, 18, 1)
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
                UPDATE ShowtimeSeat 
                SET Status = 1
                WHERE ShowtimeID = 1 AND SeatID = 3
            ");

			connection.Execute(@"
                UPDATE ShowtimeSeat 
                SET Status = 1
                WHERE ShowtimeID = 2 AND SeatID = 22
            ");
		}

		public void Dispose()
		{
			using var connection = new SqlConnection(_connectionString);
			connection.Execute("DELETE FROM ShowtimeSeat");
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
		}
	}
}