using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Dtos.ResturantDto
{
    public class RestaurantInputDto
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string Location { get; set; }
        public string ImageUrl { get; set; }
        public string OpeningHours { get; set; }
        public bool IsOpen { get; set; }
    }
}
