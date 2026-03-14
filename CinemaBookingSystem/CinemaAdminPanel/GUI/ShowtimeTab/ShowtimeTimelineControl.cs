using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using CinemaAdminPanel.GUI.DisplayModels;
using CinemaAdminPanel.Models;

namespace CinemaAdminPanel.GUI.ShowtimeTab
{
	public partial class ShowtimeTimelineControl : UserControl
	{
		private Panel _listPanel = new();
		private Label _lblSummary = new();
		private ListView _showtimesListView = new();

		public event EventHandler<int> ShowtimeSelected = null!;
		public event EventHandler ShowtimeDeselected = null!;
		public event EventHandler<int> ShowtimeDoubleClicked = null!;

		public ShowtimeTimelineControl()
		{
			InitializeComponent();
			InitializeControls();
		}

		private void InitializeControls()
		{
			// List Panel
			_listPanel = new Panel
			{
				Dock = DockStyle.Fill,
				Padding = new Padding(10)
			};

			// Summary Label
			_lblSummary = new Label
			{
				Dock = DockStyle.Top,
				Height = 30,
				Font = new Font("Segoe UI", 10, FontStyle.Bold),
				Text = "Select a cinema and auditorium to view showtimes",
				TextAlign = ContentAlignment.MiddleLeft,
				Padding = new Padding(5)
			};

			// ListView for showtimes
			_showtimesListView = new ListView
			{
				Dock = DockStyle.Fill,
				View = View.Details,
				FullRowSelect = true,
				GridLines = true,
				Font = new Font("Segoe UI", 9)
			};

			// Add columns
			_showtimesListView.Columns.Add("Start Time", 100);
			_showtimesListView.Columns.Add("End Time", 100);
			_showtimesListView.Columns.Add("Movie", 200);
			_showtimesListView.Columns.Add("Duration", 80);
			_showtimesListView.Columns.Add("Show Type", 100);
			_showtimesListView.Columns.Add("Gap After", 100);

			_listPanel.Controls.Add(_showtimesListView);
			_listPanel.Controls.Add(_lblSummary);

			_showtimesListView.Resize += ShowtimeListView_Resize;
			_showtimesListView.SelectedIndexChanged += ShowtimesListView_SelectedIndexChanged;
			_showtimesListView.DoubleClick += ShowtimesListView_DoubleClick;

			// Add to main control
			this.Controls.Add(_listPanel);
		}

		private void ShowtimesListView_SelectedIndexChanged(object? sender, EventArgs? e)
		{
			if (_showtimesListView.SelectedItems.Count > 0)
			{
				var selectedItem = _showtimesListView.SelectedItems[0];

				if (selectedItem.Tag is int showtimeID)
				{
					ShowtimeSelected?.Invoke(this, showtimeID);
				}
			}
			else
			{
				ShowtimeDeselected?.Invoke(this, EventArgs.Empty);
			}
		}

		private void ShowtimesListView_DoubleClick(object? sender, EventArgs? e)
		{
			if (_showtimesListView.SelectedItems.Count > 0)
			{
				var selectedItem = _showtimesListView.SelectedItems[0];
				if (selectedItem.Tag is int showtimeID)
				{
					ShowtimeDoubleClicked?.Invoke(this, showtimeID);
				}
			}
		}

		public void DisplayShowtimes(List<ShowtimeDM> showtimes, DateTime startDate, DateTime endDate)
		{
			_showtimesListView.Items.Clear();

			if (showtimes == null || !showtimes.Any())
			{
				_lblSummary.Text = $"No showtimes found for {startDate:MMM dd} - {endDate:MMM dd}";
				_lblSummary.ForeColor = Color.Gray;
				return;
			}

			CalculateGaps(showtimes);
			var showtimesByDate = showtimes.GroupBy(s => s.Date).OrderBy(g => g.Key);
			int totalShowtimes = 0;

			foreach (var dateGroup in showtimesByDate)
			{
				var dateHeader = new ListViewItem(dateGroup.Key.ToString("dddd, MMMM dd, yyyy"))
				{
					Font = new Font("Segoe UI", 10, FontStyle.Bold),
					ForeColor = Color.DarkBlue
				};
				_showtimesListView.Items.Add(dateHeader);

				var orderedShowtimes = dateGroup.ToList();

				foreach (var showtime in orderedShowtimes)
				{
					var item = new ListViewItem(showtime.StartTime);
					item.SubItems.Add(showtime.EndTime);
					item.SubItems.Add(showtime.MovieTitle);
					item.SubItems.Add(showtime.Duration);
					item.SubItems.Add(showtime.ShowtimeType);
					item.SubItems.Add(showtime.GapAfter);

					if (showtime.GapAfter.Contains("OVERLAP"))
					{
						item.BackColor = Color.LightCoral;
					}

					item.Tag = showtime.ShowtimeID;

					_showtimesListView.Items.Add(item);
					totalShowtimes++;
				}


			_lblSummary.Text = $"Showing {totalShowtimes} showtime(s) from {startDate:MMM dd} to {endDate:MMM dd}";
			_lblSummary.ForeColor = Color.Black;
			}
		}

		private void CalculateGaps(List<ShowtimeDM> showtimes)
		{
			var showtimesByDate = showtimes.GroupBy(s => s.Date).OrderBy(g => g.Key);
			foreach (var dateGroup in showtimesByDate)
			{
				var orderedShowtimes = dateGroup.OrderBy(s => DateTime.Parse(s.StartTime)).ToList();

				for (int i = 0; i < orderedShowtimes.Count - 1; i++)
				{
					var currentShowtime = orderedShowtimes[i];
					var nextShowtime = orderedShowtimes[i + 1];

					var endTime = DateTime.Parse(currentShowtime.EndTime);
					var nextStartTime = DateTime.Parse(nextShowtime.StartTime);

					var gap = (nextStartTime - endTime).TotalMinutes;
					currentShowtime.GapAfter = gap > 0 ? $"{gap} min" : "OVERLAP!";
				}
			}
		}

		public void ClearShowtimes()
		{
			_showtimesListView.Items.Clear();
			_lblSummary.Text = "Select a cinema and auditorium to view showtimes";
			_lblSummary.ForeColor = Color.Black;
		}

		private void ShowtimeListView_Resize(object? sender, EventArgs? e)
		{
			int totalWidth = _showtimesListView.ClientSize.Width;
			_showtimesListView.Columns[0].Width = (int)(totalWidth * 0.30);
			_showtimesListView.Columns[1].Width = (int)(totalWidth * 0.15);
			_showtimesListView.Columns[2].Width = (int)(totalWidth * 0.25);
			_showtimesListView.Columns[3].Width = (int)(totalWidth * 0.10);
			_showtimesListView.Columns[4].Width = (int)(totalWidth * 0.10);
			_showtimesListView.Columns[5].Width = (int)(totalWidth * 0.10);
		}
	}
}