using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CinemaAPI.Core.Models
{
    public class CityZipcode
    {
        public int CityZipCodeID { get; set; }
        public required string ZipCode { get; set; }
        public required string City { get; set; }
    }
}
