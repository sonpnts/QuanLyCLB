using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyCLB.API.Data;
using QuanLyCLB.API.Models;
using QuanLyCLB.API.Services;
using QuanLyCLB.API.DTOs;

namespace QuanLyCLB.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly QuanLyCLBContext _context;
        private readonly ITokenService _tokenService;

        public AuthController(QuanLyCLBContext context, ITokenService tokenService)
        {
            _context = context;
            _tokenService = tokenService;
        }

        [HttpPost("login")]
        public async Task<ActionResult<LoginResponseDto>> Login(LoginDto loginDto)
        {
            // For demo purposes, we'll create a simple email-based login
            // In production, you'd implement proper password authentication
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == loginDto.Email);

            if (user == null)
            {
                return Unauthorized("Invalid credentials");
            }

            var token = _tokenService.GenerateToken(user);

            var response = new LoginResponseDto
            {
                Token = token,
                User = new UserDto
                {
                    Id = user.Id,
                    FullName = user.FullName,
                    Email = user.Email,
                    PhoneNumber = user.PhoneNumber,
                    Role = user.Role.ToString(),
                    CreatedAt = user.CreatedAt
                }
            };

            return Ok(response);
        }

        [HttpPost("google-login")]
        public async Task<ActionResult<LoginResponseDto>> GoogleLogin(GoogleLoginDto googleLoginDto)
        {
            // In a real implementation, you would validate the Google ID token here
            // For demo purposes, we'll just check if the user exists or create them
            
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.GoogleId == googleLoginDto.GoogleId || u.Email == googleLoginDto.Email);

            if (user == null)
            {
                // Create new user from Google profile
                user = new User
                {
                    FullName = googleLoginDto.Name,
                    Email = googleLoginDto.Email,
                    GoogleId = googleLoginDto.GoogleId,
                    Role = UserRole.Trainer // Default role, can be changed by admin
                };

                _context.Users.Add(user);
                await _context.SaveChangesAsync();
            }
            else if (string.IsNullOrEmpty(user.GoogleId))
            {
                // Link existing user with Google account
                user.GoogleId = googleLoginDto.GoogleId;
                await _context.SaveChangesAsync();
            }

            var token = _tokenService.GenerateToken(user);

            var response = new LoginResponseDto
            {
                Token = token,
                User = new UserDto
                {
                    Id = user.Id,
                    FullName = user.FullName,
                    Email = user.Email,
                    PhoneNumber = user.PhoneNumber,
                    Role = user.Role.ToString(),
                    CreatedAt = user.CreatedAt
                }
            };

            return Ok(response);
        }

        [HttpPost("register")]
        public async Task<ActionResult<LoginResponseDto>> Register(RegisterDto registerDto)
        {
            if (await _context.Users.AnyAsync(u => u.Email == registerDto.Email))
            {
                return BadRequest("Email already exists");
            }

            var user = new User
            {
                FullName = registerDto.FullName,
                Email = registerDto.Email,
                PhoneNumber = registerDto.PhoneNumber,
                Role = UserRole.Trainer // Default role
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var token = _tokenService.GenerateToken(user);

            var response = new LoginResponseDto
            {
                Token = token,
                User = new UserDto
                {
                    Id = user.Id,
                    FullName = user.FullName,
                    Email = user.Email,
                    PhoneNumber = user.PhoneNumber,
                    Role = user.Role.ToString(),
                    CreatedAt = user.CreatedAt
                }
            };

            return Ok(response);
        }
    }
}