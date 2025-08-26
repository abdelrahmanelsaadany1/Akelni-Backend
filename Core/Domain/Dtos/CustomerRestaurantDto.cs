using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Dtos
{
    public class CustomerRestaurantDto
    {
        public string ResName { get; set; }
        public double Rating { get; set; }
        public string ResImage { get; set; }
        public string Location { get; set; }
        public List<CategorySummaryDto> Categories { get; set; }
        public List<ItemSummaryDto> Items { get; set; }
        public int TotalPages { get; set; }
        public int CurrentPage { get; set; }
        public int PageSize { get; set; }
        public int TotalItems { get; set; }
    }

    public class CategorySummaryDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

    public class ItemSummaryDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; }
        public string Image { get; set; }
        public string CategoryName { get; set; }
        public int CategoryId { get; set; }
    }
}