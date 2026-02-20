using System.Net;
using System.Text.Json;
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
    public async Task<IActionResult> Login([FromBody] LoginDto request)
    {
        if (string.IsNullOrEmpty(request.UserName) || string.IsNullOrEmpty(request.Password))
            return BadRequest("Username and password are required.");

        var user = await _context.Users
            .Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.UserName == request.UserName);

        if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.HashedPassword))
            return BadRequest("Invalid username or password.");

        HttpContext.Session.SetInt32("UserId", user.Id);
        var userDto = new UserDto
        {
            Id = user.Id,
            UserName = user.UserName,
            Roles = [user.Role!.Name]
        };
        // turn our userDto into json
		var cookieJson = JsonSerializer.Serialize(userDto);
		Response.Cookies.Append("User", cookieJson, new CookieOptions
		{
			Expires = DateTimeOffset.UtcNow.AddHours(1),
			HttpOnly = false, 
			IsEssential = true
		});

		return Ok(userDto);
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
        return Ok(new UserDto { Id = user.Id, UserName = user.UserName, Roles = [user.Role!.Name] });
    }
}