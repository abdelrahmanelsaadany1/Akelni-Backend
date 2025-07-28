using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Dtos.Auth
{
    public enum LocationType { Governorate, City, Zone }
    public class LocationDto
    {
            public int Id { get; set; }
            public string Name { get; set; }
            public LocationType Type { get; set; }
            public int? ParentId { get; set; }
    }
}
