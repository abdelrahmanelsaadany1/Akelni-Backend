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
        public string image {  get; set; }
        public string OpeningHours { get; set; } // e.g., "Mon-Fri 9:00-22:00"
        public bool IsOpen { get; set; }
        //public double Rating { get; set; }
        //public string ChefId { get; set; }
    }
}
