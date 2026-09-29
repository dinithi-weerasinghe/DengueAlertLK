using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DengueAlertLK.Data;
using DengueAlertLK.Services;
namespace DengueAlertLK.Controllers.Api;

[Authorize]
[ApiController]
[Route("api/dashboard")]
public class DashboardApiController : ControllerBase
{
    private readonly ApplicationDbContext _db;
    public DashboardApiController(ApplicationDbContext db) => _db = db;

    private static (DateTime f, DateTime t) GetRange(DateTime? start, DateTime? end)
    {
        var t = (end ?? DateTime.Today).Date;
        var f = (start ?? t.AddDays(-29)).Date;
        if (f > t) (f, t) = (t, f);
        return (f, t);
    }

    private static double Pct(int cur, int prev) =>
        prev == 0 ? (cur > 0 ? 100 : 0) : Math.Round((cur - prev) * 100.0 / prev, 1);

    private async Task<int> Totals(DateTime f, DateTime t) =>
        await _db.DengueCases.Where(c => c.ReportDate >= f && c.ReportDate <= t)
            .SumAsync(c => (int?)c.Cases) ?? 0;

    // ---------- Dashboard ----------
    [HttpGet("summary")]
    public async Task<IActionResult> Summary(DateTime? start, DateTime? end)
    {
        var (f, t) = GetRange(start, end);
        var days = (t - f).Days + 1;
        var pf = f.AddDays(-days);          // previous period (same length)
        var pt = f.AddDays(-1);

        var cur = await _db.DengueCases.Include(c => c.Area)
            .Where(c => c.ReportDate >= f && c.ReportDate <= t).ToListAsync();
        var prev = await _db.DengueCases
            .Where(c => c.ReportDate >= pf && c.ReportDate <= pt).ToListAsync();

        int total = cur.Sum(c => c.Cases), prevTotal = prev.Sum(c => c.Cases);
        int rec = cur.Sum(c => c.Recovered), prevRec = prev.Sum(c => c.Recovered);

        var today = DateTime.Today;
        var week = await Totals(today.AddDays(-6), today);
        var prevWeek = await Totals(today.AddDays(-13), today.AddDays(-7));

        // weekly trend: current period vs previous period
        var weeks = (int)Math.Ceiling(days / 7.0);
        var curW = new int[weeks];
        var prevW = new int[weeks];
        foreach (var c in cur) curW[(c.ReportDate.Date - f).Days / 7] += c.Cases;
        foreach (var c in prev) prevW[(c.ReportDate.Date - pf).Days / 7] += c.Cases;

        var areaTotals = cur.GroupBy(c => c.AreaId).ToDictionary(g => g.Key, g => g.Sum(x => x.Cases));

        var byArea = cur.GroupBy(c => c.Area.Name)
            .Select(g => new { name = g.Key, cases = g.Sum(x => x.Cases) })
            .OrderByDescending(x => x.cases).Take(6).ToList();

        var risk = new[] { "High", "Medium", "Low" }.Select(l => new
        {
            level = l,
            areas = areaTotals.Count(kv => RiskHelper.Level(kv.Value) == l),
            cases = areaTotals.Where(kv => RiskHelper.Level(kv.Value) == l).Sum(kv => kv.Value)
        }).ToList();

        var recent = cur.OrderByDescending(c => c.ReportDate).ThenByDescending(c => c.Id).Take(5)
            .Select(c => new
            {
                date = c.ReportDate.ToString("yyyy-MM-dd"),
                area = c.Area.Name,
                cases = c.Cases,
                risk = RiskHelper.Level(areaTotals[c.AreaId])
            }).ToList();

        return Ok(new
        {
            rangeStart = f.ToString("yyyy-MM-dd"),
            rangeEnd = t.ToString("yyyy-MM-dd"),
            total,
            totalChange = Pct(total, prevTotal),
            newThisWeek = week,
            weekChange = Pct(week, prevWeek),
            highRiskAreas = risk.First(r => r.level == "High").areas,
            recovered = rec,
            recoveredChange = Pct(rec, prevRec),
            trend = new { labels = Enumerable.Range(1, weeks).Select(i => "Week " + i), current = curW, previous = prevW },
            byArea,
            risk,
            recent
        });
    }

    // ---------- Map (dashboard + risk map) ----------
    [HttpGet("map")]
    public async Task<IActionResult> Map(DateTime? start, DateTime? end)
    {
        var (f, t) = GetRange(start, end);
        var totals = await _db.DengueCases.Where(c => c.ReportDate >= f && c.ReportDate <= t)
            .GroupBy(c => c.AreaId)
            .Select(g => new { AreaId = g.Key, Total = g.Sum(x => x.Cases) })
            .ToDictionaryAsync(x => x.AreaId, x => x.Total);

        var areas = await _db.Areas.ToListAsync();
        return Ok(areas.Select(a =>
        {
            var n = totals.GetValueOrDefault(a.Id);
            return new { a.Id, a.Name, a.District, lat = a.Latitude, lng = a.Longitude, cases = n, risk = RiskHelper.Level(n) };
        }).OrderByDescending(x => x.cases));
    }

    // ---------- Analytics (last 12 months) ----------
    [HttpGet("analytics")]
    public async Task<IActionResult> Analytics()
    {
        var first = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1).AddMonths(-11);
        var rows = await _db.DengueCases.Where(c => c.ReportDate >= first).ToListAsync();
        var months = Enumerable.Range(0, 12).Select(i => first.AddMonths(i)).ToList();

        var cases = months.Select(m => rows.Where(r => r.ReportDate.Year == m.Year && r.ReportDate.Month == m.Month).Sum(r => r.Cases)).ToList();
        var recovered = months.Select(m => rows.Where(r => r.ReportDate.Year == m.Year && r.ReportDate.Month == m.Month).Sum(r => r.Recovered)).ToList();

        return Ok(new { labels = months.Select(m => m.ToString("MMM yyyy")), cases, recovered });
    }

    // ---------- Report ----------
    [HttpGet("report")]
    public async Task<IActionResult> Report(DateTime? start, DateTime? end)
    {
        var (f, t) = GetRange(start, end);
        var rows = await _db.DengueCases.Include(c => c.Area)
            .Where(c => c.ReportDate >= f && c.ReportDate <= t).ToListAsync();

        var list = rows.GroupBy(c => c.AreaId).Select(g => new
        {
            area = g.First().Area.Name,
            district = g.First().Area.District,
            entries = g.Count(),
            cases = g.Sum(x => x.Cases),
            recovered = g.Sum(x => x.Recovered),
            risk = RiskHelper.Level(g.Sum(x => x.Cases))
        }).OrderByDescending(x => x.cases).ToList();

        return Ok(new
        {
            rangeStart = f.ToString("yyyy-MM-dd"),
            rangeEnd = t.ToString("yyyy-MM-dd"),
            rows = list,
            totalCases = list.Sum(x => x.cases),
            totalRecovered = list.Sum(x => x.recovered)
        });
    }
}