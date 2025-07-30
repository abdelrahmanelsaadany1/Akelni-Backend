using Domain.Contracts;
using Domain.Dtos.AddOnDto;
using Domain.Entities;
using Services.Abstractions.IServices;

public class AddOnService : IAddOnService
{
    private readonly IGenericRepository<AddOn> _addOnRepository;

    public AddOnService(IGenericRepository<AddOn> addOnRepository)
    {
        _addOnRepository = addOnRepository;
    }

    public async Task<IEnumerable<ReturnDto>> GetAllAsync()
    {
        var addOns = await _addOnRepository.GetAllAsync();
        return addOns.Select(a => new ReturnDto
        {
            Id = a.Id,
            Name = a.Name,
            AdditionalPrice = a.AdditionalPrice
        });
    }

    public async Task<ReturnDto> GetByIdAsync(int id)
    {
        var addOn = await _addOnRepository.GetByIdAsync(id);
        if (addOn == null)
            throw new Exception("AddOn not found");

        return new ReturnDto
        {
            Id = addOn.Id,
            Name = addOn.Name,
            AdditionalPrice = addOn.AdditionalPrice
        };
    }

    public async Task<ReturnDto> AddAsync(AddOnDto dto)
    {
        var addOn = new AddOn
        {
            Name = dto.Name,
            AdditionalPrice = dto.AdditionalPrice
        };

        await _addOnRepository.AddAsync(addOn);
        await _addOnRepository.SaveChangesAsync();

        return new ReturnDto
        {
            Id = addOn.Id,
            Name = addOn.Name,
            AdditionalPrice = addOn.AdditionalPrice
        };
    }

    public async Task<ReturnDto> UpdateAsync(int id, AddOnDto dto)
    {
        var addOn = await _addOnRepository.GetByIdAsync(id);
        if (addOn == null)
            throw new Exception("AddOn not found");

        addOn.Name = dto.Name;
        addOn.AdditionalPrice = dto.AdditionalPrice;

        _addOnRepository.Update(addOn);
        await _addOnRepository.SaveChangesAsync();

        return new ReturnDto
        {
            Id = addOn.Id,
            Name = addOn.Name,
            AdditionalPrice = addOn.AdditionalPrice
        };
    }

    public async Task DeleteAsync(int id)
    {
        var addOn = await _addOnRepository.GetByIdAsync(id);
        if (addOn == null)
            throw new Exception("AddOn not found");

        _addOnRepository.Delete(addOn);
        await _addOnRepository.SaveChangesAsync();
    }
}
