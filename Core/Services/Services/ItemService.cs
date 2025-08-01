using System.Linq.Expressions;
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
    private readonly IItemRepository<ItemSizePrice> _itemSizePriceRepo;
    private readonly IMapper _mapper;

    public ItemService(
        IItemRepository<Item> itemRepo,
        IItemRepository<ItemAddOn> itemAddOnRepo,
        IItemRepository<ItemCombo> itemComboRepo,
        IItemRepository<Category> categoryRepository,
        IItemRepository<Restaurant> restaurantRepository,
         IItemRepository<AddOn> addOnRepo,
         IItemRepository<Combo> comboRepo,
         IItemRepository<ItemSizePrice> itemSizePriceRepo,
        IMapper mapper)
    {
        _itemRepo = itemRepo;
        _itemAddOnRepo = itemAddOnRepo;
        _itemComboRepo = itemComboRepo;
        _categoryRepository = categoryRepository;
        _restaurantRepository = restaurantRepository;
        _addOnRepo = addOnRepo;
        _comboRepo = comboRepo;
        _itemSizePriceRepo = itemSizePriceRepo;
        _mapper = mapper;
    }

    //public async Task AddItemAsync(ItemCreateUpdateDto dto)
    //{
    //    await ValidateItemDtoAsync(dto); // ✅ await here

    //    var item = _mapper.Map<Item>(dto);
    //    await _itemRepo.AddAsync(item);
    //    await _itemRepo.SaveChangesAsync(); // ✅ save the main item first

    //    // ✅ Add AddOns one-by-one
    //    if (dto.AddOnIds != null)
    //    {
    //        foreach (var addOnId in dto.AddOnIds)
    //        {
    //            await _itemAddOnRepo.AddAsync(new ItemAddOn
    //            {
    //                ItemId = item.Id,
    //                AddOnId = addOnId
    //            });
    //        }

    //        await _itemAddOnRepo.SaveChangesAsync(); 
    //    }


    //    if (dto.ComboIds != null)
    //    {
    //        foreach (var comboId in dto.ComboIds)
    //        {
    //            await _itemComboRepo.AddAsync(new ItemCombo
    //            {
    //                ItemId = item.Id,
    //                ComboId = comboId
    //            });
    //        }

    //        await _itemComboRepo.SaveChangesAsync(); 
    //    }
    //}

    //public async Task AddItemAsync(ItemCreateUpdateDto dto)
    //{
    //    await ValidateItemDtoAsync(dto);

    //    var item = _mapper.Map<Item>(dto);

    //    // Set SizeType-specific fields
    //    switch (dto.SizeType)
    //    {
    //        case ItemSizeType.Fixed:
    //            item.Size = null;
    //            item.Weight = null;
    //            break;

    //        case ItemSizeType.Sized:
    //            if (dto.SizePricing == null || !dto.SizePricing.Any())
    //                throw new ArgumentException("SizePricing is required for Sized items.");

    //            item.Size = dto.Size ?? 0; // Default to Small if not provided
    //            break;

    //        case ItemSizeType.Weighted:
    //            if (!dto.Weight.HasValue)
    //                throw new ArgumentException("Weight is required for Weighted items.");

    //            item.Size = null;
    //            break;
    //    }

    //    await _itemRepo.AddAsync(item);
    //    await _itemRepo.SaveChangesAsync();

    //    // Add size pricing if needed
    //    if (dto.SizeType == ItemSizeType.Sized)
    //    {
    //        foreach (var sp in dto.SizePricing)
    //        {
    //            await _itemSizePriceRepo.AddAsync(new ItemSizePrice
    //            {
    //                ItemId = item.Id,
    //                Size = sp.Size,
    //                Price = sp.Price
    //            });
    //        }
    //        await _itemSizePriceRepo.SaveChangesAsync();
    //    }

    //    // Add AddOns
    //    if (dto.AddOnIds != null)
    //    {
    //        foreach (var addOnId in dto.AddOnIds)
    //        {
    //            // Fetch the AddOn entity from the AddOn table
    //            var addOn = await _addOnRepo.GetByIdAsync(addOnId);
    //            if (addOn == null)
    //                throw new ArgumentException($"AddOn with ID {addOnId} does not exist.");

    //            var price = addOn.AdditionalPrice;
    //            var imageUrl = addOn.ImageUrl;


    //            await _itemAddOnRepo.AddAsync(new ItemAddOn
    //            {
    //                ItemId = item.Id,
    //                AddOnId = addOnId
    //            });
    //        }
    //        await _itemAddOnRepo.SaveChangesAsync();
    //    }

    //    // Add Combos
    //    if (dto.ComboIds != null)
    //    {
    //        foreach (var comboId in dto.ComboIds)
    //        {

    //            var combo = await _comboRepo.GetByIdAsync(comboId);
    //            if (combo == null)
    //                throw new ArgumentException($"Combo with ID {comboId} does not exist.");

    //            await _itemComboRepo.AddAsync(new ItemCombo
    //            {
    //                ItemId = item.Id,
    //                ComboId = comboId,
    //                AdditionalPrice = combo.ComboPrice,
    //                ImageUrl = item.ImageUrl
    //            });
    //        }
    //        await _itemComboRepo.SaveChangesAsync();
    //    }
    //}

    //public async Task AddItemAsync(ItemCreateUpdateDto dto)
    //{
    //    await ValidateItemDtoAsync(dto);

    //    var item = _mapper.Map<Item>(dto);

    //    // Set SizeType-specific fields
    //    switch (dto.SizeType)
    //    {
    //        case ItemSizeType.Fixed:
    //            item.Size = null;
    //            item.Weight = null;
    //            break;

    //        case ItemSizeType.Sized:
    //            if (dto.SizePricing == null || !dto.SizePricing.Any())
    //                throw new ArgumentException("SizePricing is required for Sized items.");

    //            item.Size = dto.Size ?? 0;
    //            break;

    //        case ItemSizeType.Weighted:
    //            if (!dto.Weight.HasValue)
    //                throw new ArgumentException("Weight is required for Weighted items.");

    //            item.Size = null;
    //            break;
    //    }

    //    await _itemRepo.AddAsync(item);
    //    await _itemRepo.SaveChangesAsync();

    //    // Add size pricing if needed
    //    if (dto.SizeType == ItemSizeType.Sized)
    //    {
    //        foreach (var sp in dto.SizePricing)
    //        {
    //            await _itemSizePriceRepo.AddAsync(new ItemSizePrice
    //            {
    //                ItemId = item.Id,
    //                Size = sp.Size,
    //                Price = sp.Price
    //            });
    //        }
    //        await _itemSizePriceRepo.SaveChangesAsync();
    //    }

    //    // Add AddOns
    //    if (dto.AddOnIds != null)
    //    {
    //        foreach (var addOnId in dto.AddOnIds)
    //        {
    //            var addOn = await _addOnRepo.GetByIdAsync(addOnId);
    //            if (addOn == null)
    //                throw new ArgumentException($"AddOn with ID {addOnId} does not exist.");

    //            await _itemAddOnRepo.AddAsync(new ItemAddOn
    //            {
    //                ItemId = item.Id,
    //                AddOnId = addOnId,
    //                AdditionalPrice = addOn.AdditionalPrice,
    //                ImageUrl = addOn.ImageUrl
    //            });
    //        }
    //        await _itemAddOnRepo.SaveChangesAsync();
    //    }

    //    // Add Combos
    //    if (dto.ComboIds != null)
    //    {
    //        foreach (var comboId in dto.ComboIds)
    //        {
    //            var combo = await _comboRepo.GetByIdAsync(comboId);
    //            if (combo == null)
    //                throw new ArgumentException($"Combo with ID {comboId} does not exist.");

    //            await _itemComboRepo.AddAsync(new ItemCombo
    //            {
    //                ItemId = item.Id,
    //                ComboId = comboId,
    //                ComboPrice = combo.ComboPrice,
    //                ImageUrl = combo.ImageUrl
    //            });
    //        }
    //        await _itemComboRepo.SaveChangesAsync();
    //    }
    //}

    //public async Task AddItemAsync(ItemCreateUpdateDto dto)
    //{
    //    await ValidateItemDtoAsync(dto);

    //    var item = _mapper.Map<Item>(dto);

    //    // Set SizeType-specific fields
    //    switch (dto.SizeType)
    //    {
    //        case ItemSizeType.Fixed:
    //            item.Size = null;
    //            item.Weight = null;
    //            break;

    //        case ItemSizeType.Sized:
    //            if (dto.SizePricing == null || !dto.SizePricing.Any())
    //                throw new ArgumentException("SizePricing is required for Sized items.");

    //            item.Size = dto.Size ?? 0;
    //            break;

    //        case ItemSizeType.Weighted:
    //            if (!dto.Weight.HasValue)
    //                throw new ArgumentException("Weight is required for Weighted items.");

    //            item.Size = null;
    //            break;
    //    }

    //    await _itemRepo.AddAsync(item);
    //    await _itemRepo.SaveChangesAsync();

    //    // Add size pricing if needed
    //    if (dto.SizeType == ItemSizeType.Sized)
    //    {
    //        foreach (var sp in dto.SizePricing)
    //        {
    //            await _itemSizePriceRepo.AddAsync(new ItemSizePrice
    //            {
    //                ItemId = item.Id,
    //                Size = sp.Size,
    //                Price = sp.Price
    //            });
    //        }
    //        await _itemSizePriceRepo.SaveChangesAsync();
    //    }

    //    // ✅ CREATE NEW ADDONS INLINE (instead of referencing existing ones)
    //    if (dto.AddOns != null && dto.AddOns.Any())
    //    {
    //        foreach (var addOnDto in dto.AddOns)
    //        {
    //            // First create the AddOn
    //            var newAddOn = new AddOn
    //            {
    //                Name = addOnDto.Name,
    //                AdditionalPrice = addOnDto.AdditionalPrice,
    //                ImageUrl = addOnDto.ImageUrl,
    //            };

    //            await _addOnRepo.AddAsync(newAddOn);
    //            await _addOnRepo.SaveChangesAsync();

    //            // Then link it to the item
    //            await _itemAddOnRepo.AddAsync(new ItemAddOn
    //            {
    //                ItemId = item.Id,
    //                AddOnId = newAddOn.Id,
    //                AdditionalPrice = newAddOn.AdditionalPrice,
    //                ImageUrl = newAddOn.ImageUrl
    //            });
    //        }
    //        await _itemAddOnRepo.SaveChangesAsync();
    //    }

    //    // ✅ CREATE NEW COMBOS INLINE (same approach)
    //    if (dto.Combos != null && dto.Combos.Any())
    //    {
    //        foreach (var comboDto in dto.Combos)
    //        {
    //            // First create the Combo
    //            var newCombo = new Combo
    //            {
    //                Name = comboDto.Name,
    //                ComboPrice = comboDto.ComboPrice,
    //                ImageUrl = comboDto.ImageUrl,
    //            };

    //            await _comboRepo.AddAsync(newCombo);
    //            await _comboRepo.SaveChangesAsync();

    //            // Then link it to the item
    //            await _itemComboRepo.AddAsync(new ItemCombo
    //            {
    //                ItemId = item.Id,
    //                ComboId = newCombo.Id,
    //                ComboPrice = newCombo.ComboPrice,
    //                ImageUrl = newCombo.ImageUrl
    //            });
    //        }
    //        await _itemComboRepo.SaveChangesAsync();
    //    }
    //}

    public async Task AddItemAsync(ItemCreateUpdateDto dto)
    {
        await ValidateItemDtoAsync(dto);

        // ✅ Create Item manually instead of using AutoMapper
        var item = new Item
        {
            Name = dto.Name,
            Description = dto.Description,
            Price = dto.Price,
            ImageUrl = dto.ImageUrl,
            CategoryId = dto.CategoryId,
            RestaurantId = dto.RestaurantId,
            SizeType = dto.SizeType,
            Size = dto.Size,
            Weight = decimal.Parse(decimal.TryParse(dto.Weight?.ToString() ?? "0", out var weight) ? weight.ToString() : "0")
        };

        // Set SizeType-specific fields
        switch (dto.SizeType)
        {
            case ItemSizeType.Fixed:
                item.Size = null;
                item.Weight = null;
                break;

            case ItemSizeType.Sized:
                if (dto.SizePricing == null || !dto.SizePricing.Any())
                    throw new ArgumentException("SizePricing is required for Sized items.");
                item.Size = dto.Size ?? 0;
                break;

            case ItemSizeType.Weighted:
                if (!dto.Weight.HasValue)
                    throw new ArgumentException("Weight is required for Weighted items.");
                item.Size = null;
                break;
        }

        await _itemRepo.AddAsync(item);
        await _itemRepo.SaveChangesAsync();

        // ✅ Handle size pricing manually
        if (dto.SizeType == ItemSizeType.Sized && dto.SizePricing != null)
        {
            foreach (var sp in dto.SizePricing)
            {
                var itemSizePrice = new ItemSizePrice
                {
                    ItemId = item.Id,
                    Size = sp.Size,
                    Price = sp.Price
                };
                await _itemSizePriceRepo.AddAsync(itemSizePrice);
            }
            await _itemSizePriceRepo.SaveChangesAsync();
        }

        // ✅ CREATE NEW ADDONS INLINE - Use Item's RestaurantId
        if (dto.AddOns != null && dto.AddOns.Any())
        {
            foreach (var addOnDto in dto.AddOns)
            {
                // Create the AddOn with the same RestaurantId as the Item
                var newAddOn = new AddOn
                {
                    Name = addOnDto.Name,
                    AdditionalPrice = addOnDto.AdditionalPrice,
                    ImageUrl = addOnDto.ImageUrl,
                    RestaurantId = item.RestaurantId // ✅ Use Item's RestaurantId
                };

                await _addOnRepo.AddAsync(newAddOn);
                await _addOnRepo.SaveChangesAsync();

                // Link it to the item
                await _itemAddOnRepo.AddAsync(new ItemAddOn
                {
                    ItemId = item.Id,
                    AddOnId = newAddOn.Id,
                    AdditionalPrice = newAddOn.AdditionalPrice,
                    ImageUrl = newAddOn.ImageUrl
                });
            }
            await _itemAddOnRepo.SaveChangesAsync();
        }

        // ✅ CREATE NEW COMBOS INLINE - Use Item's RestaurantId
        if (dto.Combos != null && dto.Combos.Any())
        {
            foreach (var comboDto in dto.Combos)
            {
                // Create the Combo with the same RestaurantId as the Item
                var newCombo = new Combo
                {
                    Name = comboDto.Name,
                    ComboPrice = comboDto.ComboPrice,
                    ImageUrl = comboDto.ImageUrl,
                    RestaurantId = item.RestaurantId // ✅ Use Item's RestaurantId
                };

                await _comboRepo.AddAsync(newCombo);
                await _comboRepo.SaveChangesAsync();

                // Link it to the item
                await _itemComboRepo.AddAsync(new ItemCombo
                {
                    ItemId = item.Id,
                    ComboId = newCombo.Id,
                    ComboPrice = newCombo.ComboPrice,
                    ImageUrl = newCombo.ImageUrl
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
    private async Task<IEnumerable<ItemClassDto>> GetItemsAsync(Expression<Func<Item, bool>> filter)
    {
        var items = await _itemRepo.GetAllWithIncludesAsync(
            filter,
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

    public Task<IEnumerable<ItemClassDto>> GetItemsByRestaurantIdAndCategoryIdAsync(int restaurantId, int categoryId)
    {
        if (restaurantId <= 0 || categoryId <= 0)
            throw new ArgumentException("Invalid restaurant or category ID.");

        return GetItemsAsync(x => x.RestaurantId == restaurantId && x.CategoryId == categoryId);
    }

    public Task<IEnumerable<ItemClassDto>> GetAllItemsByRestaurantIdAsync(int restaurantId)
    {
        if (restaurantId <= 0)
            throw new ArgumentException("Invalid restaurant ID.");

        return GetItemsAsync(x => x.RestaurantId == restaurantId);
    }
}
