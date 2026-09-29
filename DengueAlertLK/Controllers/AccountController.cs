using Microsoft.AspNetCore.Mvc;
namespace DengueAlertLK.Controllers;

public class AccountController : Controller
{
    public IActionResult Login() =>
        User.Identity?.IsAuthenticated == true ? RedirectToAction("Index", "Home") : View();
}