using CinemaAPI.Core.DatabaseLayer;
using CinemaAPI.Core.Models;
using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;
using Xunit;

namespace CinemaAPI.Tests.DatabaseTesting
{
	[Collection("Database Tests")]
	public class MovieAccessTests : IDisposable
	{
		private readonly ConnectionHandler _handler;
		private readonly MovieAccess _movieAccess;
		private readonly string _connectionString;

		public MovieAccessTests(ConnectionHandler handler)
		{
			_handler = handler;
			_connectionString = _handler.ConnectionString;
			_movieAccess = new MovieAccess(_handler.Configuration);

			SeedTestData();
		}

		[Fact]
		public async Task GetAllMovies_ReturnsAllMovies()
		{
			// Act
			var result = await _movieAccess.GetAllMoviesAsync(null, null, null, null);

			// Assert
			Assert.NotNull(result);
			Assert.NotEmpty(result);
			Assert.True(result.Count() > 0);
		}

		[Fact]
		public async Task GetAllMoviesAsync_ReturnsMoviesWithGenres()
		{
			// Act
			var result = await _movieAccess.GetAllMoviesAsync(null, null, null, null);

			// Assert
			var movies = result.ToList();
			Assert.Contains(movies, m => m.Genres.Any());

			var movieWithGenres = movies.First(m => m.Genres.Any());
			Assert.NotEmpty(movieWithGenres.Genres);

			var genreIds = movieWithGenres.Genres.Select(g => g.GenreID).ToList();
			Assert.Equal(genreIds.Distinct().Count(), genreIds.Count);
		}

		[Fact]
		public async Task GetAllMoviesAsync_ReturnsMoviesWithImages()
		{
			// Act
			var result = await _movieAccess.GetAllMoviesAsync(null, null, null, null);

			// Assert
			var movies = result.ToList();
			Assert.Contains(movies, m => m.MovieImages.Any());

			var movieWithImages = movies.First(m => m.MovieImages.Any());
			Assert.NotEmpty(movieWithImages.MovieImages);

			// Check for unique images
			var imageIds = movieWithImages.MovieImages.Select(i => i.ImageID).ToList();
			Assert.Equal(imageIds.Distinct().Count(), imageIds.Count);
		}

		[Fact]
		public async Task GetAllMoviesAsync_DoesNotDuplicateMovies()
		{
			// Act
			var result = await _movieAccess.GetAllMoviesAsync(null, null, null, null);

			// Assert
			var movies = result.ToList();
			var movieIds = movies.Select(m => m.MovieID).ToList();
			Assert.Equal(movieIds.Distinct().Count(), movieIds.Count);
		}

		[Fact]
		public async Task GetMovieByIDAsync_WithValidID_ReturnsMovie()
		{
			// Arrange
			int validMovieId = 1; // Assumes ID 1 exists in test data

			// Act
			var result = await _movieAccess.GetMovieByIDAsync(validMovieId);

			// Assert
			Assert.NotNull(result);
			Assert.Equal(validMovieId, result.MovieID);
			Assert.NotNull(result.Title);
			Assert.NotEmpty(result.Title);
			Assert.NotNull(result.Genres);
			Assert.NotNull(result.MovieImages);
		}

		[Fact]
		public async Task GetMovieByIDAsync_WithInvalidID_ReturnsNull()
		{
			// Arrange
			int invalidMovieId = 99999;

			// Act
			var result = await _movieAccess.GetMovieByIDAsync(invalidMovieId);

			// Assert
			Assert.Null(result);
		}

		[Fact]
		public async Task GetMovieByIDAsync_VerifyAllPropertiesArePopulated()
		{
			// Arrange
			int validMovieId = 1;

			// Act
			var result = await _movieAccess.GetMovieByIDAsync(validMovieId);

			// Assert
			Assert.NotNull(result);
			Assert.NotNull(result.Title);
			Assert.NotEmpty(result.Title);
			Assert.NotNull(result.Director);
			Assert.NotEmpty(result.Director);
			Assert.NotNull(result.MainActor);
			Assert.NotEmpty(result.MainActor);
			Assert.NotNull(result.Resume);
			Assert.NotEmpty(result.Resume);
			Assert.NotNull(result.TrailerYoutubeID);
			Assert.NotEmpty(result.TrailerYoutubeID);
			Assert.True(result.Duration > 0);
			Assert.True(result.AgeLimit >= 0);
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
                WHERE ShowtimeID = 2 AND SeatID = 4
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