using Domain.Dtos.ItemDto;
using Microsoft.AspNetCore.Mvc;
using Services.Abstractions.IServices;

namespace FoodCourt.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ItemsController : ControllerBase
    {
        private readonly IItemService _itemService;

        public ItemsController(IItemService itemService)
        {
            _itemService = itemService;
        }

        [HttpPost("Add")]
        public async Task<IActionResult> Add([FromBody] ItemDto dto)
        {
            try
            {
                await _itemService.AddItemAsync(dto);
                return Ok("Item added successfully!");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("GetAll")]
        public async Task<IActionResult> GetAll()
        {
            var items = await _itemService.GetAllItemsAsync();
            return Ok(items);
        }

        [HttpGet("GetById/{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var item = await _itemService.GetItemByIdAsync(id);
                return Ok(item);
            }
            catch (Exception ex)
            {
                return NotFound(ex.Message);
            }
        }

        [HttpPut("Update/{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] ItemDto dto)
        {
            try
            {
                await _itemService.UpdateItemAsync(id, dto);
                return Ok("Item updated successfully!");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete("Delete/{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                await _itemService.DeleteItemAsync(id);
                return Ok("Item deleted successfully!");
            }
            catch (Exception ex)
            {
                return NotFound(ex.Message);
            }
        }
    }
}
