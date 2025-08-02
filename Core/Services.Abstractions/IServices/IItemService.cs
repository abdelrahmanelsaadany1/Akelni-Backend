using Domain.Dtos.ItemDto;
using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Abstractions.IServices
{
    public interface IItemService
    {
        Task AddItemAsync(ItemCreateUpdateDto dto);
        Task<IEnumerable<ItemClassDto>> GetAllItemsAsync();
        Task<ItemClassDto> GetItemByIdAsync(int id);
        //GetItemsByRestaurantIdWithIncludesAsync
        //Task<ItemClassDto> GetItemsByRestaurantIdWithIncludesAsync(int restaurantId);
        Task UpdateItemAsync(int id, ItemCreateUpdateDto dto);
        Task DeleteItemAsync(int id);
        // Get items by restaurant ID and category ID
        Task<IEnumerable<ItemClassDto>> GetItemsByRestaurantIdAndCategoryIdAsync(int restaurantId, int categoryId);
        Task<IEnumerable<ItemClassDto>> GetAllItemsByRestaurantIdAsync(int restaurantId);
    }
}
