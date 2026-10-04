using JobForgeAPI.Data;
using JobForgeAPI.Models;
using JobForgeAPI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace JobForgeAPI.Controllers;

[ApiController]
[Route("[controller]")]
public class AuthController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly TokenService _tokenService;

    public AuthController(AppDbContext context,  TokenService tokenService)
    {
        _context = context;
        _tokenService = tokenService;
    }

    [HttpPost("register")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> PostRegister([FromBody] RegisterRequest request)
    {
        var isUsernameTaken = await _context.Users.FirstOrDefaultAsync(u => u.Username == request.Username);
        if (isUsernameTaken is not null) return Conflict();

        var hashedPassword = BCrypt.Net.BCrypt.HashPassword(request.Password);
    
        var organization = await _context.Organizations.FirstOrDefaultAsync(o => o.Name == request.OrganizationName);
        
        var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            if (organization is null)
            {
                organization = new Organization
                {
                    Name = request.OrganizationName,
                };
            
                await _context.Organizations.AddAsync(organization);
                await _context.SaveChangesAsync();
            }
            
            var newUser = new User
            {
                Username = request.Username,
                HashedPassword = hashedPassword,
                OrganizationId = organization.Id,
            };

            await _context.Users.AddAsync(newUser);
            await _context.SaveChangesAsync();

            await transaction.CommitAsync();

            return StatusCode(201, new { newUser.Id, newUser.Username, OrganizationName = organization.Name});
        }
        catch
        {
            await transaction.RollbackAsync();
            return Problem();
        }
    }

    [HttpPost("login")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> PostLogin([FromBody] LoginRequest request)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == request.Username);
        if (user is null) return Unauthorized();

        var isPasswordCorrect = BCrypt.Net.BCrypt.Verify(request.Password, user.HashedPassword);
        if (!isPasswordCorrect) return Unauthorized();
        
        return Ok(_tokenService.GenerateToken(user));
    }
}