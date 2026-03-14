using System.Text.Json;
using System.Text.Json.Serialization;
using CinemaWebService.BusinessLogic;
using CinemaWebService.BusinessLogic.Interfaces;
using CinemaWebService.Service;
using CinemaWebService.Service.Interfaces;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();


builder.Services.AddHttpClient<IMovieAPIClient, MovieAPIRestfulClient>((serviceProvider, client) =>
{
	var configuration = serviceProvider.GetRequiredService<IConfiguration>();
	var baseUrl = configuration["ApiSettings:BaseUrl"];

	client.BaseAddress = new Uri(baseUrl);
	client.DefaultRequestHeaders.Add("Accept", "application/json");
	client.Timeout = TimeSpan.FromSeconds(30);
});

builder.Services.AddHttpClient<IShowtimeAPIClient, ShowtimeAPIRestfulClient>((serviceProvider, client) =>
{
	var configuration = serviceProvider.GetRequiredService<IConfiguration>();
	var baseUrl = configuration["ApiSettings:BaseUrl"];

	client.BaseAddress = new Uri(baseUrl);
	client.DefaultRequestHeaders.Add("Accept", "application/json");
	client.Timeout = TimeSpan.FromSeconds(30);
});

builder.Services.AddHttpClient<ITicketAPIClient, TicketAPIRestfulClient>((serviceProvider, client) =>
{
	var configuration = serviceProvider.GetRequiredService<IConfiguration>();
	var baseUrl = configuration["ApiSettings:BaseUrl"];

	client.BaseAddress = new Uri(baseUrl);
	client.DefaultRequestHeaders.Add("Accept", "application/json");
	client.Timeout = TimeSpan.FromSeconds(30);
});

builder.Services.AddHttpClient<ICinemaAPIClient, CinemaAPIRestfulClient>((serviceProvider, client) =>
{
	var configuration = serviceProvider.GetRequiredService<IConfiguration>();
	var baseUrl = configuration["ApiSettings:BaseUrl"];

	client.BaseAddress = new Uri(baseUrl);
	client.DefaultRequestHeaders.Add("Accept", "application/json");
	client.Timeout = TimeSpan.FromSeconds(30);
});

// Register Services and Business Layer for Dependency Injection
builder.Services.AddScoped<IMovieBusinessService, MovieBusinessService>();
builder.Services.AddScoped<ITicketApiClient, TicketBusinessService>();
builder.Services.AddScoped<IShowtimeBusinessService, ShowtimeBusinessService>();
builder.Services.AddScoped<ICinemaBusinessService, CinemaBusinessService>();
builder.Services.AddScoped<IOrderBusinessService, OrderBusinessService>();

// Add services to the container.
builder.Services.AddSignalR();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Cinema/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
	name: "order",
	pattern: "order/{id:guid}/details",
	defaults: new { controller = "Cinema", action = "MovieOrder" });

app.MapControllerRoute(
	name: "default",
	pattern: "{controller=Cinema}/{action=Index}/{movieID?}"
);

app.MapHub<CinemaWebService.Service.SignalHub.SeatBookingHub>("/SeatBookingHub");

app.Run();
