using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CinemaAPI.Core.DatabaseLayer.Interfaces;
using CinemaAPI.Core.Models;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Dapper;

namespace CinemaAPI.Core.DatabaseLayer
{
	public class ShowtimeAccess : IShowtimeAccess
	{
		private readonly string _connectionString = "";

		public ShowtimeAccess(IConfiguration configuration)
		{
			var connectionString = configuration.GetConnectionString("DefaultConnection");
			if(connectionString != null && !string.IsNullOrWhiteSpace(connectionString))
			{
				_connectionString = connectionString;
			}
		}

		public async Task<IEnumerable<Showtime>?> GetAllShowtimesByCityAsync(string city)
		{
			string sql = """
				SELECT 
				SH.ShowtimeID, SH. Showtype, SH.StartTime, SH.MovieID, SH.AuditoriumID, 
				AU.AuditoriumID, AU.Name, AU. RowNum, AU.CinemaID,
				M.MovieID, M.Title, M.ReleaseDate, M.AgeLimit, M.Duration, M.TrailerYouTubeID, M.Resume, M.Language, M.Subtitles, M.Director, M.MainActor, 
				MI.ImageID, MI.MovieID,
				G.GenreID, G.Name
				FROM Showtime SH 
				JOIN Auditorium AU ON SH. AuditoriumID = AU. AuditoriumID
				JOIN Movie M ON SH.MovieID = M.MovieID
				LEFT JOIN MovieImage MI ON M.MovieID = MI.MovieID
				LEFT JOIN MovieGenre MG ON M.MovieID = MG. MovieID 
				LEFT JOIN Genre G ON MG.GenreID = G.GenreID
				LEFT JOIN Cinema C ON AU.CinemaID = C.CinemaID
				LEFT JOIN Address AD ON C.AddressID = AD.AddressID
				LEFT JOIN CityZipCode CZ ON AD.CityZipCodeID = CZ. CityZipCodeID
				WHERE CZ.City = @cityQuery
				ORDER BY SH.ShowtimeID
				""";

			using (var connection = new SqlConnection(_connectionString))
			{
				var showtimeDictionary = new Dictionary<int, Showtime>();

				await connection.QueryAsync<Showtime, Auditorium, Movie, MovieImage, Genre, Showtime>(
					sql,
					(showtime, auditorium, movie, movieImage, genre) =>
					{
						if (!showtimeDictionary.ContainsKey(showtime.ShowtimeID))
						{
							showtime.Auditorium = auditorium;
							showtimeDictionary.Add(showtime.ShowtimeID, showtime);
						}

						var existingShowtime = showtimeDictionary[showtime.ShowtimeID];

						if (existingShowtime.Movie == null)
						{
							existingShowtime.Movie = movie;
							existingShowtime.Movie.MovieImages = new List<MovieImage>();
							existingShowtime.Movie.Genres = new List<Genre>();
						}

						if (movieImage != null && movieImage.ImageID >= 0)
						{
							if (!existingShowtime.Movie.MovieImages.Any(i => i.ImageID == movieImage.ImageID))
							{
								existingShowtime.Movie.MovieImages.Add(new MovieImage
								{
									ImageID = movieImage.ImageID,
									MovieID = movieImage.MovieID,
									ImageData = Array.Empty<byte>()
								});
							}
						}

						if (genre != null && genre.GenreID >= 0)
						{
							if (!existingShowtime.Movie.Genres.Any(g => g.GenreID == genre.GenreID))
							{
								existingShowtime.Movie.Genres.Add(genre);
							}
						}

						return existingShowtime;
					}, param: new
					{
						cityQuery = city
					},
					splitOn: "AuditoriumID,MovieID,ImageID,GenreID"
				);

				return showtimeDictionary.Values;
			}
		}

		public async Task<IEnumerable<Showtime>?> GetShowtimesByAuditoriumDateAsync(int auditoriumID, DateTime startDate, DateTime endDate)
		{
			string sql = """
				SELECT 
				SH.ShowtimeID, SH.Showtype, SH.StartTime, SH.MovieID, SH.AuditoriumID, 
				SS.SeatID, SS.ShowtimeID, SS.Status, SS.TicketID, 
				SE.SeatID, SE.RowNo, SE.SeatNo, SE.SeatType,
				AU.AuditoriumID, AU.Name, AU.RowNum, AU.CinemaID,
				M.MovieID, M.Title, M.ReleaseDate, M.AgeLimit, M.Duration, M.TrailerYouTubeID, M.Resume, M.Language, M.Subtitles, M.Director, M.MainActor, 
				MI.ImageID, MI.ImageData, 
				G.GenreID, G.Name
				FROM Showtime SH 
				JOIN ShowtimeSeat SS ON SH.ShowtimeID = SS.ShowtimeID 
				JOIN Seat SE ON SS.SeatID = SE.SeatID
				JOIN Auditorium AU ON SH.auditoriumID = AU.auditoriumID
				JOIN Movie M ON SH.MovieID = M.MovieID
				LEFT JOIN MovieImage MI ON M.MovieID = MI.MovieID
				LEFT JOIN MovieGenre MG ON M.MovieID = MG.MovieID 
				LEFT JOIN Genre G ON MG.GenreID = G.GenreID
				WHERE SH.auditoriumID = @auditoriumID 
				AND CAST(SH.startTime AS DATE) BETWEEN @startDate AND @endDate
				ORDER BY SH.startTime
				""";

			using (var connection = new SqlConnection(_connectionString))
			{
				var showtimeDictionary = new Dictionary<int, Showtime>();
				await connection.QueryAsync<Showtime, ShowtimeSeat, Seat, Auditorium, Movie, MovieImage, Genre, Showtime>(
					sql,
					(showtime, showtimeSeat, seat, auditorium, movie, movieImage, genre) =>
					{
						if (!showtimeDictionary.ContainsKey(showtime.ShowtimeID))
						{
							showtime.SeatAvailability = new List<ShowtimeSeat>();
							showtimeDictionary.Add(showtime.ShowtimeID, showtime);
						}

						var existingShowtime = showtimeDictionary[showtime.ShowtimeID];

						showtimeSeat.Seat = seat;
						if (!existingShowtime.SeatAvailability.Any(s => s.Seat.SeatID == showtimeSeat.Seat.SeatID))
						{
							existingShowtime.SeatAvailability.Add(showtimeSeat);
						}

						existingShowtime.Auditorium = auditorium;

						if (existingShowtime.Movie == null)
						{
							existingShowtime.Movie = movie;
							existingShowtime.Movie.MovieImages = new List<MovieImage>();
							existingShowtime.Movie.Genres = new List<Genre>();
						}

						if (movieImage != null && movieImage.ImageID >= 0)
						{
							existingShowtime.Movie.MovieImages.Add(new MovieImage
							{
								ImageID = movieImage.ImageID,
								MovieID = movieImage.MovieID,
								ImageData = Array.Empty<byte>()
							});
						}

						if (genre != null && genre.GenreID >= 0)
						{
							if (!existingShowtime.Movie.Genres.Any(g => g.GenreID == genre.GenreID))
							{
								existingShowtime.Movie.Genres.Add(genre);
							}
						}

						return existingShowtime;
					},
					param: new
					{
						AuditoriumID = auditoriumID,
						StartDate = startDate.Date,
						EndDate = endDate.Date
					},
					splitOn: "SeatID,SeatID,AuditoriumID,MovieID,ImageID,GenreID"
				);
				return showtimeDictionary.Values;
			}
		}

		public async Task<IEnumerable<Showtime>?> GetMovieShowtimesAsync(int movieID, string? city)
		{
			string sql = """
				SELECT 
				SH.ShowtimeID, SH.Showtype, SH.StartTime, SH.MovieID, SH.AuditoriumID, 
				AU.AuditoriumID, AU.Name, AU.RowNum, AU.CinemaID,
				M.MovieID, M.Title, M.ReleaseDate, M.AgeLimit, M.Duration, M.TrailerYouTubeID, M.Resume, M.Language, M.Subtitles, M.Director, M.MainActor, 
				MI.ImageID, MI.ImageData, MI.ImageIndex,
				G.GenreID, G.Name
				FROM Showtime SH 
				JOIN Auditorium AU ON SH.AuditoriumID = AU.AuditoriumID
				JOIN Movie M ON SH. MovieID = M.MovieID
				LEFT JOIN MovieImage MI ON M.MovieID = MI.MovieID
				LEFT JOIN MovieGenre MG ON M.MovieID = MG.MovieID 
				LEFT JOIN Genre G ON MG.GenreID = G.GenreID
				LEFT JOIN Cinema C ON AU.CinemaID = C. CinemaID
				LEFT JOIN Address AD ON C.AddressID = AD.AddressID
				LEFT JOIN CityZipCode CZ ON AD.CityZipCodeID = CZ.CityZipCodeID
				WHERE SH.MovieID = @MovieID 
				AND (@cityQuery IS NULL OR CZ.City = @cityQuery)
				ORDER BY SH.ShowtimeID
				""";

			using (var connection = new SqlConnection(_connectionString))
			{
				var showtimeDictionary = new Dictionary<int, Showtime>();

				await connection.QueryAsync<Showtime, Auditorium, Movie, MovieImage, Genre, Showtime>(
					sql,
					(showtime, auditorium, movie, movieImage, genre) =>
					{
						if (!showtimeDictionary.ContainsKey(showtime.ShowtimeID))
						{
							showtime.Auditorium = auditorium;
							showtimeDictionary.Add(showtime.ShowtimeID, showtime);
						}

						var existingShowtime = showtimeDictionary[showtime.ShowtimeID];

						if (movie != null && movie.MovieID >= 0)
						{
							if (existingShowtime.Movie == null)
							{
								existingShowtime.Movie = movie;
								existingShowtime.Movie.MovieImages = new List<MovieImage>();
								existingShowtime.Movie.Genres = new List<Genre>();
							}

							if (movieImage != null && movieImage.ImageID >= 0)
							{
								if (!existingShowtime.Movie.MovieImages.Any(i => i.ImageID == movieImage.ImageID))
								{
									existingShowtime.Movie.MovieImages.Add(new MovieImage
									{
										ImageID = movieImage.ImageID,
										MovieID = movieImage.MovieID,
										ImageData = Array.Empty<byte>()
									});
								}
							}

							if (genre != null && genre.GenreID >= 0)
							{
								if (!existingShowtime.Movie.Genres.Any(g => g.GenreID == genre.GenreID))
								{
									existingShowtime.Movie.Genres.Add(genre);
								}
							}
						}

						return existingShowtime;
					},
					param: new
					{
						MovieID = movieID,
						cityQuery = city
					},
					splitOn: "AuditoriumID,MovieID,ImageID,GenreID"
				);

				return showtimeDictionary.Values;
			}
		}

		public async Task<Showtime?> GetShowtimeByIDAsync(int showtimeID)
		{
			string sql = """
				SELECT 
				SH.ShowtimeID, SH.Showtype, SH.StartTime, SH.MovieID, SH.AuditoriumID, 
				SS.SeatID, SS.ShowtimeID, SS.Status, SS.TicketID, 
				SE.SeatID, SE.RowNo, SE.SeatNo, SE.SeatType,
				A.AuditoriumID, A.Name, A.RowNum, A.CinemaID,
				M.MovieID, M.Title, M.ReleaseDate, M.AgeLimit, M.Duration, M.TrailerYouTubeID, M.Resume, M.Language, M.Subtitles, M.Director, M.MainActor, 
				MI.ImageID, MI.ImageData, 
				G.GenreID, G.Name
				FROM Showtime SH 
				JOIN ShowtimeSeat SS ON SH.ShowtimeID = SS.ShowtimeID 
				JOIN Seat SE ON SS.SeatID = SE.SeatID
				JOIN Auditorium A ON SH.auditoriumID = A.auditoriumID
				JOIN Movie M ON SH.MovieID = M.MovieID
				LEFT JOIN MovieImage MI ON M.MovieID = MI.MovieID
				LEFT JOIN MovieGenre MG ON M.MovieID = MG.MovieID 
				LEFT JOIN Genre G ON MG.GenreID = G.GenreID
				WHERE SH.ShowtimeID = @ShowtimeID 
				ORDER BY SH.ShowtimeID, SE.RowNo, SE.SeatNo
				""";

			using (var connection = new SqlConnection(_connectionString))
			{
				Showtime? currentShowtime = null;

				await connection.QueryAsync<Showtime, ShowtimeSeat, Seat, Auditorium, Movie, MovieImage, Genre, Showtime>(
					sql,
					(showtime, showtimeSeat, seat, auditorium, movie, movieImage, genre) =>
					{
						if (currentShowtime == null)
						{
							currentShowtime = showtime;
							currentShowtime.SeatAvailability = new List<ShowtimeSeat>();
						}

						showtimeSeat.Seat = seat;
						if (!currentShowtime.SeatAvailability.Any(s => s.Seat.SeatID == showtimeSeat.Seat.SeatID))
						{
							currentShowtime.SeatAvailability.Add(showtimeSeat);
						}

						currentShowtime.Auditorium = auditorium;

						if (movie != null && movie.MovieID >= 0)
						{
							if (currentShowtime.Movie == null)
							{
								currentShowtime.Movie = movie;
								currentShowtime.Movie.MovieImages = new List<MovieImage>();
								currentShowtime.Movie.Genres = new List<Genre>();
							}

							if (movieImage != null && movieImage.ImageID >= 0)
							{
								currentShowtime.Movie.MovieImages.Add(new MovieImage
								{
									ImageID = movieImage.ImageID,
									MovieID = movieImage.MovieID,
									ImageData = Array.Empty<byte>()
								});
							}

							if (genre != null && genre.GenreID >= 0)
							{
								if (!currentShowtime.Movie.Genres.Any(g => g.GenreID == genre.GenreID))
								{
									currentShowtime.Movie.Genres.Add(genre);
								}
							}
						}

						return currentShowtime;
					},
					param: new { ShowtimeID = showtimeID },
					splitOn: "SeatID,SeatID,AuditoriumID,MovieID,ImageID,GenreID"
				);

				return currentShowtime;
			}
		}

		public async Task<ShowtimeSeat?> GetShowtimeSeatByIDAsync(int showtimeID, int seatID)
		{

			string sql = """
				SELECT 
				SS.SeatID, SS.ShowtimeID, SS.Status, SS.TicketID, 
				SE.SeatID, SE.RowNo, SE.SeatNo, SE.SeatType, SE.AuditoriumID,
				T.TicketID, T.Price, T.CustomerID, T.PurchaseDate, T.ExpireDate,
				C.CustomerID, C.Name, C.Email, C.PhoneNo
				FROM ShowtimeSeat SS 
				JOIN Seat SE ON SS.SeatID = SE.SeatID
				LEFT JOIN Ticket T ON SS.TicketID = T.TicketID
				LEFT JOIN Customer C ON T.CustomerID = C.CustomerID
				WHERE SS.ShowtimeID = @ShowtimeID AND SS.SeatID = @SeatID
				""";
			using (var connection = new SqlConnection(_connectionString))
			{
				var result = await connection.QueryAsync<ShowtimeSeat, Seat, Ticket, Customer, ShowtimeSeat>(
					sql,
					(showtimeSeat, seat, ticket, customer) =>
					{
						showtimeSeat.Seat = seat;
						if (ticket != null)
						{
							showtimeSeat.Ticket = ticket;
							showtimeSeat.Ticket.Customer = customer;
						}
						return showtimeSeat;
					},
					param: new { ShowtimeID = showtimeID, SeatID = seatID },
					splitOn: "SeatID,TicketID,CustomerID"
				);
				return result.FirstOrDefault();
			}
		}

		public async Task<Showtime> CreateShowtimeAsync(Showtime showtime)
		{
			string sql = """
				INSERT INTO Showtime (Showtype, StartTime, MovieID, AuditoriumID)
				VALUES (@Showtype, @StartTime, @MovieID, @AuditoriumID);
				SELECT CAST(SCOPE_IDENTITY() AS INT);
				""";

			using (var connection = new SqlConnection(_connectionString))
			{
				await connection.OpenAsync();
				using (var transaction = await connection.BeginTransactionAsync())
				{
					try
					{
						// Insert the showtime and get the generated ShowtimeID
						var showtimeID = await connection.ExecuteScalarAsync<int>(
							sql, new
							{
								showtime.ShowType,
								showtime.StartTime,
								showtime.MovieID,
								showtime.AuditoriumID
							}, transaction: transaction);
						showtime.ShowtimeID = showtimeID;

						await transaction.CommitAsync();
						var createdShowtime = await GetShowtimeByIDAsync(showtimeID);
						return await GetShowtimeByIDAsync(showtimeID);
					}
					catch
					{
						await transaction.RollbackAsync();
						throw;
					}
				}
			}
		}

		public async Task<bool> DeleteShowtimeAsync(int showtimeID)
		{
			string sql = """
				DELETE FROM Showtime
				WHERE ShowtimeID = @ShowtimeID;
				""";
			using (var connection = new SqlConnection(_connectionString))
			{
				var rowsAffected = await connection.ExecuteAsync(
					sql,
					new { ShowtimeID = showtimeID });
				return rowsAffected > 0;
			}
		}

		public async Task<Showtime> UpdateShowtimeAsync(Showtime showtime)
		{
			string sql = """
				UPDATE Showtime
				SET Showtype = @Showtype,
					StartTime = @StartTime,
					MovieID = @MovieID,
					AuditoriumID = @AuditoriumID
				WHERE ShowtimeID = @ShowtimeID;
				""";

			using (var connection = new SqlConnection(_connectionString))
			{

				await connection.ExecuteAsync(
					sql,
					new
					{
						showtime.ShowType,
						showtime.StartTime,
						showtime.MovieID,
						showtime.AuditoriumID,
						showtime.ShowtimeID
					});

				return await GetShowtimeByIDAsync(showtime.ShowtimeID);
			}
		}
	}
}
