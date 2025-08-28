using System.Linq;
using System.Net.NetworkInformation;
using System.Security.Claims;
using Domain.Contracts;
using Domain.Contracts.SieveProcessor;
using Domain.Dtos.ResturantDto;
using Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Persistence.Data;
using Services.Abstractions.ICategoryService;
using Services.Abstractions.IServices;
using Sieve.Services;
using static System.Net.Mime.MediaTypeNames;
using Domain.Dtos;


[ApiController]
[Route("api/[controller]")]
public class RestaurantsController : ControllerBase
{
    private readonly IResturantService _restaurantService;
    private readonly IGenericService<Restaurant> _genericService;
    public RestaurantsController(
        IResturantService restaurantService,
        IGenericService<Restaurant> genericService
        )
    {
        _restaurantService = restaurantService;
        _genericService = genericService;
    }

    [HttpGet]
    [Authorize(Roles = "Admin,Customer")]
    public async Task<IActionResult> GetAllRestaurants([FromQuery] CustomSieveModel sieveModel)
    {
        try
        {
            // 1) filtered+sorted, NO pagination -> correct totalCount
            var filtered = _genericService.GetAllSieveAsync(sieveModel, applyPagination: false);
            var totalCount = await filtered.CountAsync();

            // 2) filtered+sorted, WITH pagination -> page items
            var paged = _genericService.GetAllSieveAsync(sieveModel, applyPagination: true);
            var restaurants = await paged.ToListAsync();

            var page = sieveModel.Page ?? 1;
            var pageSize = sieveModel.PageSize ?? 20;

            return Ok(new
            {
                restaurants,
                totalCount,
                page,
                pageSize
            });
        }
        catch
        {
            return StatusCode(500, new { error = "An error occurred while fetching restaurants." });
        }
    }

    // Authorized Chef can create a restaurant
    [HttpPost]
    [Authorize(Roles = "Chef")]
    public async Task<IActionResult> CreateRestaurant([FromBody] RestaurantInputDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        // ✅ Get ChefId from current authenticated user's claims
        var chefId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(chefId))
            return Unauthorized("User is not authenticated.");

        var restaurant = new Restaurant
        {
            Name = dto.Name,
            Description = dto.Description,
            Location = dto.Location,
            ImageUrl = dto.ImageUrl,
            OpeningHours = dto.OpeningHours,
            IsOpen = dto.IsOpen,
            ChefId = chefId
        };

        try
        {
            await _restaurantService.AddResturantAsync(restaurant);
            var responseDto = new RestaurantResponseDto
            {
                Id = restaurant.Id,
                Name = restaurant.Name,
                Description = restaurant.Description,
                Location = restaurant.Location,
                ImageUrl = restaurant.ImageUrl,
                Rating = restaurant.Rating,
                ChefId = restaurant.ChefId,
                OpeningHours = dto.OpeningHours,
                IsOpen = dto.IsOpen
            };
            return Ok(responseDto);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(ex.Message);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    // Authorized Chef, and Admin can view the restaurant by ID
    [HttpPut("{restaurantId:int}")]
    [Authorize(Roles = "Chef,Admin")]
    public async Task<IActionResult> UpdateRestaurant(int restaurantId, [FromBody] RestaurantInputDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var updatedRestaurant = new Restaurant
        {
            Name = dto.Name,
            Description = dto.Description,
            Location = dto.Location,
            ImageUrl = dto.ImageUrl
        };

        try
        {
            var restaurant = await _restaurantService.UpdateRestaurantAsync(restaurantId, updatedRestaurant);
            var responseDto = new RestaurantResponseDto
            {
                Id = restaurant.Id,
                Name = restaurant.Name,
                Description = restaurant.Description,
                Location = restaurant.Location,
                ImageUrl = restaurant.ImageUrl,
                Rating = restaurant.Rating,
                ChefId = restaurant.ChefId,
                OpeningHours = dto.OpeningHours,
                IsOpen = dto.IsOpen
            };
            return Ok(responseDto);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(ex.Message);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    // Authorized Chef can delete a restaurant
    [HttpDelete("{restaurantId:int}")]
    [Authorize (Roles = "Chef")]
    public async Task<IActionResult> DeleteRestaurant(int restaurantId)
    {
        try
        {
            await _restaurantService.DeleteRestaurantAsync(restaurantId);
            return NoContent();
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(ex.Message);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    // Authorized Chef can check if they have a restaurant
    [HttpGet("check")]
    [Authorize (Roles = "Chef")]
    public async Task<IActionResult> CheckChefHasRestaurant()
    {
        try
        {
            var hasRestaurant = await _restaurantService.CheckChefHasRestaurantAsync();
            return Ok(new { hasRestaurant });
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(ex.Message);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    // GET: api/Restaurants/chef-restaurant
    [HttpGet("chef-restaurant")]
    [Authorize(Roles = "Chef")]
    public async Task<IActionResult> GetChefRestaurant()
    {
        try
        {
            var restaurant = await _restaurantService.GetChefRestaurantAsync();
            if (restaurant == null)
                return NotFound(new { message = "No restaurant found for the current chef." });

            var responseDto = new RestaurantResponseDto
            {
                Id = restaurant.Id,
                Name = restaurant.Name,
                Description = restaurant.Description,
                Location = restaurant.Location,
                ImageUrl = restaurant.ImageUrl,
                OpeningHours = restaurant.OpeningHours,
                IsOpen = restaurant.IsOpen,
                Rating = restaurant.Rating,
                ChefId = restaurant.ChefId
            };
            return Ok(responseDto);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(ex.Message);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }

    }

    [HttpGet("customer-restaurant/{id}")]
    public async Task<IActionResult> GetCustomerRestaurant(int id, [FromQuery] int page = 1, [FromQuery] int pageSize = 4)
    {
        try
        {
            var result = await _restaurantService.GetCustomerRestaurantDetailsAsync(id, page, pageSize);
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "An unexpected error occurred." });
        }
    }

    [HttpGet("restCatItems/{id}")]
    public async Task<IActionResult> GetRestCatItems(int id, [FromQuery] int catId, [FromQuery] int page = 1, [FromQuery] int pageSize = 4)
    {
        try
        {
            var result = await _restaurantService.GetRestaurantCategoryItemsAsync(id, catId, page, pageSize);
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "An unexpected error occurred." });
        }
    }

}



