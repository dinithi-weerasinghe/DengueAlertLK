using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DengueAlertLK.Data;
using DengueAlertLK.Models.Dtos;
using DengueAlertLK.Models.Entities;
using DengueAlertLK.Services;
namespace DengueAlertLK.Controllers.Api;

[Authorize]
[ApiController]
[Route("api/cases")]
public class CasesApiController : ControllerBase
{
    private readonly ApplicationDbContext _db;
    public CasesApiController(ApplicationDbContext db) => _db = db;

    // Risk of an area = total cases in the last 30 days
    private async Task<Dictionary<int, int>> AreaTotals()
    {
        var since = DateTime.Today.AddDays(-29);
        return await _db.DengueCases.Where(c => c.ReportDate >= since)
            .GroupBy(c => c.AreaId)
            .Select(g => new { g.Key, Total = g.Sum(x => x.Cases) })
            .ToDictionaryAsync(x => x.Key, x => x.Total);
    }

    private static object ToObj(DengueCase c, Dictionary<int, int> totals) => new
    {
        c.Id,
        c.AreaId,
        areaName = c.Area.Name,
        date = c.ReportDate.ToString("yyyy-MM-dd"),
        cases = c.Cases,
        recovered = c.Recovered,
        notes = c.Notes,
        risk = RiskHelper.Level(totals.GetValueOrDefault(c.AreaId))
    };

    private async Task<string?> Validate(CaseSaveDto dto)
    {
        if (dto.ReportDate.Date > DateTime.Today) return "Report date cannot be in the future.";
        if (!await _db.Areas.AnyAsync(a => a.Id == dto.AreaId)) return "Selected area was not found.";
        return null;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(DateTime? start, DateTime? end, int? areaId, string? search)
    {
        var q = _db.DengueCases.Include(c => c.Area).AsQueryable();
        if (start.HasValue) q = q.Where(c => c.ReportDate >= start.Value.Date);
        if (end.HasValue) q = q.Where(c => c.ReportDate <= end.Value.Date);
        if (areaId.HasValue) q = q.Where(c => c.AreaId == areaId.Value);
        if (!string.IsNullOrWhiteSpace(search))
            q = q.Where(c => c.Area.Name.Contains(search) || (c.Notes != null && c.Notes.Contains(search)));

        var list = await q.OrderByDescending(c => c.ReportDate).ThenByDescending(c => c.Id).Take(500).ToListAsync();
        var totals = await AreaTotals();
        return Ok(list.Select(c => ToObj(c, totals)));
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var c = await _db.DengueCases.Include(x => x.Area).FirstOrDefaultAsync(x => x.Id == id);
        return c == null ? NotFound(new { message = "Case not found." }) : Ok(ToObj(c, await AreaTotals()));
    }

    [HttpPost]
    public async Task<IActionResult> Create(CaseSaveDto dto)
    {
        var err = await Validate(dto);
        if (err != null) return BadRequest(new { message = err });

        var c = new DengueCase
        {
            AreaId = dto.AreaId,
            ReportDate = dto.ReportDate.Date,
            Cases = dto.Cases,
            Recovered = dto.Recovered,
            Notes = dto.Notes?.Trim()
        };
        _db.DengueCases.Add(c);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(GetById), new { id = c.Id }, new { c.Id });
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, CaseSaveDto dto)
    {
        var c = await _db.DengueCases.FindAsync(id);
        if (c == null) return NotFound(new { message = "Case not found." });

        var err = await Validate(dto);
        if (err != null) return BadRequest(new { message = err });

        c.AreaId = dto.AreaId; c.ReportDate = dto.ReportDate.Date; c.Cases = dto.Cases;
        c.Recovered = dto.Recovered; c.Notes = dto.Notes?.Trim();
        await _db.SaveChangesAsync();
        return Ok(new { c.Id });
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var c = await _db.DengueCases.FindAsync(id);
        if (c == null) return NotFound(new { message = "Case not found." });
        _db.DengueCases.Remove(c);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}