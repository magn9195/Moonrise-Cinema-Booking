using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CinemaAPI.Core.Models
{
	public class MovieImage
	{
		public int ImageID { get; set; }
		public required byte[] ImageData { get; set; }
		public int ImageIndex { get; set; }
		public int MovieID { get; set; }
	}
}
