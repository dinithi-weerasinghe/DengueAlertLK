using System.ComponentModel.DataAnnotations;
namespace DengueAlertLK.Models.Entities;

public class DengueCase
{
    public int Id { get; set; }
    public int AreaId { get; set; }
    public Area Area { get; set; } = null!;
    public DateTime ReportDate { get; set; }
    public int Cases { get; set; }
    public int Recovered { get; set; }
    [MaxLength(300)] public string? Notes { get; set; }
}