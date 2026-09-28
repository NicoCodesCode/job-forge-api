using JobForgeAPI.Data;
using JobForgeAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace JobForgeAPI.Controllers;

[ApiController]
[Route("[controller]")]
public class AuthController : ControllerBase
{
    private readonly AppDbContext _context;

    public AuthController(AppDbContext context)
    {
        _context = context;
    }

    [HttpPost("register")]
    public async Task<IActionResult> PostRegister([FromBody] RegisterRequest request)
    {
        var isUsernameTaken = await _context.Users.FirstOrDefaultAsync(u => u.Username == request.Username);
        if (isUsernameTaken is not null) return Conflict();

        var hashedPassword = BCrypt.Net.BCrypt.HashPassword(request.Password);
        var newUser = new User
        {
            Username = request.Username,
            HashedPassword = hashedPassword,
        };

        await _context.Users.AddAsync(newUser);
        await _context.SaveChangesAsync();

        return StatusCode(201, new { newUser.Id, newUser.Username });
    }
}