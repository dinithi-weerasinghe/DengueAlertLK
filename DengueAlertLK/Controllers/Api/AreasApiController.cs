using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DengueAlertLK.Data;
using DengueAlertLK.Models.Dtos;
using DengueAlertLK.Models.Entities;
namespace DengueAlertLK.Controllers.Api;

[Authorize]
[ApiController]
[Route("api/areas")]
public class AreasApiController : ControllerBase
{
    private readonly ApplicationDbContext _db;
    public AreasApiController(ApplicationDbContext db) => _db = db;

    [HttpGet]
    public async Task<IActionResult> GetAll() =>
        Ok(await _db.Areas.OrderBy(a => a.Name).Select(a => new
        {
            a.Id,
            a.Name,
            a.District,
            a.Latitude,
            a.Longitude,
            a.Population,
            totalCases = a.Cases.Sum(c => c.Cases)
        }).ToListAsync());

    [HttpPost]
    public async Task<IActionResult> Create(AreaDto dto)
    {
        var name = dto.Name.Trim();
        if (await _db.Areas.AnyAsync(a => a.Name == name))
            return Conflict(new { message = $"Area '{name}' already exists." });

        var area = new Area
        {
            Name = name,
            District = dto.District.Trim(),
            Latitude = dto.Latitude,
            Longitude = dto.Longitude,
            Population = dto.Population
        };
        _db.Areas.Add(area);
        await _db.SaveChangesAsync();
        return Ok(new { area.Id });
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, AreaDto dto)
    {
        var area = await _db.Areas.FindAsync(id);
        if (area == null) return NotFound(new { message = "Area not found." });

        var name = dto.Name.Trim();
        if (await _db.Areas.AnyAsync(a => a.Name == name && a.Id != id))
            return Conflict(new { message = $"Area '{name}' already exists." });

        area.Name = name; area.District = dto.District.Trim();
        area.Latitude = dto.Latitude; area.Longitude = dto.Longitude; area.Population = dto.Population;
        await _db.SaveChangesAsync();
        return Ok(new { area.Id });
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var area = await _db.Areas.FindAsync(id);
        if (area == null) return NotFound(new { message = "Area not found." });
        if (await _db.DengueCases.AnyAsync(c => c.AreaId == id))
            return Conflict(new { message = "This area has case records and cannot be deleted." });

        _db.Areas.Remove(area);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}