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
        Task UpdateItemAsync(int id, ItemCreateUpdateDto dto);
        Task DeleteItemAsync(int id);
    }
}
