using AutoMapper;
using Domain.Contracts;
using Domain.Dtos.ItemDto;
using Domain.Entities;
using Services.Abstractions.IServices;

namespace Services.Services
{
    public class ItemService : IItemService
    {
        private readonly IGenericRepository<Item> _itemRepository;
        private readonly IMapper _mapper;

        public ItemService(IGenericRepository<Item> itemRepository, IMapper mapper)
        {
            _itemRepository = itemRepository;
            _mapper = mapper;
        }

        public async Task AddItemAsync(ItemClassDto dto)
        {
            var item = _mapper.Map<Item>(dto);
            await _itemRepository.AddAsync(item);
            await _itemRepository.SaveChangesAsync();
        }

        public async Task<IEnumerable<ItemClassDto>> GetAllItemsAsync()
        {
            var items = await _itemRepository.GetAllAsync();
            return items.Select(i => _mapper.Map<ItemClassDto>(i));
        }

        public async Task<ItemClassDto> GetItemByIdAsync(int id)
        {
            var item = await _itemRepository.GetByIdAsync(id);
            if (item == null)
                throw new Exception("Item not found.");

            return _mapper.Map<ItemClassDto>(item);
        }

        public async Task UpdateItemAsync(int id, ItemClassDto dto)
        {
            var existingItem = await _itemRepository.GetByIdAsync(id);
            if (existingItem == null)
                throw new Exception("Item not found.");

            _mapper.Map(dto, existingItem); 

            _itemRepository.Update(existingItem);
            await _itemRepository.SaveChangesAsync();
        }

        public async Task DeleteItemAsync(int id)
        {
            var item = await _itemRepository.GetByIdAsync(id);
            if (item == null)
                throw new Exception("Item not found.");

            _itemRepository.Delete(item);

            try
            {
                await _itemRepository.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                // log or return the actual DB error
                throw new Exception("Failed to delete item. Inner: " + ex.InnerException?.Message ?? ex.Message);
            }
        }
    }
}
