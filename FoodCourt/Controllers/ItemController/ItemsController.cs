
using Domain.Contracts.SieveProcessor;
using Domain.Dtos.ItemDto;
using Domain.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Services.Abstractions.IServices;
using Sieve.Models;

namespace FoodCourt.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ItemsController : ControllerBase
    {
        private readonly IItemService _itemService;
        private readonly IGenericService<Item> _genericService;
        public ItemsController(
            IItemService itemService,
            IGenericService<Item> genericService
            )
        {
            _itemService = itemService;
            _genericService = genericService;
        }

        [HttpPost("Add")]
        public async Task<IActionResult> Add([FromBody] ItemCreateUpdateDto dto)
        {
            try
            {
                await _itemService.AddItemAsync(dto);
                return Ok("Item added successfully!");
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    message = ex.Message,
                    inner = ex.InnerException?.Message,
                    stack = ex.StackTrace
                });
            }
        }

        // api/Items/GetAll?filters=restaurantId==22,CategoryId==59,price>20

        [HttpGet("GetAll")]
        public async Task<IActionResult> GetAll([FromQuery] CustomSieveModel sieveModel)
        {
            //var items = await _itemService.GetAllItemsAsync();
            //var categories = await _categoryService.GetAllCategoriesAsync();
            //var categories = _context.Categories.AsQueryable();
            //var result = _sieveProcessor.Apply(sieveModel, categories,applyPagination:true);
            var result = _genericService.GetAllSieveAsync(sieveModel);

            return Ok(new
            {
                categories = await result.ToListAsync(),
                totalCount = await result.CountAsync()
            });
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
        public async Task<IActionResult> Update(int id, [FromBody] ItemCreateUpdateDto dto)
        {
            try
            {
                await _itemService.UpdateItemAsync(id, dto);
                return Ok("Item updated successfully!");
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    message = ex.Message,
                    inner = ex.InnerException?.Message,
                    stack = ex.StackTrace
                });
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
                return BadRequest(new
                {
                    message = ex.Message,
                    inner = ex.InnerException?.Message,
                    stack = ex.StackTrace
                });
            }
        }
    }
}
