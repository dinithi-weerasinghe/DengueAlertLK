using System.ComponentModel.DataAnnotations;
namespace DengueAlertLK.Models.Entities;

public class AppUser
{
    public int Id { get; set; }
    [Required, MaxLength(50)] public string Username { get; set; } = "";
    [Required] public string PasswordHash { get; set; } = "";
    [MaxLength(100)] public string FullName { get; set; } = "";
    [MaxLength(20)] public string Role { get; set; } = "Officer";   // Admin | Officer
}