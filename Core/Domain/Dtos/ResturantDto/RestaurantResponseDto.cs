using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Dtos.ItemDto;
using Domain.Dtos.Review;

namespace Domain.Dtos.ResturantDto
{
    public  class RestaurantResponseDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Location { get; set; }
        public string ImageUrl { get; set; }
        public double Rating { get; set; }
        public string ChefId { get; set; }
        public string OpeningHours { get; set; }
        public bool IsOpen { get; set; }
        public List<ReviewDto> Reviews { get; set; }
        public List<ItemClassDto> Items { get; set; }
    }
}
