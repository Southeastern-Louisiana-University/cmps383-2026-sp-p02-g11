using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Selu383.SP26.Api.Data;
using Selu383.SP26.Api.Features.Users;

namespace Selu383.SP26.Api.Controllers;

[ApiController]
[Route("api/authentication")]
public class AuthenticationController : ControllerBase
{
    private readonly DataContext _context;

    public AuthenticationController(DataContext context)
    {
        _context = context;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] UserLoginDto request)
    {
        if (string.IsNullOrEmpty(request.UserName) || string.IsNullOrEmpty(request.Password))
            return BadRequest("Username and password are required.");

        var user = await _context.Users
            .Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.UserName == request.UserName);

        if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.HashedPassword))
            return BadRequest("Invalid username or password.");

        HttpContext.Session.SetInt32("UserId", user.Id);
        return Ok(new UserDto { Id = user.Id, UserName = user.UserName, Roles = new List<string> { user.Role!.Name } });
    }

    [HttpPost("logout")]
    public IActionResult Logout()
    {
        HttpContext.Session.Clear();
        Response.Cookies.Append(".AspNetCore.Session", "", new CookieOptions
        {
            Expires = DateTimeOffset.UtcNow.AddDays(-1),
            HttpOnly = true,
            IsEssential = true
        });
        return Ok();
    }

    [HttpGet("me")]
    public async Task<IActionResult> Me()
    {
        var userId = HttpContext.Session.GetInt32("UserId");
        if (userId == null) return Unauthorized();

        var user = await _context.Users
            .Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (user == null) return Unauthorized();
        return Ok(new UserDto { Id = user.Id, UserName = user.UserName, Roles = new List<string> { user.Role!.Name } });
    }
}

public class UserLoginDto
{
    public string UserName { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}