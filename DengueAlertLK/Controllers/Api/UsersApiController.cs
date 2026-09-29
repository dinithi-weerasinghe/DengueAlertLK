using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DengueAlertLK.Data;
using DengueAlertLK.Models.Dtos;
using DengueAlertLK.Models.Entities;
namespace DengueAlertLK.Controllers.Api;

[Authorize(Roles = "Admin")]
[ApiController]
[Route("api/users")]
public class UsersApiController : ControllerBase
{
    private readonly ApplicationDbContext _db;
    private readonly IPasswordHasher<AppUser> _hasher;
    public UsersApiController(ApplicationDbContext db, IPasswordHasher<AppUser> hasher)
    { _db = db; _hasher = hasher; }

    [HttpGet]
    public async Task<IActionResult> GetAll() =>
        Ok(await _db.Users.OrderBy(u => u.Username)
            .Select(u => new { u.Id, u.Username, u.FullName, u.Role }).ToListAsync());

    [HttpPost]
    public async Task<IActionResult> Create(UserCreateDto dto)
    {
        var username = dto.Username.Trim();
        if (await _db.Users.AnyAsync(u => u.Username == username))
            return Conflict(new { message = $"Username '{username}' already exists." });

        var user = new AppUser { Username = username, FullName = dto.FullName.Trim(), Role = dto.Role };
        user.PasswordHash = _hasher.HashPassword(user, dto.Password);
        _db.Users.Add(user);
        await _db.SaveChangesAsync();
        return Ok(new { user.Id });
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        if (id == int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!))
            return BadRequest(new { message = "You cannot delete your own account." });

        var user = await _db.Users.FindAsync(id);
        if (user == null) return NotFound(new { message = "User not found." });
        _db.Users.Remove(user);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}