using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using CinemaAdminPanel.BusinessLogic;
using CinemaAdminPanel.BusinessLogic.Interfaces;
using CinemaAdminPanel.Models;
using CinemaAdminPanel.Models.Enum;
using CinemaAdminPanel.Properties;

namespace CinemaAdminPanel.GUI.MovieTab
{
	public partial class MovieEditTab : UserControl
	{
		private readonly IMovieBusinessService _movieBusinessService;
		private GetMovieCS? _currentMovie = null;
		private byte[]? _originalImageBytes = null;

		private PictureBox _moviePicture = new();
		private Button _selectImageButton = new();
		private TextBox _txtfldMovieID = new();
		private TextBox _txtfldTitle = new();
		private DateTimePicker _txtfldRelease = new();
		private TextBox _txtfldDuration = new();
		private TextBox _txtfldAgeLimit = new();
		private TextBox _txtfldGenre = new();
		private ComboBox _languageComboBox = new();
		private ComboBox _subtitlesComboBox = new();
		private TextBox _txtfldEmbedLink = new();
		private TextBox _txtfldDirector = new();
		private TextBox _txtfldActors = new();
		private RichTextBox _txtboxResume = new();
		private Button _saveButton = new();

		public event EventHandler MovieUpdated = null!;

		public MovieEditTab(IMovieBusinessService movieBusinessService)
		{
			InitializeComponent();
			_movieBusinessService = movieBusinessService;
			InitializeControls();
		}

		private void InitializeControls()
		{
			var mainPanel = new Panel();
			mainPanel.Dock = DockStyle.Fill;
			mainPanel.Padding = new Padding(20);
			mainPanel.AutoScroll = true;

			var topLayout = new TableLayoutPanel();
			topLayout.Dock = DockStyle.Top;
			topLayout.RowCount = 3;
			topLayout.ColumnCount = 1;
			topLayout.Height = 240;

			topLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 40));
			topLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));

			var titleLabel = new Label();
			titleLabel.Name = "editHeaderTitle";
			titleLabel.Font = new Font("Segoe UI", 14, FontStyle.Bold);
			titleLabel.Text = "Edit Movie";
			titleLabel.TextAlign = ContentAlignment.MiddleCenter;
			titleLabel.Dock = DockStyle.Fill;

			_moviePicture = new PictureBox();
			_moviePicture.Name = "moviePicture";
			_moviePicture.SizeMode = PictureBoxSizeMode.Zoom;
			_moviePicture.Size = new Size(140, 140);
			_moviePicture.Image = Resources.Placeholder;
			_moviePicture.Dock = DockStyle.None;
			_moviePicture.Anchor = AnchorStyles.None;

			_selectImageButton = new Button { Text = "Select Image", Width = 120, Height = 30, Enabled = false };
			_selectImageButton.Anchor = AnchorStyles.None;
			_selectImageButton.Enabled = false;
			_selectImageButton.Click += SelectImageButton_Click;

			topLayout.Controls.Add(titleLabel, 0, 0);
			topLayout.Controls.Add(_moviePicture, 0, 1);
			topLayout.Controls.Add(_selectImageButton, 0, 2);

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

			var createTextBox = (string name, string text, bool enabled = true) => new TextBox
			{
				Name = name,
				Text = text,
				Enabled = false,
				Height = 25,
				Anchor = AnchorStyles.Left | AnchorStyles.Right,
				Margin = new Padding(3, 3, 3, 3)
			};

			// Create fields
			var lblMovieID = createLabel("Movie ID:");
			_txtfldMovieID = createTextBox("txtMovieID", "", false);

			var lblTitle = createLabel("Title:");
			_txtfldTitle = createTextBox("txtTitle", "");

			var lblRelease = createLabel("Release Date:");
			_txtfldRelease = new DateTimePicker
			{
				Height = 25,
				Anchor = AnchorStyles.Left | AnchorStyles.Right,
				Margin = new Padding(3, 3, 3, 3),
				Enabled = false,
				Dock = DockStyle.Fill,
				Format = DateTimePickerFormat.Short,
				Value = DateTime.Today
			};


			var lblDuration = createLabel("Duration (min):");
			_txtfldDuration = createTextBox("txtDuration", "");

			var lblAgeLimit = createLabel("Age Limit:");
			_txtfldAgeLimit = createTextBox("txtAgeLimit", "");

			var lblGenre = createLabel("Genre:");
			_txtfldGenre = createTextBox("txtGenre", "");

			var lblLanguage = createLabel("Language:");
			_languageComboBox = new ComboBox
			{
				Height = 25,
				Anchor = AnchorStyles.Left | AnchorStyles.Right,
				Margin = new Padding(3, 3, 3, 3),
				Enabled = false,
				Dock = DockStyle.Fill,
				DropDownStyle = ComboBoxStyle.DropDownList
			};

			var lblSubtitles = createLabel("Subtitles:");
			_subtitlesComboBox = new ComboBox
			{
				Height = 25,
				Anchor = AnchorStyles.Left | AnchorStyles.Right,
				Margin = new Padding(3, 3, 3, 3),
				Enabled = false,
				Dock = DockStyle.Fill,
				DropDownStyle = ComboBoxStyle.DropDownList
			};

			var lblEmbedLink = createLabel("EmbedLink:");
			_txtfldEmbedLink = createTextBox("txtEmbedLink", "");

			var lblDirector = createLabel("Director:");
			_txtfldDirector = createTextBox("txtDirector", "");

			var lblActors = createLabel("Main Actor:");
			_txtfldActors = createTextBox("txtActors", "");

			var lblResume = createLabel("Resume:");
			_txtboxResume = new RichTextBox
			{
				Name = "txtResume",
				Text = "",
				Height = 100,
				Width = 200,
				Anchor = AnchorStyles.Left | AnchorStyles.Right,
				Margin = new Padding(3, 3, 3, 3),
				Enabled = false
			};

			_saveButton = new Button { Text = "Save Changes", Width = 120, Height = 30, Enabled = false };
			_saveButton.BackColor = Color.LightBlue;
			_saveButton.FlatStyle = FlatStyle.Flat;
			_saveButton.Click += SaveButton_Click;

			// Add them in rows
			fieldLayout.Controls.Add(lblMovieID, 0, 0);
			fieldLayout.Controls.Add(_txtfldMovieID, 1, 0);

			fieldLayout.Controls.Add(lblTitle, 0, 1);
			fieldLayout.Controls.Add(_txtfldTitle, 1, 1);

			fieldLayout.Controls.Add(lblRelease, 0, 2);
			fieldLayout.Controls.Add(_txtfldRelease, 1, 2);

			fieldLayout.Controls.Add(lblGenre, 0, 3);
			fieldLayout.Controls.Add(_txtfldGenre, 1, 3);

			fieldLayout.Controls.Add(lblDuration, 0, 4);
			fieldLayout.Controls.Add(_txtfldDuration, 1, 4);

			fieldLayout.Controls.Add(lblAgeLimit, 0, 5);
			fieldLayout.Controls.Add(_txtfldAgeLimit, 1, 5);

			fieldLayout.Controls.Add(lblLanguage, 0, 6);
			fieldLayout.Controls.Add(_languageComboBox, 1, 6);

			fieldLayout.Controls.Add(lblSubtitles, 0, 7);
			fieldLayout.Controls.Add(_subtitlesComboBox, 1, 7);

			fieldLayout.Controls.Add(lblEmbedLink, 0, 8);
			fieldLayout.Controls.Add(_txtfldEmbedLink, 1, 8);

			fieldLayout.Controls.Add(lblDirector, 0, 9);
			fieldLayout.Controls.Add(_txtfldDirector, 1, 9);

			fieldLayout.Controls.Add(lblActors, 0, 10);
			fieldLayout.Controls.Add(_txtfldActors, 1, 10);

			fieldLayout.Controls.Add(lblResume, 0, 11);
			fieldLayout.Controls.Add(_txtboxResume, 1, 11);

			fieldLayout.Controls.Add(_saveButton, 1, 12);

			mainPanel.Controls.Add(fieldLayout);
			mainPanel.Controls.Add(topLayout);

			this.Controls.Add(mainPanel);
			PopulateComboBoxes();
		}

		private void PopulateComboBoxes()
		{
			var languages = Enum.GetValues(typeof(LanguageEnum)).Cast<LanguageEnum>().ToArray();
			_languageComboBox.DataSource = new List<LanguageEnum>(languages);
			_languageComboBox.SelectedIndex = 0;

			var subtitles = Enum.GetValues(typeof(SubtitlesEnum)).Cast<SubtitlesEnum>().ToArray();
			_subtitlesComboBox.DataSource = new List<SubtitlesEnum>(subtitles);
			_subtitlesComboBox.SelectedIndex = 0;
		}

		public async void LoadMovie(GetMovieCS movie)
		{
			if (movie == null)
			{
				ClearMovie();
				return;
			}

			_currentMovie = movie;

			_txtfldMovieID.Text = movie.MovieID.ToString();
			_txtfldTitle.Text = movie.Title;
			_txtfldRelease.Text = movie.ReleaseDate.ToShortDateString();
			_txtfldDuration.Text = movie.Duration.ToString();
			_txtfldAgeLimit.Text = movie.AgeLimit.ToString();
			_txtfldGenre.Text = string.Join(", ", movie.Genres);
			_languageComboBox.Text = movie.Language;
			_subtitlesComboBox.Text = movie.Subtitles;
			_txtfldEmbedLink.Text = movie.TrailerYoutubeID;
			_txtfldDirector.Text = movie.Director;
			_txtfldActors.Text = movie.MainActor;
			_txtboxResume.Text = movie.Resume;

			EnableControls(true);
			await LoadMovieImageFromUrl(movie.ImageUrls.FirstOrDefault());
		}

		private async Task LoadMovieImageFromUrl(string imageUrl)
		{
			if (string.IsNullOrEmpty(imageUrl))
			{
				_moviePicture.Image = Resources.Placeholder;
				_moviePicture.Tag = "Placeholder";
				_originalImageBytes = null;
				return;
			}

			try
			{
				using (var httpClient = new HttpClient())
				{
					var imageBytes = await httpClient.GetByteArrayAsync(imageUrl);
					_originalImageBytes = imageBytes;

					using (var ms = new MemoryStream(imageBytes))
					{
						_moviePicture.Image = Image.FromStream(ms);
						_moviePicture.Tag = "OriginalImage";
					}
				}
			}
			catch (Exception)
			{
				_moviePicture.Image = Resources.Placeholder;
				_moviePicture.Tag = "Placeholder";
				_originalImageBytes = null;
				Console.WriteLine($"Error loading image");
			}
		}

		private byte[]? ConvertImageToBytes()
		{
			// User selected a new image
			if (_moviePicture.Tag?.ToString() == "NewImage")
			{
				try
				{
					using (var ms = new MemoryStream())
					{
						_moviePicture.Image.Save(ms, System.Drawing.Imaging.ImageFormat.Jpeg);
						return ms.ToArray();
					}
				}
				catch (Exception ex)
				{
					MessageBox.Show($"Warning: Could not process new image. Error: {ex.Message}",
						"Image Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
					return null;
				}
			}

			if (_moviePicture.Tag?.ToString() == "OriginalImage" && _originalImageBytes != null)
			{
				return _originalImageBytes;
			}
			return null;
		}

		private void EnableControls(bool enabled)
		{
			_txtfldTitle.Enabled = enabled;
			_txtfldRelease.Enabled = enabled;
			_txtfldDuration.Enabled = enabled;
			_txtfldAgeLimit.Enabled = enabled;
			_txtfldGenre.Enabled = enabled;
			_languageComboBox.Enabled = enabled;
			_subtitlesComboBox.Enabled = enabled;
			_txtfldEmbedLink.Enabled = enabled;
			_txtfldDirector.Enabled = enabled;
			_txtfldActors.Enabled = enabled;
			_txtboxResume.Enabled = enabled;
			_selectImageButton.Enabled = enabled;
			_saveButton.Enabled = enabled;
		}

		public void ClearMovie()
		{
			_txtfldMovieID.Text = "";
			_txtfldTitle.Text = "";
			_txtfldRelease.Value = DateTime.Today;
			_txtfldDuration.Text = "";
			_txtfldAgeLimit.Text = "";
			_txtfldGenre.Text = "";
			_languageComboBox.SelectedIndex = 0;
			_subtitlesComboBox.SelectedIndex = 0;
			_txtfldEmbedLink.Text = "";
			_txtfldDirector.Text = "";
			_txtfldActors.Text = "";
			_txtboxResume.Text = "";
			_moviePicture.Image = Resources.Placeholder;
		}

		private void SelectImageButton_Click(object? sender, EventArgs? e)
		{
			using (OpenFileDialog ofd = new OpenFileDialog())
			{
				ofd.Title = "Select Movie Image";
				ofd.Filter = "Image Files|*.jpg;*.jpeg;*.png;*.bmp";

				if (ofd.ShowDialog() == DialogResult.OK)
				{
					try
					{
						_moviePicture.Image = Image.FromFile(ofd.FileName);
						_moviePicture.Tag = "NewImage";
					}
					catch (Exception ex)
					{
						MessageBox.Show($"Error loading image: {ex.Message}", "Error",
							MessageBoxButtons.OK, MessageBoxIcon.Error);
					}
				}
			}
		}

		private async void SaveButton_Click(object? sender, EventArgs? e)
		{
			if (_currentMovie == null)
			{
				MessageBox.Show("No movie selected to edit.", "Error",
					MessageBoxButtons.OK, MessageBoxIcon.Error);
				return;
			}

			if (!ValidateInputs())
			{
				return;
			}

			try
			{
				var movieData = BuildMovieDTO();
				var updatedMovie = await _movieBusinessService.UpdateMovieAsync(_currentMovie.MovieID, movieData);

				if (updatedMovie != null)
				{
					MessageBox.Show("Movie updated successfully!", "Success",
						MessageBoxButtons.OK, MessageBoxIcon.Information);

					// Notify parent that the movie was saved
					MovieUpdated?.Invoke(this, EventArgs.Empty);
				}
				else
				{
					MessageBox.Show("Failed to update movie.", "Error",
						MessageBoxButtons.OK, MessageBoxIcon.Error);
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show($"Error saving movie: {ex.Message}", "Error",
					MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
		}

		private bool ValidateInputs()
		{
			if (string.IsNullOrWhiteSpace(_txtfldTitle.Text))
			{
				MessageBox.Show("Title is required.", "Validation Error",
					MessageBoxButtons.OK, MessageBoxIcon.Warning);
				return false;
			}

			if (!DateTime.TryParse(_txtfldRelease.Text, out _))
			{
				MessageBox.Show("Invalid release date format, please use: yyyy-MM-dd.", "Validation Error",
					MessageBoxButtons.OK, MessageBoxIcon.Warning);
				return false;
			}

			if (!int.TryParse(_txtfldDuration.Text, out _))
			{
				MessageBox.Show("Invalid duration. Must be a number.", "Validation Error",
					MessageBoxButtons.OK, MessageBoxIcon.Warning);
				return false;
			}

			if (!int.TryParse(_txtfldAgeLimit.Text, out _))
			{
				MessageBox.Show("Invalid age limit. Must be a number.", "Validation Error",
					MessageBoxButtons.OK, MessageBoxIcon.Warning);
				return false;
			}

			if (string.IsNullOrWhiteSpace(_txtboxResume.Text))
			{
				MessageBox.Show("Description is required.", "Validation Error",
					MessageBoxButtons.OK, MessageBoxIcon.Warning);
				return false;
			}

			return true;
		}

		private PostMovieCS BuildMovieDTO()
		{
			var genres = ParseGenres(_txtfldGenre.Text);
			var imageBytes = ConvertImageToBytes();

			return new PostMovieCS
			{
				Title = _txtfldTitle.Text.Trim(),
				ReleaseDate = DateTime.Parse(_txtfldRelease.Text),
				Duration = int.Parse(_txtfldDuration.Text),
				AgeLimit = int.Parse(_txtfldAgeLimit.Text),
				Genres = genres,
				Language = _languageComboBox.SelectedValue.ToString(),
				Subtitles = _subtitlesComboBox.SelectedValue.ToString(),
				TrailerYoutubeID = _txtfldEmbedLink.Text.Trim(),
				Director = _txtfldDirector.Text.Trim(),
				MainActor = _txtfldActors.Text.Trim(),
				Resume = _txtboxResume.Text.Trim(),
				Images = imageBytes != null ? new List<byte[]> { imageBytes } : new List<byte[]>()
			};
		}

		private List<string> ParseGenres(string genreText)
		{
			return genreText
				.Split(',', StringSplitOptions.RemoveEmptyEntries)
				.Select(genre => genre.Trim())
				.Where(genre => !string.IsNullOrWhiteSpace(genre))
				.ToList();
		}
	}
}
