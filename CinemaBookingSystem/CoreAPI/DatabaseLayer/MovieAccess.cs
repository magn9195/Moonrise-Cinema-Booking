using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CinemaAPI.Core.DatabaseLayer.Interfaces;
using CinemaAPI.Core.Models;
using CinemaAPI.Core.Models.Enum;
using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace CinemaAPI.Core.DatabaseLayer
{
	public class MovieAccess : IMovieAccess
	{
		private readonly string _connectionString = "";

		public MovieAccess(IConfiguration configuration)
		{
			var connectionString = configuration.GetConnectionString("DefaultConnection");
			if (connectionString != null && !string.IsNullOrWhiteSpace(connectionString))
			{
				_connectionString = connectionString;
			}
		}

		public async Task<IEnumerable<Movie>?> GetAllMoviesAsync(string? city, string? genre, LanguageEnum? language, string? age)
		{
			string sql = """
				SELECT
				    M.movieID, M.title, M.releaseDate, M.ageLimit, M.duration, 
				    M.trailerYouTubeID, M.resume, M.language, M.subtitles, 
				    M.director, M.mainActor,
				    MI.imageID, MI.movieID,
				    G.genreID, G.name
				FROM Movie M 
				LEFT JOIN MovieImage MI ON M.movieID = MI.movieID
				JOIN MovieGenre MG ON M.movieID = MG.movieID
				JOIN Genre G ON MG.genreID = G.genreID
				LEFT JOIN Showtime ST ON M.movieID = ST.movieID
				LEFT JOIN Auditorium AU ON ST.auditoriumID = AU.auditoriumID
				LEFT JOIN Cinema C ON AU.cinemaID = C.cinemaID
				LEFT JOIN Address AD ON C.addressID = AD.addressID
				LEFT JOIN CityZipcode CZ ON AD.cityZipCodeID = CZ.cityZipCodeID
				WHERE 
					M.movieID IN (
						SELECT M.movieID
						FROM Movie M
						JOIN MovieGenre MG ON M.movieID = MG.movieID
						JOIN Genre G ON MG.genreID = G.genreID
						WHERE (@genreQuery IS NULL OR G.name = @genreQuery)
					) AND
					(@cityQuery IS NULL OR CZ.city = @cityQuery) AND
					(@languageQuery IS NULL OR M.language = @languageQuery) AND
					(@ageQuery IS NULL OR M.ageLimit <= @ageQuery)
				ORDER BY M.movieID
				""";

			using (var connection = new SqlConnection(_connectionString))
			{
				var movieDictionary = new Dictionary<int, Movie>();
				await connection.QueryAsync<Movie, MovieImage, Genre, Movie>(
					sql,
					(movie, movieImage, genre) =>
					{
						// If the movie is not already in the dictionary, add it
						if (!movieDictionary.ContainsKey(movie.MovieID))
						{
							movie.MovieImages = new List<MovieImage>();
							movie.Genres = new List<Genre>();
							movieDictionary.Add(movie.MovieID, movie);
						}

						// Get the current context movie from the dictionary
						var existingMovie = movieDictionary[movie.MovieID];

						// Add movie image if not already added
						if (movieImage != null && movieImage.ImageID >= 0)
						{
							if (!existingMovie.MovieImages.Any(i => i.ImageID == movieImage.ImageID))
							{
								existingMovie.MovieImages.Add(new MovieImage
								{
									ImageID = movieImage.ImageID,
									MovieID = movieImage.MovieID,
									ImageData = Array.Empty<byte>()
								});
							}
						}

						// Add genre if not already added
						if (genre != null && genre.GenreID >= 0)
						{
							if (!existingMovie.Genres.Any(g => g.GenreID == genre.GenreID))
							{
								existingMovie.Genres.Add(genre);
							}
						}
						return existingMovie;
					},
					param: new
					{
						cityQuery = city,
						genreQuery = genre,
						languageQuery = language,
						ageQuery = age
					},
					splitOn: "ImageID,GenreID"
					);
				return movieDictionary.Values;
			}
		}

		public async Task<Movie?> GetMovieByIDAsync(int movieID)
		{
			string sql = """
				SELECT 
				M.MovieID, M.Title, M.ReleaseDate, M.AgeLimit, M.Duration, M.TrailerYouTubeID, M.Resume, M.Language, M.Subtitles, M.Director, M.mainActor, 
				MI.ImageID, MI.MovieID, MG.GenreID, MG.MovieID, G.GenreID, G.Name FROM Movie M 
				LEFT JOIN MovieImage MI ON M.MovieID = MI.MovieID 
				LEFT JOIN MovieGenre MG ON M.movieID = MG.movieID 
				LEFT JOIN Genre G ON MG.genreID = G.genreID 
				WHERE M.MovieID = @MovieID
				ORDER BY M.movieID
				""";

			using (var connection = new SqlConnection(_connectionString))
			{
				Movie? foundMovie = null;

				await connection.QueryAsync<Movie, MovieImage, Genre, Movie>(
					sql,
					(movie, movieImage, genre) =>
					{
						// Initialize foundMovie only once
						if (foundMovie == null)
						{
							foundMovie = movie;
							foundMovie.MovieImages = new List<MovieImage>();
							foundMovie.Genres = new List<Genre>();
						}

						// Add movie images if they are not already added
						if (movieImage != null && movieImage.ImageID >= 0)
						{
							if (!foundMovie.MovieImages.Any(i => i.ImageID == movieImage.ImageID))
							{
								foundMovie.MovieImages.Add(new MovieImage
								{
									ImageID = movieImage.ImageID,
									MovieID = movieImage.MovieID,
									ImageData = Array.Empty<byte>()
								});
							}
						}

						// Add genres if they are not already added
						if (genre != null && genre.GenreID >= 0)
						{
							if (!foundMovie.Genres.Any(g => g.GenreID == genre.GenreID))
							{
								foundMovie.Genres.Add(genre);
							}
						}

						return foundMovie;
					},
					param: new { MovieID = movieID },
					splitOn: "ImageID,GenreID"
					);

				return foundMovie;
			}
		}

		public async Task<int> GetMovieImageCountAsync(int movieID)
		{
			string sql = "SELECT COUNT(*) FROM MovieImage WHERE MovieID = @MovieID";

			using (var connection = new SqlConnection(_connectionString))
			{
				return await connection.ExecuteScalarAsync<int>(sql, new { MovieID = movieID });
			}
		}

		public async Task<byte[]?> GetMovieImageByIndexAsync(int movieID, int imageIndex)
		{
			string sql = """
				SELECT ImageData 
				FROM MovieImage
				WHERE MovieID = @MovieID AND ImageIndex = @ImageIndex
				""";

			using (var connection = new SqlConnection(_connectionString))
			{
				return await connection.QuerySingleOrDefaultAsync<byte[]>(
					sql,
					new { MovieID = movieID, ImageIndex = imageIndex }
				);
			}
		}

		public async Task<List<MovieImage>> GetMovieImageIdsAsync(int movieID)
		{
			string sql = "SELECT ImageID, ImageIndex FROM MovieImage WHERE MovieID = @MovieID ORDER BY ImageID";

			using (var connection = new SqlConnection(_connectionString))
			{
				var result = await connection.QueryAsync<MovieImage>(sql, new { MovieID = movieID });
				return result.ToList();
			}
		}

		public async Task<Movie> CreateMovieAsync(Movie movie)
		{
			string insertMovieSql = """
				INSERT INTO Movie (Title, ReleaseDate, AgeLimit, Duration, TrailerYouTubeID, Resume, Language, Subtitles, Director, MainActor)
				OUTPUT INSERTED.MovieID
				VALUES (@Title, @ReleaseDate, @AgeLimit, @Duration, @TrailerYouTubeID, @Resume, @Language, @Subtitles, @Director, @MainActor)
				""";

			string getOrCreateGenreSql = """
				IF NOT EXISTS (SELECT 1 FROM Genre WHERE Name = @Name)
				BEGIN
					INSERT INTO Genre (Name)
					VALUES (@Name)
				END
				SELECT GenreID FROM Genre WHERE Name = @Name
				""";

			string insertMovieGenreSql = """
				INSERT INTO MovieGenre (MovieID, GenreID)
				VALUES (@MovieID, @GenreID)
				""";

			string insertMovieImageSql = """
				INSERT INTO MovieImage (MovieID, ImageData)
				OUTPUT INSERTED.ImageID
				VALUES (@MovieID, @ImageData)
				""";

			using (var connection = new SqlConnection(_connectionString))
			{
				await connection.OpenAsync();
				using (var transaction = connection.BeginTransaction())
				{
					try
					{
						// Insert the movie
						var movieID = await connection.ExecuteScalarAsync<int>(insertMovieSql, new
						{
							movie.Title,
							movie.ReleaseDate,
							movie.AgeLimit,
							movie.Duration,
							movie.TrailerYoutubeID,
							movie.Resume,
							movie.Language,
							movie.Subtitles,
							movie.Director,
							movie.MainActor
						}, transaction);

						movie.MovieID = movieID;

						// Handle genres if they exist
						if (movie.Genres != null && movie.Genres.Any())
						{
							foreach (var genre in movie.Genres)
							{
								// Get existing genre or create new one
								var genreID = await connection.ExecuteScalarAsync<int>(
									getOrCreateGenreSql,
									new { genre.Name },
									transaction
								);

								genre.GenreID = genreID;

								// Create the many-to-many relationship
								await connection.ExecuteAsync(
									insertMovieGenreSql,
									new { MovieID = movieID, GenreID = genreID },
									transaction
								);
							}
						}

						if (movie.MovieImages != null && movie.MovieImages.Any())
						{
							foreach (var image in movie.MovieImages)
							{
								// Insert the image and get the generated ImageID
								var imageID = await connection.ExecuteScalarAsync<int>(
									insertMovieImageSql,
									new { MovieID = movieID, ImageData = image.ImageData },
									transaction
								);
								image.ImageID = imageID;
								image.MovieID = movieID;
							}
						}

						transaction.Commit();
						return movie;
					}
					catch
					{
						transaction.Rollback();
						throw;
					}
				}
			}
		}

		public async Task<bool> DeleteMovieAsync(int movieID)
		{
			string sqlDeleteMovie = """
				DELETE FROM Movie
				WHERE MovieID = @MovieID
				""";

			string sqlDeleteOrphanedGenres = """
				DELETE FROM Genre
				WHERE GenreID NOT IN (SELECT GenreID FROM MovieGenre)
				""";

			using (var connection = new SqlConnection(_connectionString))
			{
				await connection.OpenAsync();
				using (var transaction = connection.BeginTransaction())
				{
					try
					{
						// Delete the movie (cascade will handle MovieGenre)
						var rowsAffected = await connection.ExecuteAsync(sqlDeleteMovie, new { MovieID = movieID }, transaction);

						if (rowsAffected > 0)
						{
							// Clean up any orphaned genres
							await connection.ExecuteAsync(sqlDeleteOrphanedGenres, transaction: transaction);

							transaction.Commit();
							return true;
						}

						transaction.Rollback();
						return false;
					}
					catch
					{
						transaction.Rollback();
						throw;
					}
				}
			}
		}

		public async Task<Movie> UpdateMovieAsync(Movie movie)
		{
			string sqlUpdateMovie = """
				UPDATE Movie
				SET Title = @Title,
					ReleaseDate = @ReleaseDate,
					AgeLimit = @AgeLimit,
					Duration = @Duration,
					TrailerYouTubeID = @TrailerYouTubeID,
					Resume = @Resume,
					Language = @Language,
					Subtitles = @Subtitles,
					Director = @Director,
					MainActor = @MainActor
				WHERE MovieID = @MovieID
				""";

			string deleteMovieGenresSql = """
				DELETE FROM MovieGenre
				WHERE MovieID = @MovieID
				""";

			string deleteMovieImagesSql = """
				DELETE FROM MovieImage
				WHERE MovieID = @MovieID
				""";

			string getOrCreateGenreSql = """
				IF NOT EXISTS (SELECT 1 FROM Genre WHERE Name = @Name)
				BEGIN
					INSERT INTO Genre (Name)
					VALUES (@Name)
				END
				SELECT GenreID FROM Genre WHERE Name = @Name
				""";

			string insertMovieGenreSql = """
				INSERT INTO MovieGenre (MovieID, GenreID)
				VALUES (@MovieID, @GenreID)
				""";

			string insertMovieImageSql = """
				INSERT INTO MovieImage (MovieID, ImageData)
				OUTPUT INSERTED.ImageID
				VALUES (@MovieID, @ImageData)
				""";

			string deleteOrphanedGenresSql = """
				DELETE FROM Genre
				WHERE GenreID NOT IN (SELECT GenreID FROM MovieGenre)
				""";

			using (var connection = new SqlConnection(_connectionString))
			{
				await connection.OpenAsync();
				using (var transaction = connection.BeginTransaction())
				{
					try
					{
						// Update the movie
						await connection.ExecuteAsync(sqlUpdateMovie, new
						{
							movie.Title,
							movie.ReleaseDate,
							movie.AgeLimit,
							movie.Duration,
							movie.TrailerYoutubeID,
							movie.Resume,
							movie.Language,
							movie.Subtitles,
							movie.Director,
							movie.MainActor,
							movie.MovieID
						}, transaction);

						// Delete existing genre relationships
						await connection.ExecuteAsync(deleteMovieGenresSql, new { movie.MovieID }, transaction);

						// Handle genres if they exist
						if (movie.Genres != null && movie.Genres.Any())
						{
							foreach (var genre in movie.Genres)
							{
								// Get existing genre or create new one
								var genreID = await connection.ExecuteScalarAsync<int>(
									getOrCreateGenreSql,
									new { genre.Name },
									transaction
								);
								genre.GenreID = genreID;

								// Create the many-to-many relationship
								await connection.ExecuteAsync(
									insertMovieGenreSql,
									new { MovieID = movie.MovieID, GenreID = genreID },
									transaction
								);
							}
						}

						// Clean up orphaned genres
						await connection.ExecuteAsync(deleteOrphanedGenresSql, transaction: transaction);

						// Delete existing images
						await connection.ExecuteAsync(deleteMovieImagesSql, new { movie.MovieID }, transaction);

						// Handle images if they exist
						if (movie.MovieImages != null && movie.MovieImages.Any())
						{
							foreach (var image in movie.MovieImages)
							{
								// Insert the image and get the generated ImageID
								var imageID = await connection.ExecuteScalarAsync<int>(
									insertMovieImageSql,
									new { MovieID = movie.MovieID, ImageData = image.ImageData },
									transaction
								);
								image.ImageID = imageID;
								image.MovieID = movie.MovieID;
							}
						}

						transaction.Commit();
						return movie;
					}
					catch
					{
						transaction.Rollback();
						throw;
					}
				}
			}
		}

		public async Task<List<string>> GetGenresFromCityAsync(string city)
		{
			string sql = """
				SELECT DISTINCT G.name
				FROM Genre G
				JOIN MovieGenre MG ON G.genreID = MG.genreID
				JOIN Movie M ON MG.movieID = M.movieID
				JOIN Showtime ST ON M.movieID = ST.movieID
				JOIN Auditorium AU ON ST.auditoriumID = AU.auditoriumID
				JOIN Cinema C ON AU.cinemaID = C.cinemaID
				JOIN Address AD ON C.addressID = AD.addressID
				JOIN CityZipcode CZ ON AD.cityZipCodeID = CZ.cityZipCodeID
				WHERE CZ.city = @cityQuery
				""";

			using (var connection = new SqlConnection(_connectionString))
			{
				var result = await connection.QueryAsync<string>(sql, new { cityQuery = city });
				return result.ToList();
			}
		}

		public async Task<List<LanguageEnum>> GetMovieLanguagesFromCityAsync(string city)
		{
			string sql = """
				SELECT DISTINCT M.language
				FROM Movie M
				JOIN Showtime ST ON M.movieID = ST.movieID
				JOIN Auditorium AU ON ST.auditoriumID = AU.auditoriumID
				JOIN Cinema C ON AU.cinemaID = C.cinemaID
				JOIN Address AD ON C.addressID = AD.addressID
				JOIN CityZipcode CZ ON AD.cityZipCodeID = CZ.cityZipCodeID
				WHERE CZ.city = @cityQuery
				""";

			using (var connection = new SqlConnection(_connectionString))
			{
				var result = await connection.QueryAsync<LanguageEnum>(sql, new { cityQuery = city });
				return result.ToList();
			}
		}
	}
}
