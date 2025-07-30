using Domain.Dtos.ComboDto;
using Microsoft.AspNetCore.Mvc;
using Services.Abstractions.IServices;

[Route("api/[controller]")]
[ApiController]
public class ComboController : ControllerBase
{
    private readonly IComboService _comboService;

    public ComboController(IComboService comboService)
    {
        _comboService = comboService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var combos = await _comboService.GetAllAsync();
        return Ok(combos);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var combo = await _comboService.GetByIdAsync(id);
        if (combo == null) return NotFound();

        return Ok(combo);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] ComboDto dto)
    {
        await _comboService.AddAsync(dto);
        return Ok("Created successfully");
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] ComboDto dto)
    {
        await _comboService.UpdateAsync(id, dto);
        return Ok("Updated successfully");
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        await _comboService.DeleteAsync(id);
        return Ok("Deleted successfully");
    }
}
