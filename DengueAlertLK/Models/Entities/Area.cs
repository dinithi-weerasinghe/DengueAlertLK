using System.ComponentModel.DataAnnotations;
namespace DengueAlertLK.Models.Entities;

public class Area
{
    public int Id { get; set; }
    [Required, MaxLength(100)] public string Name { get; set; } = "";
    [Required, MaxLength(100)] public string District { get; set; } = "";
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public int Population { get; set; }
    public List<DengueCase> Cases { get; set; } = new();
}