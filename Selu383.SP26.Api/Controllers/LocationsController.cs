using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Selu383.SP26.Api.Data;
using Selu383.SP26.Api.Features.Locations;
using Selu383.SP26.Api.Features.Users;
using System.Security.Claims;

namespace Selu383.SP26.Api.Controllers;

[Route("api/locations")]
[ApiController]
public class LocationsController(DataContext dataContext) : ControllerBase
{
    [HttpGet]
    public IQueryable<LocationDto> GetAll()
    {
        return dataContext.Set<Location>()
            .Select(x => new LocationDto
            {
                Id = x.Id,
                Name = x.Name,
                Address = x.Address,
                TableCount = x.TableCount,
                ManagerId = x.ManagerId // Updated
            });
    }

    [HttpGet("{id}")]
    public ActionResult<LocationDto> GetById(int id)
    {
        var result = dataContext.Set<Location>()
            .FirstOrDefault(x => x.Id == id);

        if (result == null)
        {
            return NotFound();
        }

        return Ok(new LocationDto
        {
            Id = result.Id,
            Name = result.Name,
            Address = result.Address,
            TableCount = result.TableCount,
            ManagerId = result.ManagerId // Updated
        });
    }

    [HttpPost]
    [Authorize(Roles = "Admin")] // Only Admins can create
    public ActionResult<LocationDto> Create(LocationDto dto)
    {
        if (dto.TableCount < 1 || string.IsNullOrWhiteSpace(dto.Name) || dto.Name.Length > 120)
        {
            return BadRequest();
        }

        var location = new Location
        {
            Name = dto.Name,
            Address = dto.Address,
            TableCount = dto.TableCount,
            ManagerId = dto.ManagerId // Updated
        };

        dataContext.Set<Location>().Add(location);
        dataContext.SaveChanges();

        dto.Id = location.Id;

        return CreatedAtAction(nameof(GetById), new { id = dto.Id }, dto);
    }

    [HttpPut("{id}")]
    [Authorize] // Must be logged in
    public ActionResult<LocationDto> Update(int id, LocationDto dto)
    {
        var location = dataContext.Set<Location>().FirstOrDefault(x => x.Id == id);
        if (location == null)
        {
            return NotFound();
        }

        // Authorization Logic: Admin or the assigned Manager
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var isAdmin = User.IsInRole("Admin");
        var isManager = location.ManagerId == userId;

        if (!isAdmin && !isManager)
        {
            return Forbidden(); // Helper method or return StatusCode(403)
        }

        if (dto.TableCount < 1 || string.IsNullOrWhiteSpace(dto.Name) || dto.Name.Length > 120)
        {
            return BadRequest();
        }

        // Only admins can change the ManagerId
        if (isAdmin)
        {
            location.ManagerId = dto.ManagerId;
        }

        location.Name = dto.Name;
        location.Address = dto.Address;
        location.TableCount = dto.TableCount;

        dataContext.SaveChanges();

        dto.Id = location.Id;
        dto.ManagerId = location.ManagerId;

        return Ok(dto);
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")] // Only Admins can delete
    public ActionResult Delete(int id)
    {
        var location = dataContext.Set<Location>()
            .FirstOrDefault(x => x.Id == id);

        if (location == null)
        {
            return NotFound();
        }

        dataContext.Set<Location>().Remove(location);
        dataContext.SaveChanges();

        return Ok();
    }

    private ActionResult Forbidden() => StatusCode(403);
}