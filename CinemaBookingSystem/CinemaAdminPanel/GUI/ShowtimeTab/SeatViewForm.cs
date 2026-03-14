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
using CinemaAdminPanel.Models.Enum;
using CinemaAdminPanel.Properties;
using Svg;

namespace CinemaAdminPanel.GUI.ShowtimeTab
{
	public partial class SeatViewForm : Form
	{
		private Panel _seatGridPanel = new();
		private Panel _legendPanel = new();
		private Label _titleLabel = new();

		private GetShowtimeCS _showtime;
		private const int SEAT_SIZE = 50;

		public SeatViewForm(GetShowtimeCS showtime)
		{
			InitializeComponent();
			InitializeComponentCustom();
			_showtime = showtime;
			InitializeControls();
			LoadSeatGrid();
		}

		private void InitializeComponentCustom()
		{
			this.Text = "Seat Grid Viewer";
			this.Size = new Size(1000, 800);
			this.StartPosition = FormStartPosition.CenterParent;
			this.FormBorderStyle = FormBorderStyle.FixedSingle;
		}

		private void InitializeControls()
		{
			_titleLabel = new Label
			{
				Dock = DockStyle.Top,
				Height = 50,
				Font = new Font("Segoe UI", 14, FontStyle.Bold),
				TextAlign = ContentAlignment.MiddleCenter,
				Text = $"{_showtime.Movie.Title} - {_showtime.StartTime:f}",
			};

			_legendPanel = new Panel
			{
				Dock = DockStyle.Top,
				Height = 80,
				Padding = new Padding(20, 10, 20, 10),
				BackColor = Color.LightGray
			};
			CreateLegend();

			_seatGridPanel = new Panel
			{
				Dock = DockStyle.Fill,
				AutoScroll = true,
				BackColor = Color.LightGray,
				Padding = new Padding(20)
			};

			this.Controls.Add(_seatGridPanel);
			this.Controls.Add(_legendPanel);
			this.Controls.Add(_titleLabel);
		}

		private void CreateLegend()
		{
			var legendLayout = new FlowLayoutPanel
			{
				Dock = DockStyle.Fill,
				FlowDirection = FlowDirection.LeftToRight,
				WrapContents = true,
				AutoSize = true,
				AutoSizeMode = AutoSizeMode.GrowAndShrink
			};

			AddLegendItem(legendLayout, "Available", "#000000");
			AddLegendItem(legendLayout, "Booked", "#FF0000");
			AddLegendItem(legendLayout, "Reserved", "#FFFB00");
			AddLegendItem(legendLayout, "Unavailable", "#808080");
			AddLegendItem(legendLayout, "Handicap", "#0000FF");

			legendLayout.Left = (_legendPanel.Width - legendLayout.PreferredSize.Width) / 2;
			legendLayout.Anchor = AnchorStyles.Top;

			_legendPanel.Controls.Add(legendLayout);
		}

		private void AddLegendItem(FlowLayoutPanel parent, string label, string colorHex)
		{
			var itemPanel = new Panel
			{
				Width = 150,
				Height = 40,
				Margin = new Padding(5)
			};

			var seatIcon = new PictureBox
			{
				Size = new Size(32, 26),
				Location = new Point(10, 7),
				Image = GetSeatImage(colorHex, 32, 26),
				SizeMode = PictureBoxSizeMode.StretchImage
			};

			var labelControl = new Label
			{
				Text = label,
				AutoSize = true,
				Location = new Point(50, 12),
				Font = new Font("Segoe UI", 9)
			};

			itemPanel.Controls.Add(seatIcon);
			itemPanel.Controls.Add(labelControl);
			parent.Controls.Add(itemPanel);
		}

		private void LoadSeatGrid()
		{
			if (_showtime?.SeatAvailability == null || !_showtime.SeatAvailability.Any())
			{
				var noSeatsLabel = new Label
				{
					Text = "No seat data available",
					Font = new Font("Segoe UI", 12),
					ForeColor = Color.Gray,
					AutoSize = true,
					Location = new Point(50, 50)
				};
				_seatGridPanel.Controls.Add(noSeatsLabel);
				return;
			}

			var containerPanel = new Panel
			{
				AutoSize = true,
				AutoSizeMode = AutoSizeMode.GrowAndShrink
			};

			var groupedSeats = _showtime.SeatAvailability.GroupBy(s => s.Seat.RowNo).OrderBy(g => g.Key).ToList();
			int yPosition = 0;

			foreach (var rowGroup in groupedSeats)
			{
				var rowPanel = CreateRowPanel(rowGroup.Key, rowGroup.OrderBy(s => s.Seat.SeatNo).ToList());
				rowPanel.Location = new Point(0, yPosition);
				containerPanel.Controls.Add(rowPanel);
				yPosition += SEAT_SIZE + 5;
			}
			_seatGridPanel.Controls.Add(containerPanel);
			containerPanel.PerformLayout();
			containerPanel.Location = new Point((_seatGridPanel.ClientSize.Width - containerPanel.Width) / 2, 20);
		}

		private Panel CreateRowPanel(int rowNumber, List<GetShowtimeSeatCS> seats)
		{
			var rowPanel = new Panel
			{
				Height = SEAT_SIZE,
				Width = seats.Count * SEAT_SIZE + 80,
				BorderStyle = BorderStyle.None
			};

			int xPosition = 70;

			foreach (var seatData in seats)
			{
				var seatPanel = CreateSeatPanel(seatData);
				seatPanel.Location = new Point(xPosition, 5);
				rowPanel.Controls.Add(seatPanel);
				xPosition += SEAT_SIZE;
			}

			return rowPanel;
		}

		private Panel CreateSeatPanel(GetShowtimeSeatCS seatData)
		{
			var panel = new Panel
			{
				Size = new Size(SEAT_SIZE, SEAT_SIZE),
				Tag = seatData
			};

			int iconWidth = (int)(SEAT_SIZE * 0.8);
			int iconHeight = (int)(SEAT_SIZE * 0.65);

			var pictureBox = new PictureBox
			{
				Size = new Size(iconWidth, iconHeight),
				Location = new Point((SEAT_SIZE - iconWidth) / 2, (SEAT_SIZE - iconHeight) / 2),
				Image = GetSeatImage(GetSeatColorHex(seatData.Status, seatData.Seat.SeatType), iconWidth, iconHeight),
				SizeMode = PictureBoxSizeMode.StretchImage
			};
			panel.Controls.Add(pictureBox);

			var tooltip = new ToolTip();
			tooltip.SetToolTip(panel, GetSeatTooltip(seatData));
			tooltip.SetToolTip(pictureBox, GetSeatTooltip(seatData));

			return panel;
		}

		private Bitmap GetSeatImage(string colorHex, int width, int height)
		{
			string svgContent = Resources.SeatTemplateSVG.Replace("{COLOR}", colorHex);

			// Load SVG from string
			using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(svgContent)))
			{
				var svgDocument = SvgDocument.Open<SvgDocument>(stream);
				return svgDocument.Draw();
			}
		}

		private string GetSeatColorHex(SeatStatusEnum status, SeatTypeEnum seatType)
		{
			switch (status)
			{
				case SeatStatusEnum.Reserved:
					return "#FFFB00"; // Yellow
				case SeatStatusEnum.Booked:
					return "#FF0000"; // Red
				case SeatStatusEnum.Unavailable:
					return "#808080"; // Gray
				case SeatStatusEnum.Available:
					if (seatType == SeatTypeEnum.Handicapped)
						return "#0000FF"; // Blue
					return "#000000"; // Black
				default:
					return "#000000"; // Black
			}
		}

		private string GetSeatTooltip(GetShowtimeSeatCS seatData)
		{
			return $"Row: {seatData.Seat.RowNo}\n" +
				   $"Seat: {seatData.Seat.SeatNo}\n" +
				   $"Type: {seatData.Seat.SeatType}\n" +
				   $"Status: {seatData.Status}";
		}
	}
}
