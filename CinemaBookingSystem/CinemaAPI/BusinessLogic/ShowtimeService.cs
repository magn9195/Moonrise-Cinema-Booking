using CinemaAPI.BusinessLogic.Interfaces;
using CinemaAPI.Core.DatabaseLayer;
using CinemaAPI.Core.DatabaseLayer.Interfaces;
using CinemaAPI.Core.Models;
using CinemaAPI.Core.Models.Enum;
using CinemaAPI.DTOs;

namespace CinemaAPI.BusinessLogic
{
	public class ShowtimeService : IShowtimeService
	{
		private readonly IShowtimeAccess _showtimeAccess;
		private readonly IMovieAccess _movieAccess;
		private readonly string _apiBaseUrl = "";

		public ShowtimeService(IShowtimeAccess showtimeAccess, IMovieAccess movieAccess, IConfiguration configuration)
		{
			_showtimeAccess = showtimeAccess;
			_movieAccess = movieAccess;
			var apiBaseUrl = configuration["ApiSettings:BaseUrl"];
			if (apiBaseUrl != null && !string.IsNullOrWhiteSpace(apiBaseUrl))
			{
				_apiBaseUrl = apiBaseUrl;
			}
		}

		public async Task<IEnumerable<GetShowtimeDTO>?> GetAllShowtimesByCityAsync(string city)
		{
			List<GetShowtimeDTO> showtimeDTOList = new List<GetShowtimeDTO>();
			var showtimes = await _showtimeAccess.GetAllShowtimesByCityAsync(city);
			if (showtimes != null && showtimes.Any())
			{
				foreach (var showtime in showtimes)
				{
					GetShowtimeDTO showtimeDTO = await MapToDTOAsync(showtime);
					showtimeDTOList.Add(showtimeDTO);
				}
			}
			return showtimeDTOList;
		}

		public async Task<IEnumerable<GetShowtimeDTO>?> GetShowtimesByAuditoriumDateAsync(int auditoriumID, DateTime startDate, DateTime endDate)
		{
			List<GetShowtimeDTO> showtimeDTOList = new List<GetShowtimeDTO>();
			var showtimes = await _showtimeAccess.GetShowtimesByAuditoriumDateAsync(auditoriumID, startDate, endDate);
			if (showtimes != null && showtimes.Any())
			{
				foreach (var showtime in showtimes)
				{
					GetShowtimeDTO showtimeDTO = await MapToDTOAsync(showtime);
					showtimeDTOList.Add(showtimeDTO);
				}
			}
			return showtimeDTOList;
		}

		public async Task<IEnumerable<GetShowtimeDTO>?> GetMovieShowtimesAsync(int movieID, string? city)
		{
			List<GetShowtimeDTO> showtimeDTOList = new List<GetShowtimeDTO>();
			var showtimes = await _showtimeAccess.GetMovieShowtimesAsync(movieID, city);
			if (showtimes != null && showtimes.Any())
			{
				foreach (var showtime in showtimes)
				{
					GetShowtimeDTO showtimeDTO = await MapToDTOAsync(showtime);
					showtimeDTOList.Add(showtimeDTO);
				}
			}
			return showtimeDTOList;
		}

		public async Task<GetShowtimeDTO?> GetShowtimeByIDAsync(int showtimeID)
		{
			var showtime = await _showtimeAccess.GetShowtimeByIDAsync(showtimeID);
			if (showtime != null)
			{
				return await MapToDTOAsync(showtime);
			}
			return null;
		}

		public async Task<GetShowtimeSeatDTO?> GetShowtimeSeatByIDAsync(int showtimeID, int seatID)
		{
			var showtimeSeat = await _showtimeAccess.GetShowtimeSeatByIDAsync(showtimeID, seatID);
			if (showtimeSeat != null)
			{
				return MapToDTOShowtimeSeat(showtimeSeat);
			}
			return null;
		}

		public async Task<GetShowtimeDTO?> CreateShowtimeAsync(PostShowtimeDTO showtimeDTO)
		{
			Showtime showtime = new Showtime
			{
				MovieID = showtimeDTO.MovieID,
				AuditoriumID = showtimeDTO.AuditoriumID,
				ShowType = Enum.Parse<ShowTypeEnum>(showtimeDTO.ShowType),
				StartTime = showtimeDTO.StartTime
			};
			var createdShowtime = await _showtimeAccess.CreateShowtimeAsync(showtime);
			if (createdShowtime != null)
			{
				return await MapToDTOAsync(createdShowtime);
			}
			return null;
		}

		public async Task<bool> DeleteShowtimeAsync(int showtimeID)
		{
			return await _showtimeAccess.DeleteShowtimeAsync(showtimeID);
		}

		public async Task<GetShowtimeDTO?> UpdateShowtimeAsync(int showtimeID, PutShowtimeDTO showtimeDTO)
		{
			var existingShowtime = await _showtimeAccess.GetShowtimeByIDAsync(showtimeID);
			if (existingShowtime == null)
			{
				return null;
			}

			Showtime showtime = new Showtime
			{
				ShowtimeID = showtimeID,
				MovieID = showtimeDTO.MovieID,
				AuditoriumID = existingShowtime.AuditoriumID,
				ShowType = Enum.Parse<ShowTypeEnum>(showtimeDTO.ShowType),
				StartTime = showtimeDTO.StartTime
			};

			var updatedShowtime = await _showtimeAccess.UpdateShowtimeAsync(showtime);
			if (updatedShowtime != null)
			{
				return await MapToDTOAsync(updatedShowtime);
			}
			return null;
		}

		public async Task<GetShowtimeDTO> MapToDTOAsync(Showtime showtime)
		{
			List<string> imageUrls = [];
			if ( showtime.Movie != null)
			{
				var imageIds = await _movieAccess.GetMovieImageIdsAsync(showtime.Movie.MovieID);
				imageUrls = imageIds.OrderBy(image => image.ImageIndex).Select(image => $"{_apiBaseUrl}/api/movies/{showtime.Movie.MovieID}/images/{image.ImageIndex}").ToList();
			}

			return new GetShowtimeDTO
			{
				ShowtimeID = showtime.ShowtimeID,
				ShowType = showtime.ShowType.ToString(),
				StartTime = showtime.StartTime,
				Movie = new GetMovieDTO
				{
					MovieID = showtime.Movie.MovieID,
					Title = showtime.Movie.Title,
					ReleaseDate = showtime.Movie.ReleaseDate,
					AgeLimit = showtime.Movie.AgeLimit,
					Duration = showtime.Movie.Duration,
					TrailerYoutubeID = showtime.Movie.TrailerYoutubeID,
					Resume = showtime.Movie.Resume,
					Language = showtime.Movie.Language.ToString(),
					Subtitles = showtime.Movie.Subtitles.ToString(),
					Director = showtime.Movie.Director,
					MainActor = showtime.Movie.MainActor,
					Genres = showtime.Movie.Genres.Select(g => g.Name).ToList(),
					ImageUrls = imageUrls
				},
				Auditorium = showtime.Auditorium,
				SeatAvailability = showtime.SeatAvailability
			};
		}

		private GetShowtimeSeatDTO MapToDTOShowtimeSeat(ShowtimeSeat showtimeSeat)
		{
			return new GetShowtimeSeatDTO
			{
				ShowtimeID = showtimeSeat.ShowtimeID,
				SeatID = showtimeSeat.SeatID,
				Seat = showtimeSeat.Seat,
				Status = showtimeSeat.Status,
				Ticket = showtimeSeat.Ticket != null ? MapToDTOTicket(showtimeSeat.Ticket) : null
			};
		}

		private GetTicketDTO MapToDTOTicket(Ticket ticket)
		{
			return new GetTicketDTO
			{
				TicketID = ticket.TicketID,
				Customer = MapToDTOCustomer(ticket.Customer),
				Price = ticket.Price,
				PurchaseDate = ticket.PurchaseDate,
				ExpireDate = ticket.ExpireDate
			};
		}

		private GetCustomerDTO MapToDTOCustomer(Customer customer)
		{
			return new GetCustomerDTO
			{
				CustomerID = customer.CustomerID,
				Name = customer.Name,
				Email = customer.Email,
				PhoneNo = customer.PhoneNo,
				CreationDate = customer.CreationDate
			};
		}
	}
}
