using AspNetCoreGeneratedDocument;
using CinemaWebService.BusinessLogic.BusinessHelper;
using CinemaWebService.BusinessLogic.Interfaces;
using CinemaWebService.Models;
using CinemaWebService.Models.Enum;
using CinemaWebService.Service.Interfaces;
using CinemaWebService.Views.ViewModels;
using Microsoft.AspNetCore.Mvc.RazorPages;
using static System.Net.Mime.MediaTypeNames;

namespace CinemaWebService.BusinessLogic
{
	public class MovieBusinessService : IMovieBusinessService
	{
		private readonly IMovieAPIClient _movieApiClient;
		private readonly IShowtimeAPIClient _showtimeApiClient;
		private readonly ICinemaAPIClient _cinemaAPIClient;
		private readonly ILogger<MovieBusinessService> _logger;

		public MovieBusinessService(IMovieAPIClient movieApiClient, IShowtimeAPIClient showtimeApiClient, ICinemaAPIClient cinemaAPIClient, ILogger<MovieBusinessService> logger)
		{
			_movieApiClient = movieApiClient;
			_showtimeApiClient = showtimeApiClient;
			_cinemaAPIClient = cinemaAPIClient;
			_logger = logger;
		}

		public async Task<ListVM?> GetListMoviesAsync(string city, string? genre, string? language, string? age)
		{
			var movies = await _movieApiClient.GetAllMoviesAsync(city, genre, language, age);
			Dictionary<GetMovieCS, List<string>> moviesVMDict = new Dictionary<GetMovieCS, List<string>>();
			var genres = await _movieApiClient.GetGenresFromCityAsync(city);
			var languages = await _movieApiClient.GetMovieLanguagesFromCityAsync(city);
				

			if (movies == null || !movies.Any())
			{
				return VMCombiner.CombineList(new Dictionary<GetMovieCS, List<string>>(), city, genres, languages);
			}

			foreach (var movie in movies)
			{
				moviesVMDict[movie] = movie.ImageUrls;
			}

			return VMCombiner.CombineList(moviesVMDict, city, genres, languages);
		}

		public async Task<DetailsVM?> GetMovieDetailsByIdAsync(int movieID, string city)
		{
			var movie = await _movieApiClient.GetMovieByIdAsync(movieID);
			if (movie == null)
			{
				return null;
			}

			var showtimes = await _showtimeApiClient.GetMovieShowtimesAsync(movieID, city);
			if (showtimes == null || !showtimes.Any())
			{
				return null;
			}

			var cinemas = await _cinemaAPIClient.GetAllCinemasAsync(city);
			if (cinemas == null || !cinemas.Any())
			{
				return null;
			}

			var showtimesByCinema = new Dictionary<int, List<GetShowtimeCS>>();
			foreach (var showtime in showtimes)
			{
				int cinemaId = showtime.Auditorium.CinemaID;
				if (!showtimesByCinema.ContainsKey(cinemaId))
				{
					showtimesByCinema[cinemaId] = new List<GetShowtimeCS>();
				}
				showtimesByCinema[cinemaId].Add(showtime);
			}

			var cinemaDictionary = new Dictionary<GetCinemaCS, IEnumerable<GetShowtimeCS>>();
			foreach (var cinema in cinemas)
			{
				if (showtimesByCinema.ContainsKey(cinema.CinemaID))
				{
					cinemaDictionary[cinema] = showtimesByCinema[cinema.CinemaID];
				}
				else
				{
					cinemaDictionary[cinema] = Enumerable.Empty<GetShowtimeCS>();
				}
			}
			List<string> imageUrls = movie.ImageUrls;
			return VMCombiner.CombineDetails(movie, imageUrls, cinemaDictionary, city);
		}
	}
}
