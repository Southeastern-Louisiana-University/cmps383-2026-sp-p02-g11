using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Selu383.SP26.Api.Data;
using Selu383.SP26.Api.Features.Locations;
using Selu383.SP26.Api.Features.Users;

namespace Selu383.SP26.Api.Controllers;

[Route("api/locations")]
[ApiController]
public class LocationsController(DataContext dataContext) : ControllerBase
{
    private int? GetUserId() => HttpContext.Session.GetInt32("UserId");
    private bool IsLoggedIn() => GetUserId() != null;

    private bool IsAdmin()
    {
        var userId = GetUserId();
        if (userId == null) return false;
        var user = dataContext.Users.Include(u => u.Role).FirstOrDefault(u => u.Id == userId);
        return user?.Role?.Name == "Admin";
    }

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
                ManagerId = x.ManagerId,
            });
    }

    [HttpGet("{id}")]
    public ActionResult<LocationDto> GetById(int id)
    {
        var result = dataContext.Set<Location>().FirstOrDefault(x => x.Id == id);
        if (result == null) return NotFound();
        return Ok(new LocationDto
        {
            Id = result.Id,
            Name = result.Name,
            Address = result.Address,
            TableCount = result.TableCount,
            ManagerId = result.ManagerId,
        });
    }

    [HttpPost]
    public ActionResult<LocationDto> Create(LocationDto dto)
    {
        if (!IsLoggedIn()) return Unauthorized();
        if (!IsAdmin()) return StatusCode(403);
        if (dto.TableCount < 1) return BadRequest();

        var location = new Location
        {
            Name = dto.Name,
            Address = dto.Address,
            TableCount = dto.TableCount,
            ManagerId = dto.ManagerId,
        };
        dataContext.Set<Location>().Add(location);
        dataContext.SaveChanges();
        dto.Id = location.Id;
        dto.ManagerId = location.ManagerId;
        return CreatedAtAction(nameof(GetById), new { id = dto.Id }, dto);
    }

    [HttpPut("{id}")]
    public ActionResult<LocationDto> Update(int id, LocationDto dto)
    {
        if (!IsLoggedIn()) return Unauthorized();

        var location = dataContext.Set<Location>().FirstOrDefault(x => x.Id == id);
        if (location == null) return NotFound();

        if (!IsAdmin() && location.ManagerId != GetUserId()) return StatusCode(403);
        if (dto.TableCount < 1) return BadRequest();

        location.Name = dto.Name;
        location.Address = dto.Address;
        location.TableCount = dto.TableCount;
        location.ManagerId = dto.ManagerId;
        dataContext.SaveChanges();
        dto.Id = location.Id;
        dto.ManagerId = location.ManagerId;
        return Ok(dto);
    }

    [HttpDelete("{id}")]
    public ActionResult Delete(int id)
    {
        if (!IsLoggedIn()) return Unauthorized();

        var location = dataContext.Set<Location>().FirstOrDefault(x => x.Id == id);
        if (location == null) return NotFound();

        if (!IsAdmin() && location.ManagerId != GetUserId()) return StatusCode(403);

        dataContext.Set<Location>().Remove(location);
        dataContext.SaveChanges();
        return Ok();
    }
}