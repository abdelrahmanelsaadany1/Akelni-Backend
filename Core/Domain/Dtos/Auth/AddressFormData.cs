using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Dtos.Auth
{
    public class AddressFormData
    {
        // Id - Description - Street - GovernorateId - GovernorateName - CityId - CityName - ZoneId - ZoneName - BuildingNumber - FloorNumber - ApartmentNumber -  IsDefault
        public int Id { get; set; }
        public string Description { get; set; }
        public string Street { get; set; }

        public int GovernorateId { get; set; }
        public GovernorateDto Governorate { get; set; }
        public string GovernorateName { get; set; }
        public int CityId { get; set; }
        public CityDto City { get; set; }
        public string CityName { get; set; }
        public int ZoneId { get; set; }
        public ZoneDto Zone { get; set; }

        public string ZoneName { get; set; }

        public int BuildingNumber { get; set; }
        public int FloorNumber { get; set; }
        public int ApartmentNumber { get; set; }
        public bool IsDefault { get; set; }
    }
}
