using Domain.Dtos.AddOnDto;
using Microsoft.AspNetCore.Mvc;
using Services.Abstractions.IServices;

[Route("api/[controller]")]
[ApiController]
public class AddOnController : ControllerBase
{
    private readonly IAddOnService _addOnService;

    public AddOnController(IAddOnService addOnService)
    {
        _addOnService = addOnService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var result = await _addOnService.GetAllAsync();
        return Ok(result); 
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _addOnService.GetByIdAsync(id);
        return Ok(result); 
    }

    [HttpPost]
    public async Task<IActionResult> Create(AddOnDto dto)
    {
        await _addOnService.AddAsync(dto);
        return Ok("Created successfully"); 
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, AddOnDto dto)
    {
        await _addOnService.UpdateAsync(id, dto);
        return Ok("Updated successfully");
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        await _addOnService.DeleteAsync(id);
        return Ok("Deleted successfully");
    }

}
