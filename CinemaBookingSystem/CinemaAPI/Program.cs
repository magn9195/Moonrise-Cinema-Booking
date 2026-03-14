using CinemaAPI.BusinessLogic;
using CinemaAPI.BusinessLogic.Interfaces;
using CinemaAPI.Core.DatabaseLayer;
using CinemaAPI.Core.DatabaseLayer.Interfaces;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Register Services and Database Layer for Dependency Injection
builder.Services.AddScoped<ICinemaAccess, CinemaAccess>();
builder.Services.AddScoped<IMovieAccess, MovieAccess>();
builder.Services.AddScoped<IShowtimeAccess, ShowtimeAccess>();
builder.Services.AddScoped<ITicketAccess, TicketAccess>();

builder.Services.AddScoped<ICinemaService, CinemaService>();
builder.Services.AddScoped<IMovieService, MovieService>();
builder.Services.AddScoped<IShowtimeService, ShowtimeService>();
builder.Services.AddScoped<ITicketService, TicketService>();


builder.Services.AddControllers().AddJsonOptions(options =>
{
	// Serialize enums as strings globally
	options.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
