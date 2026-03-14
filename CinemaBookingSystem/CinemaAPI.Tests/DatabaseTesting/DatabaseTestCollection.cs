using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CinemaAPI.Tests.DatabaseTesting
{
	[CollectionDefinition("Database Tests", DisableParallelization = true)]
	public class DatabaseTestCollection : ICollectionFixture<ConnectionHandler>
	{
	}
}
