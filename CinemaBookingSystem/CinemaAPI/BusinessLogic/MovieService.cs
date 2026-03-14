using CinemaAPI.BusinessLogic.Interfaces;
using CinemaAPI.Core.DatabaseLayer.Interfaces;
using CinemaAPI.Core.Models;
using CinemaAPI.Core.Models.Enum;
using CinemaAPI.DTOs;
using Microsoft.Extensions.Configuration;

namespace CinemaAPI.BusinessLogic
{
	public class MovieService : IMovieService
	{
		private readonly IMovieAccess _movieAccess;
		private readonly string _apiBaseUrl = "";

		public MovieService(IMovieAccess movieAccess, IConfiguration configuration)
		{
			_movieAccess = movieAccess;
			var apiBaseUrl = configuration["ApiSettings:BaseUrl"];
			if (apiBaseUrl != null && !string.IsNullOrWhiteSpace(apiBaseUrl))
			{
				_apiBaseUrl = apiBaseUrl;
			}
		}

		public async Task<IEnumerable<GetMovieDTO>?> GetAllMoviesAsync(string? city, string? genre, LanguageEnum? language, string? age)
		{
			List<GetMovieDTO> movieDTOList = [];
			var movies = await _movieAccess.GetAllMoviesAsync(city, genre, language, age);
			if(movies != null && movies.Any())
			{
				foreach (var movie in movies)
				{
					GetMovieDTO movieDTO = await MapToDTOAsync(movie);
					movieDTOList.Add(movieDTO);
				}
			}
			return movieDTOList;
		}

		public async Task<GetMovieDTO?> GetMovieByIDAsync(int movieID)
		{
			GetMovieDTO? movieDTO = null;
			Movie? movie = await _movieAccess.GetMovieByIDAsync(movieID);
			if (movie != null)
			{
				movieDTO = await MapToDTOAsync(movie);
			}
			return movieDTO;
		}

		public async Task<byte[]> GetMovieImageByIndexAsync(int movieID, int imageIndex)
		{
			return await _movieAccess.GetMovieImageByIndexAsync(movieID, imageIndex);
		}

		public async Task<GetMovieDTO?> CreateMovieAsync(PostMovieDTO movie)
		{
			Movie createdMovie = await _movieAccess.CreateMovieAsync(MapPostDTOToEntity(movie));
			return await MapToDTOAsync(createdMovie);
		}

		public async Task<bool> DeleteMovieAsync(int movieID)
		{
			return await _movieAccess.DeleteMovieAsync(movieID);
		}

		public async Task<GetMovieDTO?> UpdateMovieAsync(int movieID, PostMovieDTO movie)
		{
			Movie movieToUpdate = MapPostDTOToEntity(movie);
			movieToUpdate.MovieID = movieID;
			Movie updatedMovie = await _movieAccess.UpdateMovieAsync(movieToUpdate);
			return await MapToDTOAsync(updatedMovie);
		}
		
		public async Task<List<string>> GetGenresFromCityAsync(string city)
		{
			return await _movieAccess.GetGenresFromCityAsync(city);
		}

		public async Task<List<LanguageEnum>> GetMovieLanguagesFromCityAsync(string city)
		{
			return await _movieAccess.GetMovieLanguagesFromCityAsync(city);
		}

		public async Task<GetMovieDTO> MapToDTOAsync(Movie movie)
		{
			var imageIds = await _movieAccess.GetMovieImageIdsAsync(movie.MovieID);
			var imageUrls = imageIds.OrderBy(image => image.ImageIndex).Select(image => $"{_apiBaseUrl}/api/movies/{movie.MovieID}/images/{image.ImageIndex}").ToList();

			return new GetMovieDTO
			{
				MovieID = movie.MovieID,
				Title = movie.Title,
				ReleaseDate = movie.ReleaseDate,
				AgeLimit = movie.AgeLimit,
				Duration = movie.Duration,
				TrailerYoutubeID = movie.TrailerYoutubeID,
				Resume = movie.Resume,
				Language = movie.Language.ToString(),
				Subtitles = movie.Subtitles.ToString(),
				Director = movie.Director,
				MainActor = movie.MainActor,
				Genres = movie.Genres.Select(g => g.Name).ToList(),
				ImageUrls = imageUrls
			};
		}


		public Movie MapPostDTOToEntity(PostMovieDTO movieDTO)
		{
			List<Genre> genres = [];
			foreach (var genreName in movieDTO.Genres)
			{
				var genre = new Genre { Name = genreName };
				genres.Add(genre);
			}

			List<MovieImage> movieImages = [];
			foreach (var imageData in movieDTO.Images)
			{
				var movieImage = new MovieImage { ImageData = imageData };
				movieImages.Add(movieImage);
			}

			return new Movie
			{
				Title = movieDTO.Title,
				ReleaseDate = movieDTO.ReleaseDate,
				AgeLimit = movieDTO.AgeLimit,
				Duration = movieDTO.Duration,
				TrailerYoutubeID = movieDTO.TrailerYoutubeID,
				Resume = movieDTO.Resume,
				Language = Enum.Parse<LanguageEnum>(movieDTO.Language),
				Subtitles = Enum.Parse<SubtitlesEnum>(movieDTO.Subtitles),
				Director = movieDTO.Director,
				MainActor = movieDTO.MainActor,
				Genres = genres,
				MovieImages = movieImages
			};
		}
	}
}
