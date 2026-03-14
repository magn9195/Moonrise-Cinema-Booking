using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CinemaAPI.Core.Models
{
    public class Address
    {
        public int AddressID { get; set; }
        public required int HouseNumber { get; set; }
        public required string StreetName { get; set; }
        public required CityZipcode CityZipCode { get; set; }
    }
}
