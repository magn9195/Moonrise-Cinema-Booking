using CinemaAPI.Core.Models.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CinemaAPI.Core.Models
{
	public class Movie
	{
		public int MovieID { get; set; }
		public required string Title { get; set; }
		public required DateTime ReleaseDate { get; set; }
		public required int AgeLimit { get; set; }
		public required int Duration { get; set; }
		public required string TrailerYoutubeID { get; set; }
		public required string Resume { get; set; }
		public required LanguageEnum Language { get; set; }
		public required SubtitlesEnum Subtitles { get; set; }
		public required string Director { get; set; }
		public required string MainActor { get; set; }
		public required List<Genre> Genres { get; set; }
		public required List<MovieImage> MovieImages { get; set; }
	}
}
