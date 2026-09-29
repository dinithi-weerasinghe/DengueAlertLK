using System.ComponentModel.DataAnnotations;
namespace DengueAlertLK.Models.Dtos;

public class LoginDto
{
    [Required] public string Username { get; set; } = "";
    [Required] public string Password { get; set; } = "";
}

public class ChangePasswordDto
{
    [Required] public string CurrentPassword { get; set; } = "";
    [Required, MinLength(6)] public string NewPassword { get; set; } = "";
}

public class UserCreateDto
{
    [Required, MaxLength(50)] public string Username { get; set; } = "";
    [Required, MaxLength(100)] public string FullName { get; set; } = "";
    [Required, MinLength(6)] public string Password { get; set; } = "";
    [Required, RegularExpression("^(Admin|Officer)$")] public string Role { get; set; } = "Officer";
}

public class AreaDto
{
    [Required, MaxLength(100)] public string Name { get; set; } = "";
    [Required, MaxLength(100)] public string District { get; set; } = "";
    [Range(5.5, 10.0)] public double Latitude { get; set; }
    [Range(79.0, 82.5)] public double Longitude { get; set; }
    [Range(0, 100000000)] public int Population { get; set; }
}

public class CaseSaveDto
{
    [Range(1, int.MaxValue)] public int AreaId { get; set; }
    public DateTime ReportDate { get; set; }
    [Range(1, 100000)] public int Cases { get; set; }
    [Range(0, 100000)] public int Recovered { get; set; }
    [MaxLength(300)] public string? Notes { get; set; }
}