using AutoMapper;
using Domain.Contracts;
using Domain.Contracts.Item;
using Domain.Dtos.ItemDto;
using Domain.Entities;
using Services.Abstractions.IServices;

public class ItemService : IItemService
{
    private readonly IItemRepository<Item> _itemRepo;
    private readonly IItemRepository<AddOn> _addOnRepo;
    private readonly IItemRepository<Combo> _comboRepo;
    private readonly IItemRepository<ItemAddOn> _itemAddOnRepo;
    private readonly IItemRepository<ItemCombo> _itemComboRepo;
    private readonly IItemRepository<Category> _categoryRepository;
    private readonly IItemRepository<Restaurant> _restaurantRepository;
    private readonly IMapper _mapper;

    public ItemService(
        IItemRepository<Item> itemRepo,
        IItemRepository<ItemAddOn> itemAddOnRepo,
        IItemRepository<ItemCombo> itemComboRepo,
        IItemRepository<Category> categoryRepository,
        IItemRepository<Restaurant> restaurantRepository,
         IItemRepository<AddOn> addOnRepo,
         IItemRepository<Combo> comboRepo,
        IMapper mapper)
    {
        _itemRepo = itemRepo;
        _itemAddOnRepo = itemAddOnRepo;
        _itemComboRepo = itemComboRepo;
        _categoryRepository = categoryRepository;
        _restaurantRepository = restaurantRepository;
        _addOnRepo = addOnRepo;
        _comboRepo = comboRepo;
        _mapper = mapper;
    }

    public async Task AddItemAsync(ItemCreateUpdateDto dto)
    {
        await ValidateItemDtoAsync(dto); // ✅ await here

        var item = _mapper.Map<Item>(dto);
        await _itemRepo.AddAsync(item);
        await _itemRepo.SaveChangesAsync(); // ✅ save the main item first

        // ✅ Add AddOns one-by-one
        if (dto.AddOnIds != null)
        {
            foreach (var addOnId in dto.AddOnIds)
            {
                await _itemAddOnRepo.AddAsync(new ItemAddOn
                {
                    ItemId = item.Id,
                    AddOnId = addOnId
                });
            }

            await _itemAddOnRepo.SaveChangesAsync(); 
        }

       
        if (dto.ComboIds != null)
        {
            foreach (var comboId in dto.ComboIds)
            {
                await _itemComboRepo.AddAsync(new ItemCombo
                {
                    ItemId = item.Id,
                    ComboId = comboId
                });
            }

            await _itemComboRepo.SaveChangesAsync(); 
        }
    }

    public async Task<IEnumerable<ItemClassDto>> GetAllItemsAsync()
    {
        var items = await _itemRepo.GetAllWithIncludesAsync(
            includeProperties: "ItemAddOns,ItemAddOns.AddOn,ItemCombos,ItemCombos.Combo"
        );
        return _mapper.Map<IEnumerable<ItemClassDto>>(items);
    }

    public async Task<ItemClassDto> GetItemByIdAsync(int id)
    {
        var item = await _itemRepo.GetByIdWithIncludesAsync(
            id,
            includeProperties: "ItemAddOns,ItemAddOns.AddOn,ItemCombos,ItemCombos.Combo"
        );

        if (item == null)
            throw new Exception("Item not found");

        return _mapper.Map<ItemClassDto>(item);
    }

    public async Task UpdateItemAsync(int id, ItemCreateUpdateDto dto)
    {
        ValidateItemDtoAsync(dto);

        var item = await _itemRepo.GetByIdAsync(id) ?? throw new Exception("Item not found");

        _mapper.Map(dto, item);
        _itemRepo.Update(item);

        var existingAddOns = await _itemAddOnRepo.FindAsync(x => x.ItemId == id);
        foreach (var a in existingAddOns) _itemAddOnRepo.Delete(a);

        var existingCombos = await _itemComboRepo.FindAsync(x => x.ItemId == id);
        foreach (var c in existingCombos) _itemComboRepo.Delete(c);

        if (dto.AddOnIds != null)
        {
            foreach (var addOnId in dto.AddOnIds)
                await _itemAddOnRepo.AddAsync(new ItemAddOn { ItemId = id, AddOnId = addOnId });
        }

        if (dto.ComboIds != null)
        {
            foreach (var comboId in dto.ComboIds)
                await _itemComboRepo.AddAsync(new ItemCombo { ItemId = id, ComboId = comboId });
        }

        await _itemRepo.SaveChangesAsync();
        await _itemAddOnRepo.SaveChangesAsync();
        await _itemComboRepo.SaveChangesAsync();
    }


    public async Task DeleteItemAsync(int id)
    {
        var item = await _itemRepo.GetByIdAsync(id);
        if (item == null)
            throw new Exception("Item not found");

        _itemRepo.Delete(item);
        await _itemRepo.SaveChangesAsync();
    }
    
    public async Task<bool> ExistsAsync(int id)
    {
        return await _itemRepo.ExistsAsync(id);
    }
    private async Task ValidateItemDtoAsync(ItemCreateUpdateDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Name))
            throw new ArgumentException("Item name is required.");

        if (dto.Price <= 0)
            throw new ArgumentException("Item price must be greater than zero.");

        if (dto.CategoryId <= 0)
            throw new ArgumentException("Valid category ID is required.");

        if (dto.RestaurantId <= 0)
            throw new ArgumentException("Valid restaurant ID is required.");

        var categoryExists = await _categoryRepository.ExistsAsync(dto.CategoryId);
        if (!categoryExists)
            throw new ArgumentException($"Category with ID {dto.CategoryId} does not exist.");

        // NEW: Check if Restaurant exists
        var restaurantExists = await _restaurantRepository.ExistsAsync(dto.RestaurantId);
        if (!restaurantExists)
            throw new ArgumentException($"Restaurant with ID {dto.RestaurantId} does not exist.");

        if (dto.AddOnIds != null)
        {
            foreach (var addOnId in dto.AddOnIds)
            {
                var exists = await _addOnRepo.ExistsAsync(addOnId);
                if (!exists)
                    throw new ArgumentException($"AddOn with ID {addOnId} does not exist.");
            }
        }

        if (dto.ComboIds != null)
        {
            foreach (var comboId in dto.ComboIds)
            {
                var exists = await _comboRepo.ExistsAsync(comboId);
                if (!exists)
                    throw new ArgumentException($"Combo with ID {comboId} does not exist.");
            }
        }
    }

  
}
