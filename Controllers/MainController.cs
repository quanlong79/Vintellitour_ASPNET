// Controllers/MainController.cs
using Microsoft.AspNetCore.Mvc;

namespace Vintellitour_Framework.Controllers
{
    public class MainController : Controller
    {
        public IActionResult AboutUs()
        {
            return View();  // Sẽ tìm Views/Main/AboutUs.cshtml
        }

        public IActionResult sharespace()
        {
            return View();  // 
        }
    }
}
