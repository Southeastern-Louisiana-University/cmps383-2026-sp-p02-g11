using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Selu383.SP26.Api.Features.Users;

namespace Selu383.SP26.Api.Controllers;

[Route("api/users")]
[ApiController]
[Authorize(Roles = "Admin")] // Requirement: Only Admins can access this controller
public class UsersController(
    UserManager<User> userManager,
    RoleManager<Role> roleManager // Required to validate role existence
) : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<UserDto>> Create(CreateUserDto dto)
    {
        // 1. Basic validation for required fields
        if (string.IsNullOrWhiteSpace(dto.UserName) ||
            dto.Roles == null ||
            dto.Roles.Length == 0 ||
            string.IsNullOrWhiteSpace(dto.Password))
        {
            return BadRequest();
        }

        // 2. Role Validation: Ensure all requested roles exist in the system
        // This fixes the CreateUser_NoSuchRole_Returns400 test failure
        foreach (var roleName in dto.Roles)
        {
            var roleExists = await roleManager.RoleExistsAsync(roleName);
            if (!roleExists)
            {
                return BadRequest();
            }
        }

        // 3. Create the Identity User instance
        var newUser = new User
        {
            UserName = dto.UserName
        };

        // 4. Save the user with their password (Identity handles hashing)
        var result = await userManager.CreateAsync(newUser, dto.Password);

        if (!result.Succeeded)
        {
            // Likely reasons: User already exists or password is too simple
            return BadRequest();
        }

        // 5. Assign the validated roles to the new user
        await userManager.AddToRolesAsync(newUser, dto.Roles);

        // 6. Return the UserDto as required by the Phase 2 specs
        return Ok(new UserDto
        {
            Id = newUser.Id,
            UserName = newUser.UserName!,
            Roles = dto.Roles
        });
    }
}