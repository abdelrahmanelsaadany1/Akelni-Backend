using System.Linq.Expressions;
using AutoMapper;
using Domain.Contracts;
using Domain.Contracts.Item;
using Domain.Dtos.AddOnDto;
using Domain.Dtos.ComboDto;
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
            includeProperties: "ItemAddOns,ItemAddOns.AddOn,ItemCombos,ItemCombos.Combo,SizePricing"
        );
        return _mapper.Map<IEnumerable<ItemClassDto>>(items);
    }
    private async Task<IEnumerable<ItemClassDto>> GetItemsAsync(Expression<Func<Item, bool>> filter)
    {
        var items = await _itemRepo.GetAllWithIncludesAsync(
            filter,
            includeProperties: "ItemAddOns,ItemAddOns.AddOn,ItemCombos,ItemCombos.Combo,SizePricing"
        );

        return _mapper.Map<IEnumerable<ItemClassDto>>(items);
    }

    public async Task<List<ItemClassDto>> GetItemsByRestaurantIdAsync(int restaurantId)
    {
        var items = await _itemRepo.GetItemsByRestaurantIdWithIncludesAsync(
            restaurantId,
            includeProperties: "ItemAddOns,ItemAddOns.AddOn,ItemCombos,ItemCombos.Combo,SizePricing"
        );

        var result = new List<ItemClassDto>();

        foreach (var item in items)
        {
            // ✅ Get size pricing for each item
            var sizePricing = await _itemSizePriceRepo.FindAsync(x => x.ItemId == item.Id);

            result.Add(new ItemClassDto
            {
                Id = item.Id,
                Name = item.Name,
                Description = item.Description,
                Price = item.Price,
                SizeType = item.SizeType,
                Size = item.Size,
                Weight = item.Weight.HasValue ? (float)item.Weight.Value : null,
                ImageUrl = item.ImageUrl,
                CategoryId = item.CategoryId,
                RestaurantId = item.RestaurantId,

                //// ✅ Include size pricing in the list response
                //SizePricing = sizePricing.Select(sp => new ItemSizePriceDto
                //{
                //    Size = sp.Size,
                //    Price = sp.Price
                //}).ToList(),

                SizePricing = item.SizePricing?.Select(sp => new ItemSizePriceDto
                {
                    Size = sp.Size,
                    Price = sp.Price
                }).ToList() ?? new List<ItemSizePriceDto>(),

                AddOns = item.ItemAddOns?.Select(ia => new AddOnGetDto
                {
                    Id = ia.AddOn.Id,
                    Name = ia.AddOn.Name,
                    AdditionalPrice = ia.AdditionalPrice,
                    ImageUrl = ia.ImageUrl
                }).ToList(),

                Combos = item.ItemCombos?.Select(ic => new ComboGetDto
                {
                    Id = ic.Combo.Id,
                    Name = ic.Combo.Name,
                    ComboPrice = ic.ComboPrice,
                    ImageUrl = ic.ImageUrl
                }).ToList()
            });
        }

        return result;
    }

    // Get Item Details
    public async Task<ItemClassDto> GetItemDetailsByIdAsync(int id)
    {
        if (id <= 0)
            throw new ArgumentException("Invalid item ID.");

        var item = await _itemRepo.GetByIdWithIncludesAsync(
            id,
            includeProperties: "ItemAddOns,ItemAddOns.AddOn,ItemCombos,ItemCombos.Combo,SizePricing"
        );

        if (item == null)
            throw new Exception("Item not found");

        // ✅ Get size pricing for the item (fallback in case includes didn't work)
        var sizePricing = await _itemSizePriceRepo.FindAsync(x => x.ItemId == item.Id);

        return new ItemClassDto
        {
            Id = item.Id,
            Name = item.Name,
            Description = item.Description,
            Price = item.Price,
            SizeType = item.SizeType,
            Size = item.Size,
            Weight = item.Weight.HasValue ? (float)item.Weight.Value : null,
            ImageUrl = item.ImageUrl,
            CategoryId = item.CategoryId,
            RestaurantId = item.RestaurantId,

            // ✅ Include size pricing with fallback
            SizePricing = item.SizePricing?.Any() == true
                ? item.SizePricing.Select(sp => new ItemSizePriceDto
                {
                    Size = sp.Size,
                    Price = sp.Price
                }).ToList()
                : sizePricing.Select(sp => new ItemSizePriceDto
                {
                    Size = sp.Size,
                    Price = sp.Price
                }).ToList(),

            AddOns = item.ItemAddOns?.Select(ia => new AddOnGetDto
            {
                Id = ia.AddOn.Id,
                Name = ia.AddOn.Name,
                AdditionalPrice = ia.AdditionalPrice,
                ImageUrl = ia.ImageUrl
            }).ToList(),

            Combos = item.ItemCombos?.Select(ic => new ComboGetDto
            {
                Id = ic.Combo.Id,
                Name = ic.Combo.Name,
                ComboPrice = ic.ComboPrice,
                ImageUrl = ic.ImageUrl
            }).ToList()
        };
    }

    public async Task UpdateItemAsync(int id, ItemCreateUpdateDto dto)
    {
        await ValidateItemDtoAsync(dto);

        var item = await _itemRepo.GetByIdAsync(id) ?? throw new Exception("Item not found");

        // Manually update properties
        item.Name = dto.Name;
        item.Description = dto.Description;
        item.Price = dto.Price;
        item.ImageUrl = dto.ImageUrl;
        item.CategoryId = dto.CategoryId;
        item.RestaurantId = dto.RestaurantId;
        item.SizeType = dto.SizeType;
        item.Size = dto.Size;
        item.Weight = dto.Weight.HasValue ? (decimal?)dto.Weight.Value : null;

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

        _itemRepo.Update(item);

        // Remove old size pricing
        var existingSizePricing = await _itemSizePriceRepo.FindAsync(x => x.ItemId == id);
        foreach (var sp in existingSizePricing)
            _itemSizePriceRepo.Delete(sp);

        // Add new size pricing if needed
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

        // Remove old AddOns and Combos
        var existingAddOns = await _itemAddOnRepo.FindAsync(x => x.ItemId == id);
        foreach (var a in existingAddOns) _itemAddOnRepo.Delete(a);

        var existingCombos = await _itemComboRepo.FindAsync(x => x.ItemId == id);
        foreach (var c in existingCombos) _itemComboRepo.Delete(c);

        // Add AddOns by IDs
        if (dto.AddOnIds != null)
        {
            foreach (var addOnId in dto.AddOnIds)
            {
                var addOn = await _addOnRepo.GetByIdAsync(addOnId);
                if (addOn == null)
                    throw new ArgumentException($"AddOn with ID {addOnId} does not exist.");

                await _itemAddOnRepo.AddAsync(new ItemAddOn
                {
                    ItemId = id,
                    AddOnId = addOnId,
                    AdditionalPrice = addOn.AdditionalPrice,
                    ImageUrl = addOn.ImageUrl
                });
            }
        }

        // Add new AddOns inline
        if (dto.AddOns != null && dto.AddOns.Any())
        {
            foreach (var addOnDto in dto.AddOns)
            {
                var newAddOn = new AddOn
                {
                    Name = addOnDto.Name,
                    AdditionalPrice = addOnDto.AdditionalPrice,
                    ImageUrl = addOnDto.ImageUrl,
                    RestaurantId = item.RestaurantId
                };
                await _addOnRepo.AddAsync(newAddOn);
                await _addOnRepo.SaveChangesAsync();

                await _itemAddOnRepo.AddAsync(new ItemAddOn
                {
                    ItemId = id,
                    AddOnId = newAddOn.Id,
                    AdditionalPrice = newAddOn.AdditionalPrice,
                    ImageUrl = newAddOn.ImageUrl
                });
            }
        }

        // Add Combos by IDs
        if (dto.ComboIds != null)
        {
            foreach (var comboId in dto.ComboIds)
            {
                var combo = await _comboRepo.GetByIdAsync(comboId);
                if (combo == null)
                    throw new ArgumentException($"Combo with ID {comboId} does not exist.");

                await _itemComboRepo.AddAsync(new ItemCombo
                {
                    ItemId = id,
                    ComboId = comboId,
                    ComboPrice = combo.ComboPrice,
                    ImageUrl = combo.ImageUrl
                });
            }
        }

        // Add new Combos inline
        if (dto.Combos != null && dto.Combos.Any())
        {
            foreach (var comboDto in dto.Combos)
            {
                var newCombo = new Combo
                {
                    Name = comboDto.Name,
                    ComboPrice = comboDto.ComboPrice,
                    ImageUrl = comboDto.ImageUrl,
                    RestaurantId = item.RestaurantId
                };
                await _comboRepo.AddAsync(newCombo);
                await _comboRepo.SaveChangesAsync();

                await _itemComboRepo.AddAsync(new ItemCombo
                {
                    ItemId = id,
                    ComboId = newCombo.Id,
                    ComboPrice = newCombo.ComboPrice,
                    ImageUrl = newCombo.ImageUrl
                });
            }
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

        // Remove related ItemAddOns
        var addOns = await _itemAddOnRepo.FindAsync(x => x.ItemId == id);
        foreach (var addOn in addOns)
            _itemAddOnRepo.Delete(addOn);

        // Remove related ItemCombos
        var combos = await _itemComboRepo.FindAsync(x => x.ItemId == id);
        foreach (var combo in combos)
            _itemComboRepo.Delete(combo);

        // Remove related ItemSizePrices
        var sizePrices = await _itemSizePriceRepo.FindAsync(x => x.ItemId == id);
        foreach (var sp in sizePrices)
            _itemSizePriceRepo.Delete(sp);

        // Remove the item itself
        _itemRepo.Delete(item);

        await _itemAddOnRepo.SaveChangesAsync();
        await _itemComboRepo.SaveChangesAsync();
        await _itemSizePriceRepo.SaveChangesAsync();
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

    public Task<ItemClassDto> GetItemByIdAsync(int id)
    {
        if (id <= 0)
            throw new ArgumentException("Invalid item ID.");
        return _itemRepo.GetByIdWithIncludesAsync(
            id,
            includeProperties: "ItemAddOns,ItemAddOns.AddOn,ItemCombos,ItemCombos.Combo,SizePricing"
        ).ContinueWith(task =>
        {
            var item = task.Result;
            if (item == null)
                throw new Exception("Item not found");
            return _mapper.Map<ItemClassDto>(item);
        });
    }
}
