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
        Task AddItemAsync(ItemDto dto);
        Task<IEnumerable<ItemDto>> GetAllItemsAsync();
        Task<ItemDto> GetItemByIdAsync(int id);
        Task UpdateItemAsync(int id, ItemDto dto);
        Task DeleteItemAsync(int id);
    }
}
