using System.Data;
using System.Security.Claims;
using System.Text.Json;
using Domain.Dtos.Auth;
using Domain.Entities.Identity;
using Google.Apis.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Services.Abstractions.IServices;
using Services.Auth;

namespace FoodCourt.Controllers.Account
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly JwtService _jwtService;
        private readonly EmailService _emailService;
        //private readonly ILocationService _locationService;
        private readonly IConfiguration _configuration;
        //private readonly FacebookAuthService _facebookAuthService;

        public AuthController(UserManager<User> userManager,
                              SignInManager<User> signInManager,
                              JwtService jwtService,
                              EmailService emailService,
                              //ILocationService locationService,
                              //FacebookAuthService facebookAuthService,
                              IConfiguration configuration)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _jwtService = jwtService;
            _emailService = emailService;
            //_facebookAuthService = facebookAuthService;
            _configuration = configuration;
            //_locationService = locationService;
        }


        // Endpoints to update/delete user information (Profile) either Admin, Chef , or Customer

        [Authorize(Roles = "Admin, Chef, Customer")]
        [HttpPut("profile")]
        public async Task<IActionResult> UpdateProfile([FromBody] ProfileDto prf)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized("User not found");

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return NotFound("User not found");

            user.Email = prf.Email;
            user.ImageUrl = prf.ImageUrl;
            user.PhoneNumber = prf.PhoneNumber;
            user.FirstName = prf.FirstName;
            user.LastName = prf.LastName;

            user.Addresses.Clear();
            foreach (var addrDto in prf.Addresses)
            {
                user.Addresses.Add(new Address
                {
                    Description = addrDto.Description,
                    Street = addrDto.Street,
                    City = addrDto.City,
                    Country = addrDto.Country,
                    Zone = addrDto.Zone,
                    UserId = user.Id
                });
            }

            // Handle password change if requested
            if (!string.IsNullOrEmpty(prf.OldPassword) && !string.IsNullOrEmpty(prf.NewPassword))
            {
                var result = await _userManager.ChangePasswordAsync(user, prf.OldPassword, prf.NewPassword);
                if (!result.Succeeded)
                {
                    var errors = result.Errors.Select(e => e.Description);
                    return BadRequest(new { message = "Password change failed", errors });
                }

            }

            var updateResult = await _userManager.UpdateAsync(user);
            if (!updateResult.Succeeded)
            {
                var errors = updateResult.Errors.Select(e => e.Description);
                return BadRequest(new { message = "Update failed", errors });
            }


            return Ok("Profile updated successfully");
        }

        [Authorize(Roles = "Admin, Chef, Customer")]
        [HttpGet("profile")]
        public async Task<IActionResult> GetProfile()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized("User not found");

            var user = await _userManager.Users
                .Include(u => u.Addresses)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
                return NotFound("User not found");

            var profile = new ProfileDto
            {
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                ImageUrl = user.ImageUrl,
                PhoneNumber = user.PhoneNumber,
                Addresses = user.Addresses.Select(a => new AddressDto
                {
                    Description = a.Description,
                    Street = a.Street,
                    City = a.City,
                    Country = a.Country,
                    Zone = a.Zone
                }).ToList()
            };

            return Ok(profile);
        }

        //// a) GET /locations/governorates
        ////→ returns all governorates

        //[HttpGet("locations/governorates")]
        //public async Task<IActionResult> GetGovernorates()
        //{
        //    var governorates = await _locationService.GetAllGovernorates();
        //    return Ok(governorates);
        //}

        ////b) GET /locations/cities? governorateId = { id }
        ////→ returns all cities within a given governorate
        //[HttpGet("locations/cities")]
        //public async Task<IActionResult> GetCities([FromQuery] int governorateId)
        //{
        //    if (governorateId <= 0)
        //        return BadRequest("Invalid governorate ID");
        //    var cities = await _locationService.GetCitiesByGovernorate(governorateId);
        //    return Ok(cities);
        //}

        ////c) GET /locations/zones? cityId = { id }
        ////→ returns all zones or qisms within the selected city
        //[HttpGet("locations/zones")]
        //public async Task<IActionResult> GetZones([FromQuery] int cityId)
        //{
        //    if (cityId <= 0)
        //        return BadRequest("Invalid city ID");
        //    var zones = await _locationService.GetZonesByCity(cityId);
        //    return Ok(zones);
        //}

        //[HttpGet("locations/governorates")]
        //public async Task<IActionResult> GetGovernorates() => Ok(await _locationService.GetAllGovernoratesAsync());

        //[HttpGet("locations/cities")]
        //public async Task<IActionResult> GetCities(int governorateId) => Ok(await _locationService.GetCitiesByGovernorateAsync(governorateId));

        //[HttpGet("locations/zones")]
        //public async Task<IActionResult> GetZones(int cityId) => Ok(await _locationService.GetZonesByCityAsync(cityId));

        //../d./a.d.a/.d/a./d.a/d./a.d/a.d/a.
        //[HttpGet("locations/governorates")]
        //public async Task<IActionResult> GetGovernorates() =>
        //    Ok(await _locationService.GetAllGovernoratesAsync());

        //[HttpGet("locations/cities")]
        //public async Task<IActionResult> GetCities(int governorateId) =>
        //    Ok(await _locationService.GetCitiesByGovernorateAsync(governorateId));

        //[HttpGet("locations/zones")]
        //public async Task<IActionResult> GetZones(int cityId) =>
        //    Ok(await _locationService.GetZonesByCityAsync(cityId));



        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterDto dto)
        {
            try
            {
                // Add model validation
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                // Add logging
                Console.WriteLine($"Registration attempt for: {dto.Email}");

                var user = new User { Email = dto.Email, UserName = dto.Email, DisplayName = dto.Role };

                var result = await _userManager.CreateAsync(user, dto.Password);
                if (!result.Succeeded)
                {
                    Console.WriteLine($"User creation failed: {string.Join(", ", result.Errors.Select(e => e.Description))}");
                    return BadRequest(new { errors = result.Errors.Select(e => e.Description) });
                }

                await _userManager.AddToRoleAsync(user, dto.Role);
                var roles = await _userManager.GetRolesAsync(user);
                var token = _jwtService.GenerateToken(user, roles);

                return Ok(new { token, message = "Registration successful" });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Registration error: {ex.Message}");
                return StatusCode(500, new { message = "Internal server error", details = ex.Message });
            }
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDto dto)
        {

            var user = await _userManager.FindByEmailAsync(dto.Email);
            if (user == null)
                return Unauthorized("Invalid credentials");

            var result = await _signInManager.CheckPasswordSignInAsync(user, dto.Password, false);
            if (!result.Succeeded)
                return Unauthorized("Invalid credentials");

            var roles = await _userManager.GetRolesAsync(user);
            // Remember me
            var rememberMe = !string.IsNullOrEmpty(dto.rememberMe) &&
                (dto.rememberMe.ToLower() == "true" || dto.rememberMe == "1");

            var token = _jwtService.GenerateToken(user, roles, rememberMe);

            return Ok(new { token });
        }

        [HttpPost("google")]
        public async Task<IActionResult> GoogleLogin([FromBody] ExternalAuthDto dto)
        {
            try
            {
                if (string.IsNullOrEmpty(dto?.IdToken))
                {
                    return BadRequest(new { message = "Token is required" });
                }

                var googleClientId = _configuration["Authentication:Google:ClientId"];
                if (string.IsNullOrEmpty(googleClientId))
                {
                    return StatusCode(500, new { message = "Google ClientId is not configured." });
                }

                var payload = await GoogleJsonWebSignature.ValidateAsync(dto.IdToken, new GoogleJsonWebSignature.ValidationSettings
                {
                    Audience = new[] { googleClientId }
                });

                var user = await _userManager.FindByEmailAsync(payload.Email);

                if (user == null)
                {
                    user = new User
                    {
                        Email = payload.Email,
                        UserName = payload.Email,
                        DisplayName = dto.Role
                    };

                    var result = await _userManager.CreateAsync(user);
                    if (!result.Succeeded)
                    {
                        return BadRequest(new { message = "Failed to create user", errors = result.Errors });
                    }

                    // ✅ Add this block to populate AspNetUserLogins
                    var loginInfo = new UserLoginInfo("Google", payload.Subject, "Google");
                    var addLoginResult = await _userManager.AddLoginAsync(user, loginInfo);
                    if (!addLoginResult.Succeeded)
                    {
                        return BadRequest(new { message = "Failed to link external login", errors = addLoginResult.Errors });
                    }

                    var roleToAssign = string.IsNullOrWhiteSpace(dto.Role) ? "Customer" : dto.Role;
                    await _userManager.AddToRoleAsync(user, roleToAssign);
                }


                // Always fetch roles for the user
                var roles = await _userManager.GetRolesAsync(user);
                var token = _jwtService.GenerateToken(user, roles);

                return Ok(new
                {
                    message = "Login successful",
                    token = token,
                    user = new
                    {
                        email = user.Email,
                    }
                });
            }
            catch (InvalidJwtException ex)
            {
                return Unauthorized(new { message = "Invalid Google token", details = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Internal server error", details = ex.ToString() });
            }
        }

        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDto dto)
        {
            var user = await _userManager.FindByEmailAsync(dto.Email);
            if (user == null) return Ok();

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            if (user.Email != null)
            {
                await _emailService.SendResetLink(user.Email, token);
            }
            return Ok();
        }

        //// Facebook

        //[HttpPost("facebook")]
        //public async Task<IActionResult> FacebookLogin([FromBody] ExternalAuthDto dto)
        //{
        //    try
        //    {
        //        if (string.IsNullOrWhiteSpace(dto?.IdToken))
        //            return BadRequest(new { message = "Access token is required." });

        //        var (email, facebookId) = await _facebookAuthService.VerifyFacebookTokenAsync(dto.IdToken);

        //        if (string.IsNullOrEmpty(facebookId))
        //            return Unauthorized(new { message = "Could not retrieve Facebook user info." });

        //        // Optional: fallback username if email missing
        //        var username = email ?? $"fb_{facebookId}@facebook.com";

        //        // Try to find user by login first
        //        var user = await _userManager.FindByLoginAsync("Facebook", facebookId);

        //        if (user == null)
        //        {
        //            // Try finding by email if available
        //            user = email != null ? await _userManager.FindByEmailAsync(email) : null;

        //            if (user == null)
        //            {
        //                user = new User
        //                {
        //                    UserName = username,
        //                    Email = email,
        //                    DisplayName = dto.Role ?? "Facebook User",
        //                };

        //                var result = await _userManager.CreateAsync(user);
        //                if (!result.Succeeded)
        //                    return BadRequest(new { message = "Failed to create user", errors = result.Errors });

        //                // Link external login
        //                var loginInfo = new UserLoginInfo("Facebook", facebookId, "Facebook");
        //                var loginResult = await _userManager.AddLoginAsync(user, loginInfo);
        //                if (!loginResult.Succeeded)
        //                    return BadRequest(new { message = "Failed to link Facebook login", errors = loginResult.Errors });

        //                // Assign default role
        //                var role = string.IsNullOrWhiteSpace(dto.Role) ? "Customer" : dto.Role;
        //                await _userManager.AddToRoleAsync(user, role);
        //            }
        //            else
        //            {
        //                // User found by email — link Facebook login
        //                var loginInfo = new UserLoginInfo("Facebook", facebookId, "Facebook");
        //                await _userManager.AddLoginAsync(user, loginInfo);
        //            }
        //        }

        //        var roles = await _userManager.GetRolesAsync(user);
        //        var jwt = _jwtService.GenerateToken(user, roles);

        //        return Ok(new
        //        {
        //            message = "Facebook login successful",
        //            token = jwt,
        //            user = new
        //            {
        //                email = user.Email,
        //                name = user.DisplayName
        //            }
        //        });
        //    }
        //    catch (Exception ex)
        //    {
        //        return StatusCode(500, new { message = "Internal error", error = ex.Message });
        //    }
        //}



        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto dto)
        {
            var user = await _userManager.FindByEmailAsync(dto.Email);
            if (user == null) return BadRequest("Invalid request");

            var result = await _userManager.ResetPasswordAsync(user, dto.Token, dto.NewPassword);
            if (!result.Succeeded) return BadRequest(result.Errors);

            return Ok();
        }
    }
}