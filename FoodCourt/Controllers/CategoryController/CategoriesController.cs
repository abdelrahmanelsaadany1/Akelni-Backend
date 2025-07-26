using Domain.Dtos.CategoryDto;
using Domain.Entities;
using Microsoft.AspNetCore.Mvc;
using Services.Abstractions.ICategoryService;
using System;
using System.Threading.Tasks;

namespace FoodCourt.Controllers.CategoryController
{
    [ApiController]
    [Route("api/[controller]")]
    public class CategoriesController : ControllerBase
    {
        private readonly ICategoryService _categoryService;

        public CategoriesController(ICategoryService categoryService)
        {
            _categoryService = categoryService;
        }

        [HttpPost("Add")]
        public async Task<IActionResult> AddCategory([FromBody] CategoryCreateDto dto)
        {
            var category = new Category { Name = dto.Name };

            try
            {
                await _categoryService.AddCategoryAsync(category);
                return Ok("Category added successfully!");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("GetAll")]
        public async Task<IActionResult> GetAllCategories()
        {
            var categories = await _categoryService.GetAllCategoriesAsync();
            return Ok(categories);
        }

        [HttpGet("GetById/{id}")]
        public async Task<IActionResult> GetCategoryById(int id)
        {
            try
            {
                var category = await _categoryService.GetCategoryByIdAsync(id);
                return Ok(category);
            }
            catch (Exception ex)
            {
                return NotFound(ex.Message);
            }
        }

        [HttpGet("GetByName/{name}")]
        public async Task<IActionResult> GetCategoryByName(string name)
        {
            try
            {
                var category = await _categoryService.GetCategoryByNameAsync(name);
                return Ok(category);
            }
            catch (Exception ex)
            {
                return NotFound(ex.Message);
            }
        }

        [HttpPut("Update/{id}")]
        public async Task<IActionResult> UpdateCategory(int id, [FromBody] CategoryCreateDto dto)
        {
            try
            {
                await _categoryService.UpdateCategoryAsync(id, dto.Name);
                return Ok("Category updated successfully!");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete("Delete/{id}")]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            try
            {
                await _categoryService.DeleteCategoryAsync(id);
                return Ok("Category deleted successfully!");
            }
            catch (Exception ex)
            {
                return NotFound(ex.Message);
            }
        }

        //[HttpPost("BulkAdd")]
        //public async Task<IActionResult> BulkAddCategories([FromBody] string[] categoryNames)
        //{
        //    try
        //    {
        //        foreach (var name in categoryNames)
        //        {
        //            var category = new Category { Name = name };
        //            await _categoryService.AddCategoryAsync(category);
        //        }
        //        return Ok($"Successfully added {categoryNames.Length} categories!");
        //    }
        //    catch (Exception ex)
        //    {
        //        return BadRequest(ex.Message);
        //    }
        //}
    }
}
