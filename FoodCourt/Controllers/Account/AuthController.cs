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

        //[HttpPost("register")]
        //public async Task<IActionResult> Register(RegisterDto dto)
        //{
        //    var user = new User { Email = dto.Email, UserName = dto.Email, DisplayName = dto.Role };

        //    var result = await _userManager.CreateAsync(user, dto.Password);
        //    if (!result.Succeeded)
        //        return BadRequest(result.Errors);

        //    await _userManager.AddToRoleAsync(user, dto.Role);
        //    var roles = await _userManager.GetRolesAsync(user);
        //    var token = _jwtService.GenerateToken(user, roles);

        //    return Ok(new { token });
        //}

        //[HttpPost("register")]
        //public async Task<IActionResult> Register(RegisterDto dto)
        //{
        //    try
        //    {
        //        // Add logging
        //        Console.WriteLine($"Registration attempt for: {dto.Email}");

        //        var user = new User { Email = dto.Email, UserName = dto.Email, DisplayName = dto.Role };

        //        var result = await _userManager.CreateAsync(user, dto.Password);
        //        if (!result.Succeeded)
        //        {
        //            Console.WriteLine($"User creation failed: {string.Join(", ", result.Errors.Select(e => e.Description))}");
        //            return BadRequest(result.Errors);
        //        }

        //        await _userManager.AddToRoleAsync(user, dto.Role);
        //        var roles = await _userManager.GetRolesAsync(user);
        //        var token = _jwtService.GenerateToken(user, roles);

        //        return Ok(new { token });
        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine($"Registration error: {ex.Message}");
        //        return StatusCode(500, new { message = "Internal server error", details = ex.Message });
        //    }
        //}
        [HttpGet("test")]
        public IActionResult Test()
        {
            return Ok(new { message = "AuthController is working!", timestamp = DateTime.UtcNow });
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

        //// General temporary endpoint for testing purposes
        //[HttpGet("test")]
        //[Authorize]
        //public IActionResult Test()
        //{
        //    return Ok("AuthController is working!");
        //}

        //// General temporary endpoint for Admin testing purposes
        //[HttpGet("testA")]
        //[Authorize(Roles = "Admin")]
        //public IActionResult TestA()
        //{
        //    return Ok("Admin only is working!");
        //}

        //// General temporary endpoint for Chef testing purposes
        //[HttpGet("testChef")]
        //[Authorize(Roles = "Chef")]
        //public IActionResult TestCef()
        //{
        //    return Ok("Chef only is working!");
        //}

        //// General temporary endpoint for Customer testing purposes
        //[HttpGet("testCustomer")]
        //[Authorize(Roles = "Customer")]
        //public IActionResult TestCustomer()
        //{
        //    return Ok("Customer is working!");
        //}

        ////[HttpPost("external-login")]
        //[HttpPost("external/google")]
        //public async Task<IActionResult> ExternalLogin([FromBody] ExternalAuthDto dto)
        //{
        //    if (dto.Provider == "Google")
        //    {
        //        var payload = await GoogleJsonWebSignature.ValidateAsync(dto.IdToken);
        //        var user = await _userManager.FindByEmailAsync(payload.Email) ??
        //                   new User { Email = payload.Email, UserName = payload.Email };

        //        if (user.Id == null)
        //        {
        //            var createResult = await _userManager.CreateAsync(user);
        //            if (!createResult.Succeeded) return BadRequest(createResult.Errors);

        //            await _userManager.AddToRoleAsync(user, "Customer"); // Default role
        //        }

        //        var roles = await _userManager.GetRolesAsync(user);
        //        var token = _jwtService.GenerateToken(user, roles);
        //        return Ok(new { token });
        //    }

        //    //// Similar Facebook flow (if needed)
        //    //else if(dto.Provider == "Facebook")
        //    //{
        //    //    var payloadF = await FacebookJsonWebSignature.ValidateAsync(dto.IdToken);
        //    //    var userF = await _userManager.FindByEmailAsync(payloadF.Email) ??
        //    //               new User { Email = payloadF.Email, UserName = payloadF.Email };

        //    //    if (userF.Id == null)
        //    //    {
        //    //        var createResult = await _userManager.CreateAsync(userF);
        //    //        if (!createResult.Succeeded) return BadRequest(createResult.Errors);
        //    //        await _userManager.AddToRoleAsync(userF, "Customer"); // Default role
        //    //    }
        //    //    var rolesF = await _userManager.GetRolesAsync(userF);
        //    //    var tokenF = _jwtService.GenerateToken(userF, rolesF);
        //    //    return Ok(new { token = tokenF });
        //    //}

        //    return BadRequest("Unsupported provider");
        //}


        //[HttpPost("google")]
        //public async Task<IActionResult> GoogleLogin([FromBody] ExternalAuthDto dto)
        //{
        //    try
        //    {
        //            // Validate the Google JWT token
        //            var googleClientId = _configuration["Authentication:Google:ClientId"];
        //            var payload = await GoogleJsonWebSignature.ValidateAsync(dto.IdToken, new GoogleJsonWebSignature.ValidationSettings
        //            {
        //                Audience = new[] { googleClientId }
        //            });

        //            // You can now use payload.Email, payload.Name, etc.
        //            // Proceed with your user registration/login logic

        //            return Ok(new { message = "Google token validated", email = payload.Email });
        //    }
        //    catch (InvalidJwtException ex)
        //    {
        //        return Unauthorized(new { message = "Invalid Google token", details = ex.Message });
        //    }
        //}


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

        //[HttpPost("forgot-password")]
        //public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDto dto)
        //{
        //    var user = await _userManager.FindByEmailAsync(dto.Email);
        //    if (user == null) return Ok();

        //    var token = await _userManager.GeneratePasswordResetTokenAsync(user);
        //    await _emailService.SendResetLink(user.Email, token);
        //    return Ok();
        //}

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
