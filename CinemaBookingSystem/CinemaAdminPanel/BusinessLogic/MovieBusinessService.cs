using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CinemaAdminPanel.BusinessLogic.Interfaces;
using CinemaAdminPanel.GUI.DisplayModels;
using CinemaAdminPanel.Models;
using CinemaAdminPanel.Service.Interfaces;

namespace CinemaAdminPanel.BusinessLogic
{
    public class MovieBusinessService : IMovieBusinessService
	{
        private readonly IMovieAPIClient _movieApiClient;

        public MovieBusinessService(IMovieAPIClient movieApiClient)
        {
            _movieApiClient = movieApiClient;
		}

		public async Task<List<MovieDM>> GetAllMoviesForDisplayAsync()
		{
			var movies = await _movieApiClient.GetAllMoviesAsync();
			if (movies == null) return new List<MovieDM>();

			var displayMovies = ConvertToDisplayModels(movies);
			if (displayMovies == null) return new List<MovieDM>();

			return displayMovies;
		}

		public async Task<IEnumerable<GetMovieCS>> GetAllMoviesAsync()
		{
			var movies = await _movieApiClient.GetAllMoviesAsync();
			if (movies == null) return [];
			return movies;
		}

		public async Task<GetMovieCS?> GetMovieByIDAsync(int movieId)
		{
			return await _movieApiClient.GetMovieByIdAsync(movieId);
		}

		public async Task<MovieDM?> CreateMovieAsync(PostMovieCS movieCS)
		{
			GetMovieCS createdMovie = await _movieApiClient.CreateMovieAsync(movieCS);
			var movieDM = ConvertToDisplayModel(createdMovie);
			return movieDM;
		}

		public async Task<MovieDM?> UpdateMovieAsync(int movieId, PostMovieCS movieCS)
		{
			GetMovieCS updatedMovie = await _movieApiClient.UpdateMovieAsync(movieId, movieCS);
			var movieDM = ConvertToDisplayModel(updatedMovie);
			return movieDM;
		}

		public async Task<bool> DeleteMovieAsync(int movieId)
		{
			return await _movieApiClient.DeleteMovieAsync(movieId);
		}

		private MovieDM? ConvertToDisplayModel(GetMovieCS movie)
		{
			if (movie == null)
				return null;
			MovieDM displayMovies = new MovieDM
			{
				MovieID = movie.MovieID,
				Title = movie.Title,
				FormattedReleaseDate = movie.ReleaseDate.ToString("dd/MM/yyyy"),
				DurationDisplay = $"{movie.Duration / 60}h {movie.Duration % 60}m",
				GenreList = string.Join(", ", movie.Genres)
			};
			return displayMovies;
		}

		private List<MovieDM> ConvertToDisplayModels(IEnumerable<GetMovieCS> movies)
		{
			if (movies == null) return new List<MovieDM>();
			List<MovieDM> displayMovies = movies.Select(m => new MovieDM
			{
				MovieID = m.MovieID,
				Title = m.Title,
				FormattedReleaseDate = m.ReleaseDate.ToString("dd/MM/yyyy"),
				DurationDisplay = $"{m.Duration / 60}h {m.Duration % 60}m",
				GenreList = string.Join(", ", m.Genres)
			}).ToList();
			return displayMovies;
		}
	}
}
