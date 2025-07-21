using Domain.Dtos.ResturantDto;
using Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Services.Abstractions.ICategoryService;
using System.Security.Claims;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Chef")] // Ensure only chefs can create restaurants
public class RestaurantsController : ControllerBase
{
    private readonly IResturantService _restaurantService;

    public RestaurantsController(IResturantService restaurantService)
    {
        _restaurantService = restaurantService;
    }

    [HttpPost]
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
            //Rating = dto.Rating,
            ChefId = chefId // ✅ Set automatically from claims, not from DTO
        };

        try
        {
            await _restaurantService.AddResturantAsync(restaurant);
            return Ok(new { message = "Restaurant created successfully", restaurantId = restaurant.Id });
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
}