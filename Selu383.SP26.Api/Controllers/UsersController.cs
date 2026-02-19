using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Selu383.SP26.Api.Data;
using Selu383.SP26.Api.Features.Users;

namespace Selu383.SP26.Api.Controllers;

[ApiController]
[Route("api/users")]
public class UsersController(DataContext dataContext) : ControllerBase
{
    private bool IsLoggedIn() => HttpContext.Session.GetInt32("UserId") != null;

    private bool IsAdmin()
    {
        var userId = HttpContext.Session.GetInt32("UserId");
        if (userId == null) return false;
        var user = dataContext.Users.Include(u => u.Role).FirstOrDefault(u => u.Id == userId);
        return user?.Role?.Name == "Admin";
    }

    [HttpPost]
    public async Task<IActionResult> CreateUser([FromBody] CreateUserDto request)
    {
        if (!IsLoggedIn()) return Unauthorized();
        if (!IsAdmin()) return StatusCode(403);

        if (string.IsNullOrEmpty(request.UserName)) return BadRequest("Username is required.");
        if (string.IsNullOrEmpty(request.Password)) return BadRequest("Password is required.");
        if (request.Roles == null || request.Roles.Length == 0) return BadRequest("Role is required.");
        if (request.Password.Length < 8) return BadRequest("Password too short.");
        if (!request.Password.Any(char.IsUpper) || !request.Password.Any(char.IsDigit) || !request.Password.Any(char.IsLower) || !request.Password.Any(c => !char.IsLetterOrDigit(c)))
            return BadRequest("Password must contain uppercase, lowercase, digit, and special character.");

        var roleName = request.Roles[0];
        var role = await dataContext.Roles.FirstOrDefaultAsync(r => r.Name.ToLower() == roleName.ToLower());
        if (role == null) return BadRequest($"Role '{roleName}' not found.");

        if (await dataContext.Users.AnyAsync(u => u.UserName == request.UserName))
            return BadRequest($"Username '{request.UserName}' already exists.");

        var user = new User
        {
            UserName = request.UserName,
            HashedPassword = BCrypt.Net.BCrypt.HashPassword(request.Password),
            RoleId = role.Id
        };

        dataContext.Users.Add(user);
        await dataContext.SaveChangesAsync();

        return Ok(new UserDto
        {
            Id = user.Id,
            UserName = user.UserName,
            Roles = new List<string> { role.Name }
        });
    }
}

public class CreateUserDto
{
    public string UserName { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string[] Roles { get; set; } = Array.Empty<string>();
}