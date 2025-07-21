using Domain.Contracts;
using Domain.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using Persistence.Data;
using Services.Abstractions.ICategoryService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Services.CategoryService
{
    public class ResturantService : IResturantService
    {
        private readonly IGenericRepository<Restaurant> _RestaurantRepository;
        private readonly IdentityContext identityContext;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ResturantService(
            IGenericRepository<Restaurant> RestaurantRepository,
            IdentityContext identityContext,
            IHttpContextAccessor httpContextAccessor)
        {
            _RestaurantRepository = RestaurantRepository;
            this.identityContext = identityContext;
            _httpContextAccessor = httpContextAccessor;
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
    }
}