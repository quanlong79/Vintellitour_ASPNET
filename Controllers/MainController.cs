
using Microsoft.AspNetCore.Mvc;

namespace Vintellitour_Framework.Controllers
{
    public class MainController : Controller
    {
        public IActionResult mainpage()
        {
            return View();
        }
        public IActionResult sharespace()
        {
            return View();
        }
        public IActionResult AboutUs()
        {
            return View(); 
        }
    }
}
