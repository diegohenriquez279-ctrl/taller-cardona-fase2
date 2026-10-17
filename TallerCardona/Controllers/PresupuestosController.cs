
using Microsoft.AspNetCore.Mvc;
using TallerCardona.Models;

namespace TallerCardona.Controllers;

public class PresupuestosController : Controller
{
    public IActionResult Index()
    {
        return View();
    }
}
