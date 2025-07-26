using AutoMapper;
using Domain.Contracts;
using Domain.Contracts.SieveProcessor;
using Domain.Dtos.CategoryDto;
using Domain.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Persistence.Data;
using Services.Abstractions.ICategoryService;
using Sieve.Services;
using System;
using System.Threading.Tasks;

namespace FoodCourt.Controllers.CategoryController
{
    [ApiController]
    [Route("api/[controller]")]
    public class CategoriesController : ControllerBase
    {
        private readonly ICategoryService _categoryService;
        private readonly IMapper _mapper;
        private readonly FoodCourtDbContext _context;
        private readonly SieveProcessor _sieveProcessor;
        public CategoriesController(
            ICategoryService categoryService,
            IMapper mapper,
            FoodCourtDbContext context,
            SieveProcessor sieveProcessor
            )
        {
            _categoryService = categoryService;
            _mapper = mapper;
            _context = context;
            _sieveProcessor = sieveProcessor;
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

        //GET /api/foods?filters=name==Pizza,price>=30&sorts=-rating&page=1&pageSize=10
        [HttpGet("GetAll")]
        public async Task<IActionResult> GetAllCategories([FromQuery] CustomSieveModel sieveModel)
        {
            //var categories = await _categoryService.GetAllCategoriesAsync();
            var categories = _context.Categories.AsQueryable();
            var result = _sieveProcessor.Apply(sieveModel, categories,applyPagination:true);
          
            return Ok(new
            {
                categories = await result.ToListAsync(),
                totalCount = await result.CountAsync()
            });
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
                var mappedCategory = _mapper.Map<CategoryCreateDto>(category);
                return Ok(mappedCategory);
                
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
