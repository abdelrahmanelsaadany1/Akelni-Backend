using Domain.Contracts;
using Domain.Dtos.ComboDto;
using Domain.Entities;
using Services.Abstractions.IServices;

public class ComboService : IComboService
{
    private readonly IGenericRepository<Combo> _comboRepository;

    public ComboService(IGenericRepository<Combo> comboRepository)
    {
        _comboRepository = comboRepository;
    }

    public async Task<IEnumerable<ReturnDto>> GetAllAsync()
    {
        var combos = await _comboRepository.GetAllAsync();
        return combos.Select(c => new ReturnDto
        {
            Id = c.Id,
            Name = c.Name,
            ComboPrice = c.ComboPrice
        });
    }

    public async Task<ReturnDto?> GetByIdAsync(int id)
    {
        var combo = await _comboRepository.GetByIdAsync(id);
        if (combo == null) return null;

        return new ReturnDto
        {
            Id = combo.Id,
            Name = combo.Name,
            ComboPrice = combo.ComboPrice
        };
    }

    public async Task AddAsync(ComboDto dto)
    {
        var combo = new Combo
        {
            Name = dto.Name,
            ComboPrice = dto.ComboPrice
        };

        await _comboRepository.AddAsync(combo);
        await _comboRepository.SaveChangesAsync();
    }

    public async Task UpdateAsync(int id, ComboDto dto)
    {
        var combo = await _comboRepository.GetByIdAsync(id);
        if (combo == null)
            throw new Exception("Combo not found");

        combo.Name = dto.Name;
        combo.ComboPrice = dto.ComboPrice;

        _comboRepository.Update(combo);
        await _comboRepository.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var combo = await _comboRepository.GetByIdAsync(id);
        if (combo == null)
            throw new Exception("Combo not found");

        _comboRepository.Delete(combo);
        await _comboRepository.SaveChangesAsync();
    }
}
