namespace CinemaAPI.DTOs
{
	public class PostMovieDTO
	{
		public required string Title { get; set; }
		public required DateTime ReleaseDate { get; set; }
		public required int AgeLimit { get; set; }
		public required int Duration { get; set; }
		public required string TrailerYoutubeID { get; set; }
		public required string Resume { get; set; }
		public required string Language { get; set; }
		public required string Subtitles { get; set; }
		public required string Director { get; set; }
		public required string MainActor { get; set; }
		public required List<string> Genres { get; set; }
		public required List<byte[]> Images { get; set; }
	}
}
