using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using CinemaAdminPanel.BusinessLogic.Interfaces;
using CinemaAdminPanel.GUI.DisplayModels;
using CinemaAdminPanel.GUI.ShowtimeTab;
using CinemaAdminPanel.Models;
using CinemaAdminPanel.Models.Enum;

namespace CinemaAdminPanel
{
	public partial class ShowtimeTabControl : UserControl
	{
		private readonly ICinemaBusinessService _cinemaBusinessService;
		private readonly IShowtimeBusinessService _showtimeBusinessService;
		private readonly IMovieBusinessService _movieBusinessService;

		private ShowtimeTimelineControl _timelineControl = new();
		private ComboBox _cinemaComboBox = new(), _auditoriumComboBox = new(), _cmbDateRange = new();
		private ComboBox _addShowTypeComboBox = new(), _addMovieComboBox = new(), _editShowTypeComboBox = new(), _editMovieComboBox = new();
		private DateTimePicker _dtpStartDate = new(), _dtpEndDate = new(), _dtpAddDate = new(), _dtpAddTime = new(), _dtpEditDate = new(), _dtpEditTime = new();
		private Button _btnAddShowtime = new(), _btnEditShowtime = new(), _btnDeleteShowtime = new();

		private List<GetCinemaCS> _cinemas = [];
		private List<GetAuditoriumCS> _auditoriums = [];
		private List<ShowtimeDM> _showtimes = [];
		private GetShowtimeCS? _selectedShowtime = null;
		private List<GetMovieCS> _movies = [];
		private bool _isUpdatingDateRange = false;

		public ShowtimeTabControl(ICinemaBusinessService cinemaBusinessService, IShowtimeBusinessService showtimeBusinessService, IMovieBusinessService movieBusinessService)
		{
			InitializeComponent();
			_cinemaBusinessService = cinemaBusinessService;
			_showtimeBusinessService = showtimeBusinessService;
			_movieBusinessService = movieBusinessService;
			InitializeControls();
			LoadCinemasAsync();
			this.VisibleChanged += ShowtimeTabControl_VisibleChanged;
		}

		private async void InitializeControls()
		{
			// Control Panel
			var controlPanel = new Panel { Dock = DockStyle.Top, Height = 180, Padding = new Padding(10) };
			controlPanel.Controls.Add(CreateTopLayout());

			// Timeline Panel
			var timelinePanel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(10), AutoScroll = true };
			_timelineControl = new ShowtimeTimelineControl { Dock = DockStyle.Fill, BackColor = Color.White };
			_timelineControl.ShowtimeSelected += TimelineControl_ShowtimeSelected;
			_timelineControl.ShowtimeDeselected += TimelineControl_ShowtimeDeselected;
			_timelineControl.ShowtimeDoubleClicked += TimelineControl_ShowtimeDoubleClicked;
			timelinePanel.Controls.Add(_timelineControl);

			// Button Panel
			var buttonPanel = new Panel { Dock = DockStyle.Bottom, Height = 260, Padding = new Padding(10) };
			buttonPanel.Controls.Add(CreateButtonLayout());

			this.Controls.Add(timelinePanel);
			this.Controls.Add(controlPanel);
			this.Controls.Add(buttonPanel);

			await LoadMoviesAsync();
			PopulateShowTypeComboBoxes();
		}

		private TableLayoutPanel CreateTopLayout()
		{
			var layout = new TableLayoutPanel
			{
				Dock = DockStyle.Fill,
				ColumnCount = 2,
				RowCount = 5,
				Padding = new Padding(5)
			};
			layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 100));
			layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
			for (int i = 0; i < 5; i++) layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 30));

			// Cinema dropdown
			_cinemaComboBox = new ComboBox { Dock = DockStyle.Fill, DropDownStyle = ComboBoxStyle.DropDownList };
			_cinemaComboBox.SelectedIndexChanged += CmbCinema_SelectedIndexChanged;

			// Auditorium dropdown
			_auditoriumComboBox = new ComboBox { Dock = DockStyle.Fill, DropDownStyle = ComboBoxStyle.DropDownList, Enabled = false };
			_auditoriumComboBox.SelectedIndexChanged += CmbAuditorium_SelectedIndexChanged;

			// Quick Range Selector
			_cmbDateRange = new ComboBox { Dock = DockStyle.Fill, DropDownStyle = ComboBoxStyle.DropDownList };
			_cmbDateRange.Items.AddRange(new object[] { "Custom", "Today", "This Week", "Next Week", "This Month", "Next Month" });
			_cmbDateRange.SelectedIndex = 0;
			_cmbDateRange.SelectedIndexChanged += CmbDateRange_SelectedIndexChanged;

			// Date pickers
			_dtpStartDate = new DateTimePicker { Dock = DockStyle.Fill, Format = DateTimePickerFormat.Short, Value = DateTime.Today };
			_dtpStartDate.ValueChanged += DtpDateRange_ValueChanged;
			_dtpEndDate = new DateTimePicker { Dock = DockStyle.Fill, Format = DateTimePickerFormat.Short, Value = DateTime.Today.AddMonths(1) };
			_dtpEndDate.ValueChanged += DtpDateRange_ValueChanged;

			AddLabelAndControl(layout, "Cinema:", _cinemaComboBox, 0);
			AddLabelAndControl(layout, "Auditorium:", _auditoriumComboBox, 1);
			AddLabelAndControl(layout, "Quick Select:", _cmbDateRange, 2);
			AddLabelAndControl(layout, "Start Date:", _dtpStartDate, 3);
			AddLabelAndControl(layout, "End Date:", _dtpEndDate, 4);

			return layout;
		}

		private TableLayoutPanel CreateButtonLayout()
		{
			var btnLayout = new TableLayoutPanel
			{
				Dock = DockStyle.Fill,
				ColumnCount = 5,
				RowCount = 6,
				Padding = new Padding(80, 10, 130, 10)
			};

			// Column styles
			btnLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 100));
			btnLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 250));
			btnLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
			btnLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 100));
			btnLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 250));

			// Row styles
			for (int i = 0; i < 6; i++) btnLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 35));

			// Initialize Add controls
			_dtpAddDate = new DateTimePicker { Dock = DockStyle.Fill, Format = DateTimePickerFormat.Short, Value = DateTime.Today };
			_dtpAddTime = new DateTimePicker { Dock = DockStyle.Fill, Format = DateTimePickerFormat.Time, ShowUpDown = true };
			_addShowTypeComboBox = new ComboBox { Dock = DockStyle.Fill, DropDownStyle = ComboBoxStyle.DropDownList };
			_addMovieComboBox = new ComboBox { Dock = DockStyle.Fill, DropDownStyle = ComboBoxStyle.DropDownList };
			_btnAddShowtime = new Button { Text = "Add Showtime", Dock = DockStyle.Fill, BackColor = Color.LightGreen, FlatStyle = FlatStyle.Flat };
			_btnAddShowtime.Click += BtnAddShowtime_Click;

			// Initialize Edit controls
			_dtpEditDate = new DateTimePicker { Dock = DockStyle.Fill, Format = DateTimePickerFormat.Short, Enabled = false };
			_dtpEditTime = new DateTimePicker { Dock = DockStyle.Fill, Format = DateTimePickerFormat.Time, ShowUpDown = true, Enabled = false };
			_editShowTypeComboBox = new ComboBox { Dock = DockStyle.Fill, DropDownStyle = ComboBoxStyle.DropDownList, Enabled = false };
			_editMovieComboBox = new ComboBox { Dock = DockStyle.Fill, DropDownStyle = ComboBoxStyle.DropDownList, Enabled = false };
			_btnEditShowtime = new Button { Text = "Save Changes", Dock = DockStyle.Fill, Enabled = false, BackColor = Color.LightBlue, FlatStyle = FlatStyle.Flat };
			_btnEditShowtime.Click += BtnEditShowtime_Click;

			// Add Section
			AddTitleLabel(btnLayout, "Add Showtime", Color.DarkGreen, 0, 0);
			AddLabelAndControl(btnLayout, "Date:", _dtpAddDate, 0, 1);
			AddLabelAndControl(btnLayout, "Start Time:", _dtpAddTime, 0, 2);
			AddLabelAndControl(btnLayout, "Show Type:", _addShowTypeComboBox, 0, 3);
			AddLabelAndControl(btnLayout, "Movie:", _addMovieComboBox, 0, 4);
			btnLayout.Controls.Add(_btnAddShowtime, 1, 5);

			// Edit Section
			AddTitleLabel(btnLayout, "Edit Showtime", Color.DarkBlue, 3, 0);
			AddLabelAndControl(btnLayout, "Date:", _dtpEditDate, 3, 1);
			AddLabelAndControl(btnLayout, "Start Time:", _dtpEditTime, 3, 2);
			AddLabelAndControl(btnLayout, "Show Type:", _editShowTypeComboBox, 3, 3);
			AddLabelAndControl(btnLayout, "Movie:", _editMovieComboBox, 3, 4);
			btnLayout.Controls.Add(_btnEditShowtime, 4, 5);

			// Delete Button
			_btnDeleteShowtime = new Button
			{
				Text = "Delete Selected Showtime",
				Dock = DockStyle.Fill,
				Enabled = false,
				BackColor = Color.LightCoral,
				FlatStyle = FlatStyle.Flat,
				Font = new Font("Segoe UI", 10, FontStyle.Bold)
			};
			_btnDeleteShowtime.Click += BtnDeleteShowtime_Click;
			var deletePanelWrapper = new Panel { Dock = DockStyle.Fill, Padding = new Padding(120, 60, 100, 60) };
			deletePanelWrapper.Controls.Add(_btnDeleteShowtime);
			btnLayout.Controls.Add(deletePanelWrapper, 2, 0);
			btnLayout.SetRowSpan(deletePanelWrapper, 6);

			return btnLayout;
		}

		private void AddLabelAndControl(TableLayoutPanel layout, string labelText, Control control, int row)
		{
			var label = new Label { Text = labelText, TextAlign = ContentAlignment.MiddleRight, Dock = DockStyle.Fill };
			layout.Controls.Add(label, 0, row);
			layout.Controls.Add(control, 1, row);
		}

		private void AddLabelAndControl(TableLayoutPanel layout, string labelText, Control control, int colOffset, int row)
		{
			var label = new Label { Text = labelText, TextAlign = ContentAlignment.MiddleRight, Dock = DockStyle.Fill };
			layout.Controls.Add(label, colOffset, row);
			layout.Controls.Add(control, colOffset + 1, row);
		}

		private void AddTitleLabel(TableLayoutPanel layout, string text, Color color, int col, int row)
		{
			var label = new Label
			{
				Text = text,
				Font = new Font("Segoe UI", 12, FontStyle.Bold),
				TextAlign = ContentAlignment.MiddleCenter,
				Dock = DockStyle.Fill,
				ForeColor = color
			};
			layout.Controls.Add(label, col, row);
			layout.SetColumnSpan(label, 2);
		}

		private void PopulateShowTypeComboBoxes()
		{
			var showTypes = Enum.GetValues(typeof(ShowTypeEnum)).Cast<ShowTypeEnum>().ToList();
			_addShowTypeComboBox.DataSource = new List<ShowTypeEnum>(showTypes);
			_addShowTypeComboBox.SelectedIndex = 0;
			_editShowTypeComboBox.DataSource = new List<ShowTypeEnum>(showTypes);
		}

		private async Task LoadMoviesAsync()
		{
			var movies = new List<GetMovieCS>();
			try
			{
				movies = (await _movieBusinessService.GetAllMoviesAsync())?.OrderBy(m => m.Title).ToList();
				if (movies != null && movies.Count != 0)
				{
					_movies = movies;
				}
				if (_movies?.Any() == true)
				{
					_addMovieComboBox.DisplayMember = _editMovieComboBox.DisplayMember = "Title";
					_addMovieComboBox.ValueMember = _editMovieComboBox.ValueMember = "MovieID";
					_addMovieComboBox.DataSource = new List<GetMovieCS>(_movies);
					_editMovieComboBox.DataSource = new List<GetMovieCS>(_movies);
				}
			}
			catch (Exception ex)
			{
				ShowError($"Error loading movies: {ex.Message}");
			}
		}

		private async void ShowtimeTabControl_VisibleChanged(object? sender, EventArgs? e)
		{
			if (this.Visible)
			{
				await LoadMoviesAsync();
			}
		}

		private async void TimelineControl_ShowtimeSelected(object? sender, int showtimeID)
		{
			try
			{
				_selectedShowtime = await _showtimeBusinessService.GetShowtimeByIDAsync(showtimeID);
				if (_selectedShowtime != null)
				{
					SetEditControlsEnabled(true);

					_dtpEditDate.Value = _selectedShowtime.StartTime.Date;
					_dtpEditTime.Value = _selectedShowtime.StartTime;
					if (Enum.TryParse<ShowTypeEnum>(_selectedShowtime.ShowType, out var showTypeEnum))
						_editShowTypeComboBox.SelectedItem = showTypeEnum;
					_editMovieComboBox.SelectedValue = _selectedShowtime.Movie?.MovieID;
				}
			}
			catch (Exception ex)
			{
				ShowError($"Error loading showtime details: {ex.Message}");
			}
		}

		private async void TimelineControl_ShowtimeDoubleClicked(object? sender, int showtimeID)
		{
			try
			{
				GetShowtimeCS? currentShowtime = await _showtimeBusinessService.GetShowtimeByIDAsync(showtimeID);
				if (currentShowtime == null)
				{
					ShowError("Showtime not found.");
					return;
				}
				new SeatViewForm(currentShowtime).ShowDialog(this);
			}
			catch (Exception ex)
			{
				ShowError($"Error opening seat grid: {ex.Message}");
			}
		}

		private void TimelineControl_ShowtimeDeselected(object? sender, EventArgs? e) 
		{ 
			ClearShowtimeSelection();
		}

		private void ClearShowtimeSelection()
		{
			_selectedShowtime = null;
			SetEditControlsEnabled(false);
			_dtpEditDate.Value = DateTime.Today;
			_dtpEditTime.Value = DateTime.Now;
			if (_editShowTypeComboBox.Items.Count > 0) _editShowTypeComboBox.SelectedIndex = 0;
			if (_editMovieComboBox.Items.Count > 0) _editMovieComboBox.SelectedIndex = 0;
		}

		private void SetEditControlsEnabled(bool enabled)
		{
			_dtpEditDate.Enabled = _dtpEditTime.Enabled = _editShowTypeComboBox.Enabled =
				_editMovieComboBox.Enabled = _btnEditShowtime.Enabled = _btnDeleteShowtime.Enabled = enabled;
		}

		private async void LoadCinemasAsync()
		{
			var cinemas = new List<GetCinemaCS>();
			try
			{
				cinemas = (await _cinemaBusinessService.GetAllCinemasAsync(null))?.OrderBy(c => c.Name).ToList();
				if (cinemas != null && cinemas.Count != 0) {
					_cinemas = cinemas;
				}
				if (_cinemas?.Any() == true)
				{
					_cinemaComboBox.DisplayMember = "Name";
					_cinemaComboBox.ValueMember = "CinemaID";
					_cinemaComboBox.DataSource = _cinemas;
				}
			}
			catch (Exception ex)
			{
				ShowError($"Error loading cinemas: {ex.Message}");
			}
		}

		private async void CmbCinema_SelectedIndexChanged(object? sender, EventArgs? e)
		{
			if (_cinemaComboBox.SelectedValue == null) return;

			try
			{
				var selectedCinema = await _cinemaBusinessService.GetCinemaByIDAsync((int)_cinemaComboBox.SelectedValue);
				_auditoriumComboBox.DataSource = null;
				_auditoriumComboBox.Items.Clear();

				if (selectedCinema?.Auditoriums?.Any() == true)
				{
					_auditoriums = new List<GetAuditoriumCS>(selectedCinema.Auditoriums);
					_auditoriumComboBox.DisplayMember = "Name";
					_auditoriumComboBox.ValueMember = "AuditoriumID";
					_auditoriumComboBox.DataSource = _auditoriums;
					_auditoriumComboBox.Enabled = true;
				}
				else
				{
					_auditoriums = new List<GetAuditoriumCS>();
					_auditoriumComboBox.Enabled = false;
					_timelineControl.ClearShowtimes();
				}
			}
			catch (Exception ex)
			{
				ShowError($"Error loading auditoriums: {ex.Message}");
			}
		}

		private async void CmbAuditorium_SelectedIndexChanged(object? sender, EventArgs? e) 
		{ 
			await LoadShowtimesAsync();
		}

		private async void CmbDateRange_SelectedIndexChanged(object? sender, EventArgs? e)
		{
			_isUpdatingDateRange = true;
			var today = DateTime.Today;
			var selection = _cmbDateRange.SelectedItem?.ToString();
			bool enablePickers = true;

			switch (selection)
			{
				case "Today":
					_dtpStartDate.Value = _dtpEndDate.Value = today;
					enablePickers = false;
					break;
				case "This Week":
					_dtpStartDate.Value = today.AddDays(-(int)today.DayOfWeek);
					_dtpEndDate.Value = _dtpStartDate.Value.AddDays(6);
					enablePickers = false;
					break;
				case "Next Week":
					_dtpStartDate.Value = today.AddDays(7 - (int)today.DayOfWeek);
					_dtpEndDate.Value = _dtpStartDate.Value.AddDays(6);
					enablePickers = false;
					break;
				case "This Month":
					_dtpStartDate.Value = new DateTime(today.Year, today.Month, 1);
					_dtpEndDate.Value = _dtpStartDate.Value.AddMonths(1).AddDays(-1);
					enablePickers = false;
					break;
				case "Next Month":
					_dtpStartDate.Value = new DateTime(today.Year, today.Month, 1).AddMonths(1);
					_dtpEndDate.Value = _dtpStartDate.Value.AddMonths(1).AddDays(-1);
					enablePickers = false;
					break;
			}

			_dtpStartDate.Enabled = enablePickers;
			_dtpEndDate.Enabled = enablePickers;
			_isUpdatingDateRange = false;

			if (selection != "Custom") await LoadShowtimesAsync();
		}

		private async void DtpDateRange_ValueChanged(object? sender, EventArgs? e)
		{
			if (_isUpdatingDateRange) return;

			if (_dtpEndDate.Value < _dtpStartDate.Value)
			{
				MessageBox.Show("End date must be after start date.", "Invalid Date Range", MessageBoxButtons.OK, MessageBoxIcon.Warning);
				_dtpEndDate.Value = _dtpStartDate.Value;
				return;
			}
			await LoadShowtimesAsync();
		}

		private async Task LoadShowtimesAsync()
		{
			if (_auditoriumComboBox.SelectedValue == null) return;

			var showtimes = new List<ShowtimeDM>();
			try
			{
				var auditoriumId = (int)_auditoriumComboBox.SelectedValue;
				var startDate = _dtpStartDate.Value.Date;
				var endDate = _dtpEndDate.Value.Date;
				showtimes = (await _showtimeBusinessService.GetShowtimesByAuditoriumDateAsync(auditoriumId, startDate, endDate))?.ToList();
				if (showtimes != null && showtimes.Count != 0)
				{
					_showtimes = showtimes;
				}
				_timelineControl.DisplayShowtimes(_showtimes, startDate, endDate);
			}
			catch (Exception ex)
			{
				ShowError($"Error loading showtimes: {ex.Message}");
			}
		}

		private async void BtnAddShowtime_Click(object? sender, EventArgs? e)
		{
			if (_auditoriumComboBox.SelectedValue == null)
			{
				MessageBox.Show("Please select an auditorium first.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
				return;
			}
			if (_addMovieComboBox.SelectedValue == null)
			{
				MessageBox.Show("Please select a movie.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
				return;
			}
			if (_addShowTypeComboBox.SelectedItem == null)
			{
				MessageBox.Show("Please select a show type.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
				return;
			}

			try
			{
				var newShowtime = new PostShowtimeCS
				{
					ShowType = ((ShowTypeEnum)_addShowTypeComboBox.SelectedItem).ToString(),
					StartTime = _dtpAddDate.Value.Date.Add(_dtpAddTime.Value.TimeOfDay),
					MovieID = (int)_addMovieComboBox.SelectedValue,
					AuditoriumID = (int)_auditoriumComboBox.SelectedValue
				};

				if (await _showtimeBusinessService.CreateShowtimeAsync(newShowtime) != null)
				{
					MessageBox.Show("Showtime created successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
					await LoadShowtimesAsync();
				}
			}
			catch (Exception ex)
			{
				ShowError($"Error creating showtime: {ex.Message}");
			}
		}

		private async void BtnEditShowtime_Click(object? sender, EventArgs? e)
		{
			if (_selectedShowtime == null)
			{
				MessageBox.Show("No showtime selected.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
				return;
			}

			try
			{
				if (_editMovieComboBox.SelectedValue == null)
				{
					MessageBox.Show("Please select a movie.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
					return;
				}

				if (_editShowTypeComboBox.SelectedItem == null)
				{
					MessageBox.Show("Please select a show type.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
					return;
				}

				var updatedShowtime = new PutShowtimeCS
				{
					ShowType = ((ShowTypeEnum)_editShowTypeComboBox.SelectedItem).ToString(),
					StartTime = _dtpEditDate.Value.Date.Add(_dtpEditTime.Value.TimeOfDay),
					MovieID = (int)_editMovieComboBox.SelectedValue
				};

				if (await _showtimeBusinessService.UpdateShowtimeAsync(_selectedShowtime.ShowtimeID, updatedShowtime) != null)
				{
					MessageBox.Show("Showtime updated successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
					ClearShowtimeSelection();
					await LoadShowtimesAsync();
				}
			}
			catch (Exception ex)
			{
				ShowError($"Error updating showtime: {ex.Message}");
			}
		}

		private async void BtnDeleteShowtime_Click(object? sender, EventArgs? e)
		{
			if (_selectedShowtime == null)
			{
				MessageBox.Show("No showtime selected.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
				return;
			}

			var confirmResult = MessageBox.Show(
				$"Are you sure you want to delete the showtime for '{_selectedShowtime.Movie?.Title}' at {_selectedShowtime.StartTime:HH:mm}?",
				"Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

			if (confirmResult == DialogResult.Yes)
			{
				try
				{
					if (await _showtimeBusinessService.DeleteShowtimeAsync(_selectedShowtime.ShowtimeID))
					{
						MessageBox.Show("Showtime deleted successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
						ClearShowtimeSelection();
						await LoadShowtimesAsync();
					}
				}
				catch (Exception ex)
				{
					ShowError($"Error deleting showtime: {ex.Message}");
				}
			}
		}

		private void ShowError(string message) 
		{ 
			MessageBox.Show(message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error); 
		}
	}
}