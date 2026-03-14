using CinemaWebService.BusinessLogic.BusinessHelper;
using CinemaWebService.BusinessLogic.Interfaces;
using CinemaWebService.Models;
using CinemaWebService.Service.Interfaces;
using CinemaWebService.Views.ViewModels;

namespace CinemaWebService.BusinessLogic
{
	public class ShowtimeBusinessService : IShowtimeBusinessService
	{
		private readonly IShowtimeAPIClient _showtimeApiClient;
		private readonly IMovieAPIClient _movieApiClient;
		private readonly ICinemaAPIClient _cinemaApiClient;
		private readonly ILogger<ShowtimeBusinessService> _logger;

		public ShowtimeBusinessService(IShowtimeAPIClient showtimeApiClient, IMovieAPIClient movieApiClient, ICinemaAPIClient cinemaApiClient, ILogger<ShowtimeBusinessService> logger)
		{
			_showtimeApiClient = showtimeApiClient;
			_movieApiClient = movieApiClient;
			_cinemaApiClient = cinemaApiClient;
			_logger = logger;
		}

		public async Task<ShowtimeDetailsVM?> GetShowtimeDetailsAsync(int movieID, int showtimeID)
		{
			var showtime = await _showtimeApiClient.GetShowtimeByIdAsync(showtimeID);
			if (showtime == null)
			{
				return null;
			}

			var movie = await _movieApiClient.GetMovieByIdAsync(movieID);
			if (movie == null)
			{
				return null;
			}

			var cinema = await _cinemaApiClient.GetCinemaByIDAsync(showtime.Auditorium.CinemaID);
			if (cinema == null)
			{
				return null;
			}

			List<string> imageUrls = movie.ImageUrls;
			return VMCombiner.CombineShowtimeDetails(movie, imageUrls, showtime, cinema);
		}

		public async Task<GetShowtimeCS> GetShowtimeByIDAsync(int showtimeID)
		{
			var showtime = await _showtimeApiClient.GetShowtimeByIdAsync(showtimeID);
			return showtime;
		}
	}
}
