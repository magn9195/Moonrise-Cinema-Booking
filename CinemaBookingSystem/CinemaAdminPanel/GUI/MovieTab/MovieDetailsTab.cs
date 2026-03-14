using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using CinemaAdminPanel.Models;
using CinemaAdminPanel.Properties;

namespace CinemaAdminPanel.GUI.MovieTab
{
	public partial class MovieDetailsTab : UserControl
	{
		private Font _font;
		private PictureBox _moviePicture;
		private Label _movieMovieID;
		private Label _movieTitle;
		private Label _movieRelease;
		private Label _movieDuration;
		private Label _movieAgeLimit;
		private Label _movieGenre;
		private Label _movieLanguage;
		private Label _movieSubtitles;
		private Label _movieEmbedLink;
		private Label _movieDirector;
		private Label _movieActors;
		private RichTextBox _movieResume;

		public MovieDetailsTab()
		{
			InitializeComponent();

			_font = new Font("Arial", 12);

			var mainPanel = new Panel();
			mainPanel.Dock = DockStyle.Fill;
			mainPanel.Padding = new Padding(20);
			mainPanel.AutoScroll = true;

			var topLayout = new TableLayoutPanel();
			topLayout.Dock = DockStyle.Top;
			topLayout.RowCount = 2;
			topLayout.ColumnCount = 1;
			topLayout.Height = 300;

			topLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 40));
			topLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));

			var titleLabel = new Label();
			titleLabel.Name = "detailsHeaderTitle";
			titleLabel.Font = new Font("Segoe UI", 14, FontStyle.Bold);
			titleLabel.Text = "Movie Details";
			titleLabel.TextAlign = ContentAlignment.MiddleCenter;
			titleLabel.Dock = DockStyle.Fill;

			_moviePicture = new PictureBox();
			_moviePicture.Name = "moviePicture";
			_moviePicture.SizeMode = PictureBoxSizeMode.Zoom;
			_moviePicture.Size = new Size(200, 200);
			_moviePicture.Dock = DockStyle.None;
			_moviePicture.Anchor = AnchorStyles.None;
			_moviePicture.Image = Resources.Placeholder;

			topLayout.Controls.Add(titleLabel, 0, 0);
			topLayout.Controls.Add(_moviePicture, 0, 1);

			var fieldLayout = new TableLayoutPanel();
			fieldLayout.Dock = DockStyle.Top;
			fieldLayout.AutoSize = true;
			fieldLayout.ColumnCount = 2;
			fieldLayout.Padding = new Padding(0, 30, 0, 0);

			// Set consistent row heights
			int rowCount = 11; // Adjust based on your actual field count
			for (int i = 0; i < rowCount; i++)
			{
				fieldLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 30));
			}

			// Create fields with consistent styling
			var createLabel = (string text) => new Label
			{
				Text = text,
				AutoSize = false,
				Height = 25,
				TextAlign = ContentAlignment.MiddleLeft,
				Dock = DockStyle.Fill
			};

			var createValueLabel = (string name, string text, bool enabled = true) => new Label
			{
				Name = name,
				Text = text,
				Enabled = enabled,
				AutoSize = false,
				TextAlign = ContentAlignment.MiddleLeft,
				Dock = DockStyle.Fill,
				Height = 25,
				Margin = new Padding(3, 3, 3, 3)
			};

			// Create fields
			var lblMovieID = createLabel("Movie ID:");
			lblMovieID.Font = _font;
			_movieMovieID = createValueLabel("movieMovieID", "");
			_movieMovieID.Font = _font;

			var lblTitle = createLabel("Title:");
			lblTitle.Font = _font;

			_movieTitle = createValueLabel("movieTitle", "");
			_movieTitle.Font = _font;

			var lblRelease = createLabel("Release Date:");
			lblRelease.Font = _font;
			_movieRelease = createValueLabel("movieRelease", "");
			_movieRelease.Font = _font;

			var lblDuration = createLabel("Duration (min):");
			lblDuration.Font = _font;
			_movieDuration = createValueLabel("movieDuration", "");
			_movieDuration.Font = _font;

			var lblAgeLimit = createLabel("Age Limit:");
			lblAgeLimit.Font = _font;
			_movieAgeLimit = createValueLabel("movieAgeLimit", "");
			_movieAgeLimit.Font = _font;

			var lblGenre = createLabel("Genre:");
			lblGenre.Font = _font;
			_movieGenre = createValueLabel("movieGenre", "");
			_movieGenre.Font = _font;

			var lblLanguage = createLabel("Language:");
			lblLanguage.Font = _font;
			_movieLanguage = createValueLabel("movieLanguage", "");
			_movieLanguage.Font = _font;

			var lblSubtitles = createLabel("Subtitles:");
			lblSubtitles.Font = _font;
			_movieSubtitles = createValueLabel("movieSubtitles", "");
			_movieSubtitles.Font = _font;

			var lblEmbedLink = createLabel("EmbedLink:");
			lblEmbedLink.Font = _font;
			_movieEmbedLink = createValueLabel("movieEmbedLink", "");
			_movieEmbedLink.Font = _font;

			var lblDirector = createLabel("Director:");
			lblDirector.Font = _font;
			_movieDirector = createValueLabel("movieDirector", "");
			_movieDirector.Font = _font;

			var lblActors = createLabel("Main Actor:");
			lblActors.Font = _font;
			_movieActors = createValueLabel("movieActors", "");
			_movieActors.Font = _font;

			var lblResume = createLabel("Resume:");
			_movieResume = new RichTextBox
			{
				Name = "movieResume",
				Text = "",
				Height = 100,
				Width = 200,
				Anchor = AnchorStyles.Left | AnchorStyles.Right,
				Margin = new Padding(3, 3, 3, 3),
				ReadOnly = true
			};
			lblResume.Font = _font;
			_movieResume.Font = _font;

			// Add them in rows
			fieldLayout.Controls.Add(lblMovieID, 0, 0);
			fieldLayout.Controls.Add(_movieMovieID, 1, 0);

			fieldLayout.Controls.Add(lblTitle, 0, 1);
			fieldLayout.Controls.Add(_movieTitle, 1, 1);

			fieldLayout.Controls.Add(lblRelease, 0, 2);
			fieldLayout.Controls.Add(_movieRelease, 1, 2);

			fieldLayout.Controls.Add(lblGenre, 0, 3);
			fieldLayout.Controls.Add(_movieGenre, 1, 3);

			fieldLayout.Controls.Add(lblDuration, 0, 4);
			fieldLayout.Controls.Add(_movieDuration, 1, 4);

			fieldLayout.Controls.Add(lblAgeLimit, 0, 5);
			fieldLayout.Controls.Add(_movieAgeLimit, 1, 5);

			fieldLayout.Controls.Add(lblLanguage, 0, 6);
			fieldLayout.Controls.Add(_movieLanguage, 1, 6);

			fieldLayout.Controls.Add(lblSubtitles, 0, 7);
			fieldLayout.Controls.Add(_movieSubtitles, 1, 7);

			fieldLayout.Controls.Add(lblEmbedLink, 0, 8);
			fieldLayout.Controls.Add(_movieEmbedLink, 1, 8);

			fieldLayout.Controls.Add(lblDirector, 0, 9);
			fieldLayout.Controls.Add(_movieDirector, 1, 9);

			fieldLayout.Controls.Add(lblActors, 0, 10);
			fieldLayout.Controls.Add(_movieActors, 1, 10);

			fieldLayout.Controls.Add(lblResume, 0, 11);
			fieldLayout.Controls.Add(_movieResume, 1, 11);

			mainPanel.Controls.Add(fieldLayout);
			mainPanel.Controls.Add(topLayout);

			this.Controls.Add(mainPanel);
		}

		public async void LoadMovie(GetMovieCS movie)
		{
			if (movie == null)
			{
				ClearMovie();
				return;
			}

			_movieMovieID.Text = movie.MovieID.ToString();
			_movieTitle.Text = movie.Title;
			_movieRelease.Text = movie.ReleaseDate.ToShortDateString();
			_movieDuration.Text = movie.Duration.ToString();
			_movieAgeLimit.Text = movie.AgeLimit.ToString();
			_movieGenre.Text = string.Join(", ", movie.Genres);
			_movieLanguage.Text = movie.Language;
			_movieSubtitles.Text = movie.Subtitles;
			_movieEmbedLink.Text = movie.TrailerYoutubeID;
			_movieDirector.Text = movie.Director;
			_movieActors.Text = movie.MainActor;
			_movieResume.Text = movie.Resume;

			// Load image
			await LoadMovieImageFromUrl(movie.ImageUrls.FirstOrDefault());
		}

		private async Task LoadMovieImageFromUrl(string imageUrl)
		{
			if (string.IsNullOrEmpty(imageUrl))
			{
				_moviePicture.Image = Resources.Placeholder;
				return;
			}

			try
			{
				using (var httpClient = new HttpClient())
				{
					var imageBytes = await httpClient.GetByteArrayAsync(imageUrl);
					using (var ms = new MemoryStream(imageBytes))
					{
						_moviePicture.Image = Image.FromStream(ms);
					}
				}
			}
			catch (Exception)
			{
				_moviePicture.Image = Resources.Placeholder;
				Console.WriteLine($"Error Loading Image");
			}
		}

		public void ClearMovie()
		{
			_movieMovieID.Text = "";
			_movieTitle.Text = "";
			_movieRelease.Text = "";
			_movieDuration.Text = "";
			_movieAgeLimit.Text = "";
			_movieGenre.Text = "";
			_movieLanguage.Text = "";
			_movieSubtitles.Text = "";
			_movieEmbedLink.Text = "";
			_movieDirector.Text = "";
			_movieActors.Text = "";
			_movieResume.Text = "";
			_moviePicture.Image = Resources.Placeholder;
		}
	}
}
