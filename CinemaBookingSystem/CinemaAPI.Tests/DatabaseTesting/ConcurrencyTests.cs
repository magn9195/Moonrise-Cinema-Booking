using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CinemaAPI.Core.DatabaseLayer;
using CinemaAPI.Core.Models;
using Dapper;
using Microsoft.Data.SqlClient;

namespace CinemaAPI.Tests.DatabaseTesting
{
	[Collection("Database Tests")]
	public class ConcurrencyTests : IDisposable
	{
		private readonly ConnectionHandler _handler;
		private readonly string _connectionString;

		public ConcurrencyTests(ConnectionHandler handler)
		{
			_handler = handler;
			_connectionString = _handler.ConnectionString;
			SeedTestData();
		}

		[Fact]
		public async Task ReserveSeatsAsync_TwoTransactionsSameSeat_OnlyOneSucceeds()
		{
			// Arrange - Transaction 1 and Transaction 2 both try to reserve the same seat
			int showtimeId = 1;
			int seatId = 1;
			var showtimeAccess1 = new ShowtimeAccess(_handler.Configuration);
			var showtimeAccess2 = new ShowtimeAccess(_handler.Configuration);
			var ticketAccess1 = new TicketAccess(_handler.Configuration);
			var ticketAccess2 = new TicketAccess(_handler.Configuration);

			var seat1 = await showtimeAccess1.GetShowtimeSeatByIDAsync(showtimeId, seatId);
			var seat2 = await showtimeAccess2.GetShowtimeSeatByIDAsync(showtimeId, seatId);

			List<ShowtimeSeat> seats1 = new List<ShowtimeSeat> { seat1 };
			List<ShowtimeSeat> seats2 = new List<ShowtimeSeat> { seat2 };

			// Act
			var task1 = Task.Run(() => ticketAccess1.ReserveSeatsAsync(seats1, DateTime.Now.AddMinutes(15), 2000));
			var task2 = Task.Run(() => ticketAccess2.ReserveSeatsAsync(seats2, DateTime.Now.AddMinutes(15), 2000));

			var results = await Task.WhenAll(task1, task2);

			// Assert
			var successfulReservations = results.Count(r => r == true);
			Assert.Equal(1, successfulReservations);
		}

		[Fact]
		public async Task ReserveSeatsAsync_TwoTransactionsOverlappingSeats_OnlyOneSucceeds()
		{
			// Arrange - Transaction 1 reserves seats 1, 2 and 3. Transaction 2 reserves seat 2.
			int showtimeId = 1;
			var showtimeAccess1 = new ShowtimeAccess(_handler.Configuration);
			var showtimeAccess2 = new ShowtimeAccess(_handler.Configuration);
			var ticketAccess1 = new TicketAccess(_handler.Configuration);
			var ticketAccess2 = new TicketAccess(_handler.Configuration);

			var t1Seat1 = await showtimeAccess1.GetShowtimeSeatByIDAsync(showtimeId, 1);
			var t1Seat2 = await showtimeAccess1.GetShowtimeSeatByIDAsync(showtimeId, 2);
			var t1Seat3 = await showtimeAccess1.GetShowtimeSeatByIDAsync(showtimeId, 3);
			var t2Seat2 = await showtimeAccess2.GetShowtimeSeatByIDAsync(showtimeId, 2);

			List<ShowtimeSeat> seats1 = new List<ShowtimeSeat> { t1Seat1, t1Seat2, t1Seat3 };
			List<ShowtimeSeat> seats2 = new List<ShowtimeSeat> { t2Seat2 };

			// Act - Transaction 2 finishes first due to shorter artificial delay
			var task1 = Task.Run(() => ticketAccess1.ReserveSeatsAsync(seats1, DateTime.Now.AddMinutes(15), 2000));
			var task2 = Task.Run(() => ticketAccess2.ReserveSeatsAsync(seats2, DateTime.Now.AddMinutes(15), 1500));

			var results = await Task.WhenAll(task1, task2);

			// Assert
			var successfulReservations = results.Count(r => r == true);
			Assert.Equal(1, successfulReservations);
		}

		[Fact]
		public async Task ReserveSeatsAsync_TwoTransactionsReverseOrder_OnlyOneSucceeds()
		{
			// Arrange - Transaction 1 reserves seats 1, 2 and 3. Transaction 2 reserves seats 3, 2 and 1.
			int showtimeId = 1;
			var showtimeAccess1 = new ShowtimeAccess(_handler.Configuration);
			var showtimeAccess2 = new ShowtimeAccess(_handler.Configuration);
			var ticketAccess1 = new TicketAccess(_handler.Configuration);
			var ticketAccess2 = new TicketAccess(_handler.Configuration);

			var t1Seat1 = await showtimeAccess1.GetShowtimeSeatByIDAsync(showtimeId, 1);
			var t1Seat2 = await showtimeAccess1.GetShowtimeSeatByIDAsync(showtimeId, 2);
			var t1Seat3 = await showtimeAccess1.GetShowtimeSeatByIDAsync(showtimeId, 3);
			var t2Seat3 = await showtimeAccess2.GetShowtimeSeatByIDAsync(showtimeId, 3);
			var t2Seat2 = await showtimeAccess2.GetShowtimeSeatByIDAsync(showtimeId, 2);
			var t2Seat1 = await showtimeAccess2.GetShowtimeSeatByIDAsync(showtimeId, 1);

			List<ShowtimeSeat> seats1 = new List<ShowtimeSeat> { t1Seat1, t1Seat2, t1Seat3 };
			List<ShowtimeSeat> seats2 = new List<ShowtimeSeat> { t2Seat3, t2Seat2, t2Seat1 };

			// Act - Transaction 2 finishes first due to artificial delay
			var task1 = Task.Run(() => ticketAccess1.ReserveSeatsAsync(seats1, DateTime.Now.AddMinutes(15), 2000));
			var task2 = Task.Run(() => ticketAccess2.ReserveSeatsAsync(seats2, DateTime.Now.AddMinutes(15), 1500));

			var results = await Task.WhenAll(task1, task2);

			// Assert
			var successfulReservations = results.Count(r => r == true);
			Assert.Equal(1, successfulReservations);
		}

		[Fact]
		public async Task ReserveSeatsAsync_MultipleTransactionsSeparateSeats_AllSucceed() 
		{
			// Arrange - Each transaction books different seats
			int showtimeId = 1;
			var ticketAccess1 = new TicketAccess(_handler.Configuration);
			var ticketAccess2 = new TicketAccess(_handler.Configuration);
			var ticketAccess3 = new TicketAccess(_handler.Configuration);
			var showtimeAccess1 = new ShowtimeAccess(_handler.Configuration);
			var showtimeAccess2 = new ShowtimeAccess(_handler.Configuration);
			var showtimeAccess3 = new ShowtimeAccess(_handler.Configuration);

			var seat1 = await showtimeAccess1.GetShowtimeSeatByIDAsync(showtimeId, 1);
			var seat2 = await showtimeAccess2.GetShowtimeSeatByIDAsync(showtimeId, 2);
			var seat3 = await showtimeAccess3.GetShowtimeSeatByIDAsync(showtimeId, 3);

			List<ShowtimeSeat> seats1 = new List<ShowtimeSeat> { seat1 };
			List<ShowtimeSeat> seats2 = new List<ShowtimeSeat> { seat2 };
			List<ShowtimeSeat> seats3 = new List<ShowtimeSeat> { seat3 };

			// Act
			var task1 = Task.Run(() => ticketAccess1.ReserveSeatsAsync(seats1, DateTime.Now.AddMinutes(15), 500));
			var task2 = Task.Run(() => ticketAccess2.ReserveSeatsAsync(seats2, DateTime.Now.AddMinutes(15), 100));
			var task3 = Task.Run(() => ticketAccess3.ReserveSeatsAsync(seats3, DateTime.Now.AddMinutes(15), 250));

			var results = await Task.WhenAll(task1, task2, task3);

			// Assert
			var successfulReservations = results.Count(r => r == true);
			Assert.Equal(3, successfulReservations);
		}

		[Fact]
		public async Task ReserveSeatsAsync_DelayedReadInvalidRowVersion_FasterOneSucceeds()
		{
			// Arrange - T1 reads seat data, delays significantly, then tries to reserve
			int showtimeId = 1;
			int seatId = 1;
			var showtimeAccess1 = new ShowtimeAccess(_handler.Configuration);
			var showtimeAccess2 = new ShowtimeAccess(_handler.Configuration);
			var ticketAccess1 = new TicketAccess(_handler.Configuration);
			var ticketAccess2 = new TicketAccess(_handler.Configuration);

			var seat1 = await showtimeAccess1.GetShowtimeSeatByIDAsync(showtimeId, seatId);

			// Act
			var task1 = Task.Run(async () => {
				await Task.Delay(1000);
				return await ticketAccess1.ReserveSeatsAsync(
					new List<ShowtimeSeat> { seat1 },
					DateTime.Now.AddMinutes(15),
					0);
			});
			await Task.Delay(100);
			var seat2 = await showtimeAccess2.GetShowtimeSeatByIDAsync(showtimeId, seatId);

			var result2 = await ticketAccess2.ReserveSeatsAsync(
				new List<ShowtimeSeat> { seat2 },
				DateTime.Now.AddMinutes(15),
				0);

			var result1 = await task1;

			// Assert - T2 succeeds, T1 fails due to invalid RowVersion
			Assert.True(result2);
			Assert.False(result1);
		}

		[Fact]
		public async Task ReserveSeatsAsync_StressTest_OneSucceeds()
		{
			// Arrange - 10 transactions try to reserve the same seat with random delays/timings
			int showtimeId = 1;
			int seatId = 4;
			var tasks = new List<Task<bool>>();
			var random = new Random();

			// Act
			for (int i = 0; i < 10; i++)
			{
				var showtimeAccess = new ShowtimeAccess(_handler.Configuration);
				var ticketAccess = new TicketAccess(_handler.Configuration);
				var seat = await showtimeAccess.GetShowtimeSeatByIDAsync(showtimeId, seatId);
				List<ShowtimeSeat> seats = new List<ShowtimeSeat> { seat };

				var delay = random.Next(0, 300);
				tasks.Add(Task.Run(() => ticketAccess.ReserveSeatsAsync(seats, DateTime.Now.AddMinutes(15), delay)));
			}
			var results = await Task.WhenAll(tasks);

			// Assert
			var successfulReservations = results.Count(r => r == true);
			Assert.Equal(1, successfulReservations);

		}

		private void SeedTestData()
		{
			using var connection = new SqlConnection(_connectionString);
			connection.Open();
			connection.Execute("DELETE FROM ShowtimeSeat");
			connection.Execute("DELETE FROM TicketTypeQuantity");
			connection.Execute("DELETE FROM Ticket");
			connection.Execute("DELETE FROM Showtime");
			connection.Execute("DELETE FROM Seat");
			connection.Execute("DELETE FROM Auditorium");
			connection.Execute("DELETE FROM Cinema");
			connection.Execute("DELETE FROM Address");
			connection.Execute("DELETE FROM CityZipcode");
			connection.Execute("DELETE FROM MovieGenre");
			connection.Execute("DELETE FROM MovieImage");
			connection.Execute("DELETE FROM Movie");
			connection.Execute("DELETE FROM Genre");
			connection.Execute("DELETE FROM Customer");
			connection.Execute("DELETE FROM TicketType");

			connection.Execute("DBCC CHECKIDENT ('Seat', RESEED, 0)");
			connection.Execute("DBCC CHECKIDENT ('Ticket', RESEED, 0)");
			connection.Execute("DBCC CHECKIDENT ('Customer', RESEED, 0)");

			connection.Execute(@"
                INSERT INTO Genre (GenreID, Name)
                VALUES (1, 'Action')
            ");

			connection.Execute(@"
                INSERT INTO Movie (MovieID, Title, ReleaseDate, AgeLimit, Duration, 
                    TrailerYouTubeID, Resume, Language, Subtitles, Director, MainActor)
                VALUES 
                (1, 'Test Movie', '2024-01-01', 13, 120, 'abc123', 'Test Resume', 0, 0, 'Director', 'Actor')
            ");

			connection.Execute(@"
                INSERT INTO MovieGenre (MovieID, GenreID)
                VALUES (1, 1)
            ");

			connection.Execute(@"
                INSERT INTO CityZipcode (CityZipCodeID, Zipcode, City)
                VALUES (1, '12345', 'Test City')
            ");

			connection.Execute(@"
                INSERT INTO Address (AddressID, HouseNumber, StreetName, CityZipCodeID)
                VALUES (1, 123, 'Test Street', 1)
            ");

			connection.Execute(@"
                INSERT INTO Cinema (CinemaID, Name, AddressID)
                VALUES (1, 'Test Cinema', 1)
            ");

			connection.Execute(@"
                INSERT INTO Auditorium (AuditoriumID, Name, RowNum, seatsPerRow, CinemaID)
                VALUES (1, 'Bio1', 1, 5, 1)
            ");

			connection.Execute(@"
				INSERT INTO Seat (rowNo, seatNo, seatType, auditoriumID)
				VALUES 
				(1, 1, 1, 1),
				(1, 2, 1, 1),
				(1, 3, 1, 1),
				(1, 4, 1, 1),
				(1, 5, 1, 1)
			");

			connection.Execute(@"
                INSERT INTO Showtime (ShowtimeID, ShowType, StartTime, MovieID, AuditoriumID)
                VALUES (1, 0, '2024-12-01 19:00', 1, 1)
            ");

			connection.Execute(@"
				INSERT INTO TicketType (type, currentPrice, description)
				VALUES 
				(0, 140.0, 'Adult ticket'),
				(1, 100.0, 'Child ticket')
			");
		}

		public void Dispose()
		{
			using var connection = new SqlConnection(_connectionString);
			connection.Execute("DELETE FROM ShowtimeSeat");
			connection.Execute("DELETE FROM TicketTypeQuantity");
			connection.Execute("DELETE FROM Ticket");
			connection.Execute("DELETE FROM Showtime");
			connection.Execute("DELETE FROM Seat");
			connection.Execute("DELETE FROM Auditorium");
			connection.Execute("DELETE FROM Cinema");
			connection.Execute("DELETE FROM Address");
			connection.Execute("DELETE FROM CityZipcode");
			connection.Execute("DELETE FROM MovieGenre");
			connection.Execute("DELETE FROM MovieImage");
			connection.Execute("DELETE FROM Movie");
			connection.Execute("DELETE FROM Genre");
			connection.Execute("DELETE FROM Customer");
			connection.Execute("DELETE FROM TicketType");
		}
	}
}
