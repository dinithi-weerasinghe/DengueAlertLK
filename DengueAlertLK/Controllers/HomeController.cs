using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
namespace DengueAlertLK.Controllers;

[Authorize]
public class HomeController : Controller
{
    public IActionResult Index() => View();          // Dashboard
    public IActionResult Cases() => View();
    public IActionResult Areas() => View();
    public IActionResult Analytics() => View();
    public IActionResult RiskMap() => View();
    public IActionResult Awareness() => View();
    [Authorize(Roles = "Admin")] public IActionResult Users() => View();
    public IActionResult Reports() => View();
    public IActionResult Settings() => View();
}