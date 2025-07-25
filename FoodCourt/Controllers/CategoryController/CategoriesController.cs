using AutoMapper;
using Domain.Contracts.SieveProcessor;
using Domain.Dtos.CategoryDto;
using Domain.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Services.Abstractions.ICategoryService;
using Services.Abstractions.IServices;


namespace FoodCourt.Controllers.CategoryController
{
    [ApiController]
    [Route("api/[controller]")]
    public class CategoriesController : ControllerBase
    {
        private readonly ICategoryService _categoryService;
        private readonly IMapper _mapper;
        private readonly IGenericService<Category> _genericService;
        public CategoriesController(
            ICategoryService categoryService,
            IGenericService<Category> genericService,
            IMapper mapper
            )
        {
            _categoryService = categoryService;
            _mapper = mapper;
            _genericService = genericService;
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
    }
}
