using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace CinemaAPI.Tests
{
	public class ConnectionHandler : IDisposable
	{
		public IConfiguration Configuration { get; }
		public string ConnectionString { get; }

		public ConnectionHandler()
		{
			var configBuilder = new ConfigurationBuilder()
				.SetBasePath(Directory.GetCurrentDirectory())
				.AddJsonFile("appsettings.Test.json", optional: false, reloadOnChange: false);

			Configuration = configBuilder.Build();
			ConnectionString = Configuration.GetConnectionString("DefaultConnection")!;
		}

		public void Dispose()
		{
			// Final cleanup if needed
		}
	}
}
