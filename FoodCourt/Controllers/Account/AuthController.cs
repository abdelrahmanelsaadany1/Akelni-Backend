using System.Data;
using System.Security.Claims;
using Domain.Dtos.Auth;
using Domain.Entities.Identity;
using Google.Apis.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
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
        private readonly IConfiguration _configuration;


        public AuthController(UserManager<User> userManager,
                              SignInManager<User> signInManager,
                              JwtService jwtService,
                              EmailService emailService,
                              IConfiguration configuration)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _jwtService = jwtService;
            _emailService = emailService;
            _configuration = configuration;
        }

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
            var token = _jwtService.GenerateToken(user, roles);

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
                        //EmailConfirmed = true, 
                        DisplayName = dto.Role
                    };

                    var result = await _userManager.CreateAsync(user);
                    if (!result.Succeeded)
                    {
                        return BadRequest(new { message = "Failed to create user", errors = result.Errors });
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
