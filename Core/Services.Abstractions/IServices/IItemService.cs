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
        Task AddItemAsync(ItemClassDto dto);
        Task<IEnumerable<ItemClassDto>> GetAllItemsAsync();
        Task<ItemClassDto> GetItemByIdAsync(int id);
        Task UpdateItemAsync(int id, ItemClassDto dto);
        Task DeleteItemAsync(int id);
    }
}
