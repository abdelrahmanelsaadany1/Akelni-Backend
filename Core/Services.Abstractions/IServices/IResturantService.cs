using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Dtos;
using Domain.Dtos.ResturantDto;
using Domain.Entities;

namespace Services.Abstractions.ICategoryService
{
    public interface IResturantService
    {
        Task AddResturantAsync(Restaurant Restaurant);
        Task<Restaurant> UpdateRestaurantAsync(int restaurantId, Restaurant updatedRestaurant);
        Task DeleteRestaurantAsync(int restaurantId);
        Task<bool> CheckChefHasRestaurantAsync();
        Task<Restaurant> GetRestaurantByIdAsync(int restaurantId);
        Task<Restaurant> GetChefRestaurantAsync();
        Task<IEnumerable<Restaurant>> GetAllRestaurantsAsync();
        Task<CustomerRestaurantDto> GetCustomerRestaurantDetailsAsync(int restaurantId, int page = 1, int pageSize = 4);
        Task<RestaurantCategoryItemsDto> GetRestaurantCategoryItemsAsync(int restaurantId, int categoryId, int page = 1, int pageSize = 4);
    }
}
