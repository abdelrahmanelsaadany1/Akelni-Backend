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
using System.Linq;
using System.Security.Claims;

[ApiController]
[Route("api/[controller]")]
public class RestaurantsController : ControllerBase
{
    private readonly IResturantService _restaurantService;
    private readonly IGenericService<Restaurant> _genericService;

    readonly FoodCourtDbContext _dbContext;
    public RestaurantsController(
        IResturantService restaurantService,
        IGenericService<Restaurant> genericService,

        FoodCourtDbContext dbContext
        )
    {
        _restaurantService = restaurantService;
        _genericService = genericService;

        _dbContext = dbContext;


    }

    // Authorized Admin and Customer can view all restaurants
    [HttpGet]
    [Authorize(Roles = "Admin,Customer")]
    public async Task<IActionResult> GetAllRestaurants([FromQuery] CustomSieveModel sieveModel)
    {
        //var restaurants = await _restaurantService.GetAllRestaurantsAsync();
        var result = _genericService.GetAllSieveAsync(sieveModel);
        return Ok(new
        {
            categories = await result.ToListAsync(),
            totalCount = await result.CountAsync()
        });
    }
    //[Authorize (Roles = "Admin,Chef")]
    //public async Task<IActionResult> GetAllRestaurants()
    //{
    //    //var restaurants = await _restaurantService.GetAllRestaurantsAsync();
    //    var result = _genericService.GetAllSieveAsync(sieveModel);
    //    return Ok(new
    //    {
    //        categories = await result.ToListAsync(),
    //        totalCount = await result.CountAsync()
    //    });
    //}

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
    public async Task<IActionResult> GetCustomerRestaurant(int id, [FromQuery] int page = 1 , [FromQuery] int pageSize = 4)
    {
        try
        {
            //restaurant data include items and category items query on DB
            var restaurantData = await _dbContext.Restaurants
                .Where(res => res.Id == id)
                .Select(r => new {
                     resName = r.Name,
                      rating = r.Rating,
                      resImage = r.ImageUrl,
                     location = r.Location,
                      items = r.Items.Select(item => new {
                               id = item.Id,
                               name = item.Name,
                               price = item.Price,
                               image = item.ImageUrl,
                               categoryName = item.Category.Name,
                               categoyId = item.Category.Id
                           }
                      ).ToList()
                
                             }).FirstOrDefaultAsync();

            //select distinct categories data from previous result
            var distinctCategories = restaurantData.items
              .Select(item => new { item.categoyId, item.categoryName })
              .Distinct()
              .ToList();

         
            var itemsPagination = _genericService.CustomPagination(restaurantData.items, page, pageSize);
            int totalPages = (int)Math.Ceiling((double)restaurantData.items.Count / pageSize);
            //now display resutls as you like
            var result = new
            {
                resName = restaurantData.resName,
                rating = restaurantData.rating,
                resImage = restaurantData.resImage,
                location = restaurantData.location,
                items = itemsPagination,
                categories = distinctCategories,
                totalPages = totalPages
            };


            return Ok(result);
        }
        catch(Exception e)
        {
            return BadRequest($"{e.Message}");
        }
    }
}