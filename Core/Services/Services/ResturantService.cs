using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Domain.Contracts;
using Domain.Dtos;
using Domain.Dtos.ResturantDto;
using Domain.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Persistence.Data;
using Services.Abstractions.ICategoryService;

namespace Services.CategoryService
{
    public class ResturantService : IResturantService
    {
        private readonly IGenericRepository<Restaurant> _RestaurantRepository;
        private readonly IGenericRepository<Review> _ReviewRepository;
        private readonly IGenericRepository<Item> _ItemRepository;
        private readonly IGenericRepository<Category> _CategoryRepository;
        private readonly IdentityContext identityContext;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ResturantService(
            IGenericRepository<Restaurant> RestaurantRepository,
            IGenericRepository<Review> ReviewRepository,
            IGenericRepository<Item> ItemRepository,
            IGenericRepository<Category> CategoryRepository,
            IdentityContext identityContext,
            IHttpContextAccessor httpContextAccessor)
        {
            _RestaurantRepository = RestaurantRepository;
            _ReviewRepository = ReviewRepository;
            _ItemRepository = ItemRepository;
            _CategoryRepository = CategoryRepository;
            this.identityContext = identityContext;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<IEnumerable<Restaurant>> GetAllRestaurantsAsync()
        {
            // ✅ Get all restaurants from the repository
            var restaurants = await _RestaurantRepository.GetAllAsync();
            return restaurants;
        }

        public async Task AddResturantAsync(Restaurant restaurant)
        {
            if (restaurant == null)
                throw new ArgumentNullException(nameof(restaurant));

            // ✅ Get ChefId from current authenticated user's claims
            var currentUserId = _httpContextAccessor.HttpContext?.User
                ?.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(currentUserId))
                throw new UnauthorizedAccessException("User is not authenticated.");

            // ✅ Set ChefId automatically from current user
            restaurant.ChefId = currentUserId;

            // ✅ Validate ChefId exists in Identity DB
            var chef = await identityContext.Users.FindAsync(restaurant.ChefId);
            if (chef == null)
                throw new Exception("Chef with provided ID does not exist.");

            await _RestaurantRepository.AddAsync(restaurant);
            await _RestaurantRepository.SaveChangesAsync();
        }

        public async Task<Restaurant> UpdateRestaurantAsync(int restaurantId, Restaurant updatedRestaurant)
        {
            if (updatedRestaurant == null)
                throw new ArgumentNullException(nameof(updatedRestaurant));

            // Get current authenticated user
            var currentUserId = _httpContextAccessor.HttpContext?.User
                ?.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(currentUserId))
                throw new UnauthorizedAccessException("User is not authenticated.");

            // Get existing restaurant
            var existingRestaurant = await _RestaurantRepository.GetByIdAsync(restaurantId);
            if (existingRestaurant == null)
                throw new Exception("Restaurant not found.");

            // Verify the chef owns this restaurant
            if (existingRestaurant.ChefId != currentUserId)
                throw new UnauthorizedAccessException("You can only update your own restaurants.");

            // Update properties
            existingRestaurant.Name = updatedRestaurant.Name ?? existingRestaurant.Name;
            existingRestaurant.Description = updatedRestaurant.Description ?? existingRestaurant.Description;
            existingRestaurant.Location = updatedRestaurant.Location ?? existingRestaurant.Location;

            _RestaurantRepository.Update(existingRestaurant);
            await _RestaurantRepository.SaveChangesAsync();

            return existingRestaurant;
        }

        public async Task DeleteRestaurantAsync(int restaurantId)
        {
            // Get current authenticated user
            var currentUserId = _httpContextAccessor.HttpContext?.User
                ?.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(currentUserId))
                throw new UnauthorizedAccessException("User is not authenticated.");

            // Get existing restaurant
            var restaurant = await _RestaurantRepository.GetByIdAsync(restaurantId);
            if (restaurant == null)
                throw new Exception("Restaurant not found.");

            // Verify the chef owns this restaurant
            if (restaurant.ChefId != currentUserId)
                throw new UnauthorizedAccessException("You can only delete your own restaurants.");

            _RestaurantRepository.Delete(restaurant);
            await _RestaurantRepository.SaveChangesAsync();
        }

        public async Task<bool> CheckChefHasRestaurantAsync()
        {
            // Get current authenticated user
            var currentUserId = _httpContextAccessor.HttpContext?.User
                ?.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(currentUserId))
                throw new UnauthorizedAccessException("User is not authenticated.");

            // Check if chef has any restaurants
            var restaurants = await _RestaurantRepository.GetAllAsync();
            return restaurants.Any(r => r.ChefId == currentUserId);
        }

        public async Task<Restaurant> GetRestaurantByIdAsync(int restaurantId)
        {
            var restaurant = await _RestaurantRepository.GetByIdAsync(restaurantId);
            if (restaurant == null)
                throw new Exception("Restaurant not found.");

            return restaurant;
        }

        // Get the restaurant of the current chef
        public async Task<Restaurant> GetChefRestaurantAsync()
        {
            // Get current authenticated user
            var currentUserId = _httpContextAccessor.HttpContext?.User
                ?.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(currentUserId))
                throw new UnauthorizedAccessException("User is not authenticated.");

            // More efficient: filter at database level instead of in memory
            var restaurants = await _RestaurantRepository.GetAllAsync();
            var restaurant = restaurants.FirstOrDefault(r => r.ChefId == currentUserId);

            if (restaurant == null)
                throw new Exception("No restaurant found for the current chef.");

            return restaurant;
        }

        // Claculate rating
        public async Task<double> CalculateRestaurantRatingAsync(int restaurantId)
        {
            var restaurant = await _RestaurantRepository.GetByIdAsync(
                restaurantId,
                r => r.Include(x => x.Reviews)
            );
            if (restaurant == null || restaurant.Reviews == null || !restaurant.Reviews.Any())
                return 0;

            return restaurant.Reviews.Average(r => r.Rating);
        }

        public async Task AddReviewAsync(Review review)
        {
            if (review == null)
                throw new ArgumentNullException(nameof(review));

            // Save the review
            await _ReviewRepository.AddAsync(review);
            await _ReviewRepository.SaveChangesAsync();

            // Recalculate the restaurant's rating
            var restaurant = await _RestaurantRepository.GetByIdAsync(
                review.RestaurantId,
                r => r.Include(x => x.Reviews)
            );
            if (restaurant == null)
                throw new Exception("Restaurant not found.");

            restaurant.Rating = restaurant.Reviews.Any()
                ? restaurant.Reviews.Average(r => r.Rating)
                : 0;

            _RestaurantRepository.Update(restaurant);
            await _RestaurantRepository.SaveChangesAsync();
        }

        public async Task<CustomerRestaurantDto> GetCustomerRestaurantDetailsAsync(int restaurantId, int page = 1, int pageSize = 4)
        {
            // Validate parameters
            if (page <= 0) page = 1;
            if (pageSize <= 0) pageSize = 4;
            if (pageSize > 50) pageSize = 50;

            // Get restaurant basic info
            var restaurant = await _RestaurantRepository.GetByIdAsync(restaurantId);
            if (restaurant == null)
                throw new ArgumentException("Restaurant not found");

            // Get all items for this restaurant to extract categories
            var allItems = await _ItemRepository.GetAllQuerable()
                .Where(i => i.RestaurantId == restaurantId)
                .Include(i => i.Category)
                .AsNoTracking()
                .ToListAsync();

            // Get distinct categories
            var categories = allItems
                .Where(i => i.Category != null)
                .Select(i => i.Category)
                .DistinctBy(c => c.Id)
                .Select(c => new CategorySummaryDto
                {
                    Id = c.Id,
                    Name = c.Name
                })
                .OrderBy(c => c.Name)
                .ToList();

            // Get total items count
            var totalItems = allItems.Count;

            // Get paginated items
            var paginatedItems = allItems
                .OrderBy(i => i.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(i => new ItemSummaryDto
                {
                    Id = i.Id,
                    Name = i.Name,
                    Price = i.Price,
                    Image = i.ImageUrl,
                    CategoryName = i.Category?.Name ?? "Unknown",
                    CategoryId = i.Category?.Id ?? 0
                })
                .ToList();

            int totalPages = (int)Math.Ceiling((double)totalItems / pageSize);

            return new CustomerRestaurantDto
            {
                ResName = restaurant.Name,
                Rating = restaurant.Rating,
                ResImage = restaurant.ImageUrl,
                Location = restaurant.Location,
                Categories = categories,
                Items = paginatedItems,
                TotalPages = totalPages,
                CurrentPage = page,
                PageSize = pageSize,
                TotalItems = totalItems
            };
        }

        // Add this method to your existing ResturantService class
        public async Task<RestaurantCategoryItemsDto> GetRestaurantCategoryItemsAsync(int restaurantId, int categoryId, int page = 1, int pageSize = 4)
        {
            // Validate parameters
            if (page <= 0) page = 1;
            if (pageSize <= 0) pageSize = 4;
            if (pageSize > 50) pageSize = 50;

            // Verify restaurant exists
            var restaurant = await _RestaurantRepository.GetByIdAsync(restaurantId);
            if (restaurant == null)
                throw new ArgumentException("Restaurant not found");

            // Get all items for this restaurant and category
            var allItems = await _ItemRepository.GetAllQuerable()
                .Where(i => i.RestaurantId == restaurantId && i.CategoryId == categoryId)
                .AsNoTracking()
                .ToListAsync();

            // Get total items count
            var totalItems = allItems.Count;

            if (totalItems == 0)
            {
                return new RestaurantCategoryItemsDto
                {
                    Items = new List<CategoryItemSummaryDto>(),
                    Page = page,
                    PageSize = pageSize,
                    TotalPages = 0,
                    TotalItems = 0
                };
            }

            // Get paginated items
            var paginatedItems = allItems
                .OrderBy(i => i.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(i => new CategoryItemSummaryDto
                {
                    Id = i.Id,
                    Name = i.Name,
                    Price = i.Price,
                    Image = i.ImageUrl
                })
                .ToList();

            int totalPages = (int)Math.Ceiling((double)totalItems / pageSize);

            return new RestaurantCategoryItemsDto
            {
                Items = paginatedItems,
                Page = page,
                PageSize = pageSize,
                TotalPages = totalPages,
                TotalItems = totalItems
            };
        }
    }
}