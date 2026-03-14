using CinemaAdminPanel.BusinessLogic;
using CinemaAdminPanel.BusinessLogic.Interfaces;
using CinemaAdminPanel.Service.Interfaces;
using CinemaAdminPanel.Service;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http;

namespace CinemaAdminPanel
{
    internal static class Program
    {
		/// <summary>
		///  The main entry point for the application.
		/// </summary>
		[STAThread]
        static void Main()
        {
            // To customize application configuration such as set high DPI settings or default font,
            // see https://aka.ms/applicationconfiguration.
            ApplicationConfiguration.Initialize();
			var services = new ServiceCollection();

			services.AddHttpClient<IMovieAPIClient, MovieAPIRestfulClient>(client =>
			{
				client.BaseAddress = new Uri("https://localhost:7078/");
			});

			services.AddHttpClient<IShowtimeAPIClient, ShowtimeAPIRestfulClient>(client =>
			{
				client.BaseAddress = new Uri("https://localhost:7078/");
			});

			services.AddHttpClient<ICinemaAPIClient, CinemaAPIRestfulClient>(client =>
			{
				client.BaseAddress = new Uri("https://localhost:7078/");
			});

			services.AddTransient<IMovieBusinessService, MovieBusinessService>();
			services.AddTransient<IShowtimeBusinessService, ShowtimeBusinessService>();
			services.AddTransient<ICinemaBusinessService, CinemaBusinessService>();

			services.AddTransient<MainForm>();
			services.AddTransient<MovieTabControl>();
			services.AddTransient<ShowtimeTabControl>();

			var serviceProvider = services.BuildServiceProvider();
			var mainForm = serviceProvider.GetRequiredService<MainForm>();
			Application.Run(mainForm);
        }
    }
}