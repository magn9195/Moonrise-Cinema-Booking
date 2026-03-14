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
using CinemaAdminPanel.GUI.MovieTab;
using CinemaAdminPanel.Models;
using CinemaAdminPanel.Properties;

namespace CinemaAdminPanel
{
	public partial class MovieTabControl : UserControl
	{
		private ListView? _movieListView;
		private Button? _movieDeleteButton;
		private Panel? _movieSidePanel;

		private TabControl? _movieControl;
		private MovieDetailsTab? _detailsTab;
		private MovieEditTab? _editTab;
		private MovieAddTab? _addTab;

		private readonly IMovieBusinessService _movieBusinessService;
		private GetMovieCS? _currentMovie;

		public MovieTabControl(IMovieBusinessService movieBusinessService)
		{
			InitializeComponent();
			_movieBusinessService = movieBusinessService;
			InitializeControls();
			InitializeTabs();
			LoadMoviesAsync();
		}

		private void InitializeControls()
		{
			_movieListView = new ListView();
			_movieListView.Dock = DockStyle.Fill;
			_movieListView.View = View.Details;
			_movieListView.FullRowSelect = true;
			_movieListView.GridLines = true;
			_movieListView.Columns.Add("MovieID", 50);
			_movieListView.Columns.Add("Title", 150);
			_movieListView.Columns.Add("Release Date", 60);
			_movieListView.Columns.Add("Genre", 100);
			_movieListView.Columns.Add("Duration", 80);

			_movieDeleteButton = new Button();
			_movieDeleteButton.Dock = DockStyle.Bottom;
			_movieDeleteButton.Text = "Delete Movie";
			_movieDeleteButton.Font = new Font("Arial", 15);
			_movieDeleteButton.BackColor = Color.LightCoral;
			_movieDeleteButton.FlatStyle = FlatStyle.Flat;
			_movieDeleteButton.Enabled = false;
			_movieDeleteButton.Size = new Size(100, 40);
			_movieDeleteButton.Click += MovieListView_DeleteSelectedIndex;

			_movieListView.Resize += MovieListView_Resize;
			_movieListView.SelectedIndexChanged += MovieListView_SelectedIndexChanged;

			_movieSidePanel = new Panel();
			_movieSidePanel.Dock = DockStyle.Right;
			_movieSidePanel.Width = 700;

			_movieControl = new TabControl();
			_movieControl.Dock = DockStyle.Fill;
			_movieControl.TabPages.Add(new TabPage("Details"));
			_movieControl.TabPages.Add(new TabPage("Edit"));
			_movieControl.TabPages.Add(new TabPage("Add"));

			_movieSidePanel.Controls.Add(_movieControl);
			this.Controls.Add(_movieListView);
			this.Controls.Add(_movieDeleteButton);
			this.Controls.Add(_movieSidePanel);
		}

		private void InitializeTabs()
		{
			// Create the MovieDetailsTab instance
			_detailsTab = new MovieDetailsTab();
			_detailsTab.Dock = DockStyle.Fill;
			_movieControl.TabPages[0].Controls.Add(_detailsTab);

			// Create the MovieEditTab instance
			_editTab = new MovieEditTab(_movieBusinessService);
			_editTab.Dock = DockStyle.Fill;
			_editTab.MovieUpdated += AddTab_MovieUpdated;
			_movieControl.TabPages[1].Controls.Add(_editTab);

			// Create the MovieAddTab instance
			_addTab = new MovieAddTab(_movieBusinessService);
			_addTab.Dock = DockStyle.Fill;
			_addTab.MovieCreated += AddTab_MovieCreated;
			_movieControl.TabPages[2].Controls.Add(_addTab);
		}

		private async void LoadMoviesAsync()
		{
			try
			{
				if (_movieBusinessService == null)
				{
					throw new InvalidOperationException("MovieBusinessService is not initialized.");
				}
				var movies = await _movieBusinessService.GetAllMoviesForDisplayAsync();
				if (_movieListView == null)
				{
					throw new InvalidOperationException("Movie List not Available.");
				}
				_movieListView.Items.Clear();
				foreach (var movie in movies)
				{
					if (movie != null)
					{
						var item = new ListViewItem(movie.MovieID.ToString());
						item.SubItems.Add(movie.Title);
						item.SubItems.Add(movie.FormattedReleaseDate);
						item.SubItems.Add(movie.GenreList);
						item.SubItems.Add(movie.DurationDisplay);
						_movieListView.Items.Add(item);
					}
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show($"Error loading movies: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
		}

		private async void MovieListView_SelectedIndexChanged(object? sender, EventArgs? e)
		{
			if (_movieListView != null && _movieListView.SelectedItems.Count > 0)
			{
				int selectedMovieID = int.Parse(_movieListView.SelectedItems[0].SubItems[0].Text);
				try
				{
					_currentMovie = await _movieBusinessService.GetMovieByIDAsync(selectedMovieID);

					if (_detailsTab == null || _editTab == null)
					{
						throw new InvalidOperationException("Movie detail or edit tab is not initialized.");
					}
					if (_movieDeleteButton == null)
					{
						throw new InvalidOperationException("Movie delete button is not initialized.");
					}

					if (_currentMovie != null)
					{
						_detailsTab.LoadMovie(_currentMovie);
						_editTab.LoadMovie(_currentMovie);
						_movieDeleteButton.Enabled = true;
					}
				}
				catch (Exception ex)
				{
					MessageBox.Show($"Error loading movie details: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
				}
			}
			else
			{
				try 
				{
					if (_detailsTab == null || _editTab == null || _movieDeleteButton == null)
					{
						throw new InvalidOperationException("Movie detail or edit tab or delete button is not initialized.");
					}
					_detailsTab.ClearMovie();
					_editTab.ClearMovie();
					_movieDeleteButton.Enabled = false;
				}
				catch (Exception ex)
				{
					MessageBox.Show($"Error loading movie details: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
				}
				
			}
		}

		private async void MovieListView_DeleteSelectedIndex(object? sender, EventArgs? e)
		{
			if (_movieListView != null && _movieListView.SelectedItems.Count > 0)
			{
				if (_detailsTab == null || _editTab == null)
				{
					throw new InvalidOperationException("Movie detail or edit tab is not initialized.");
				}
				int selectedMovieID = int.Parse(_movieListView.SelectedItems[0].SubItems[0].Text);
				var confirmResult = MessageBox.Show("Are you sure to delete this movie?", "Confirm Delete", MessageBoxButtons.YesNo);
				if (confirmResult == DialogResult.Yes)
				{
					try
					{
						bool success = await _movieBusinessService.DeleteMovieAsync(selectedMovieID);
						if (success)
						{
							LoadMoviesAsync();
							_detailsTab.ClearMovie();
							_editTab.ClearMovie();
						}
						else
						{
							MessageBox.Show("Failed to delete the movie.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
						}
					}
					catch (Exception ex)
					{
						MessageBox.Show($"Error deleting movie: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
					}
				}
			}
			else
			{
				MessageBox.Show("Please select a movie to delete.", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
			}
		}

		private void AddTab_MovieCreated(object? sender, EventArgs? e)
		{
			LoadMoviesAsync();
		}

		private void AddTab_MovieUpdated(object? sender, EventArgs? e)
		{
			LoadMoviesAsync();
		}

		private void MovieListView_Resize(object? sender, EventArgs? e)
		{
			if (_movieListView == null || _movieListView.Columns.Count < 5)
			{
				return;
			}
			int totalWidth = _movieListView.ClientSize.Width;
			_movieListView.Columns[0].Width = (int)(totalWidth * 0.10);
			_movieListView.Columns[1].Width = (int)(totalWidth * 0.35);
			_movieListView.Columns[2].Width = (int)(totalWidth * 0.20);
			_movieListView.Columns[3].Width = (int)(totalWidth * 0.20);
			_movieListView.Columns[4].Width = (int)(totalWidth * 0.15);
		}
	}
}
