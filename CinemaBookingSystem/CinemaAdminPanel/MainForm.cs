using CinemaAdminPanel.BusinessLogic.Interfaces;
using CinemaAdminPanel.GUI;
using Microsoft.Extensions.DependencyInjection;

namespace CinemaAdminPanel
{
    public partial class MainForm : Form
    {
		private readonly IServiceProvider _serviceProvider;

		public MainForm(IServiceProvider serviceProvider)
        {
			InitializeComponent();
			_serviceProvider = serviceProvider;

			this.Icon = Properties.Resources.LogoCinema;

			this.Size = new Size(1440, 900);

			TabControl tabControl = new TabControl();
			tabControl.Dock = DockStyle.Fill;

			TabPage movieTab = new TabPage("Movies");
			TabPage showtimeTab = new TabPage("Showtimes");
			TabPage cinemaTab = new TabPage("Cinemas");
			TabPage ticketTab = new TabPage("Tickets");

			var movieControl = _serviceProvider.GetRequiredService<MovieTabControl>();
			movieControl.Dock = DockStyle.Fill;
			movieTab.Controls.Add(movieControl);

			var showtimeControl = _serviceProvider.GetRequiredService<ShowtimeTabControl>();
			showtimeControl.Dock = DockStyle.Fill;
			showtimeTab.Controls.Add(showtimeControl);

			cinemaTab.Controls.Add(new CinemaTabControl() { Dock = DockStyle.Fill });
			ticketTab.Controls.Add(new TicketTabControl() { Dock = DockStyle.Fill });

			tabControl.TabPages.Add(movieTab);
			tabControl.TabPages.Add(showtimeTab);
			tabControl.TabPages.Add(cinemaTab);
			tabControl.TabPages.Add(ticketTab);

			this.Controls.Add(tabControl);
		}
    }
}
