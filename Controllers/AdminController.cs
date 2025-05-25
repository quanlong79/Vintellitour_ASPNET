using Microsoft.AspNetCore.Mvc;
//fix

namespace Vintellitour_Framework.Controllers
{
    public class AdminController : Controller
    {
        public IActionResult dashboard()
        {
            return View();
        }

        public IActionResult locations()
        {
            return View();
        }
        public IActionResult posts()
        {
            return View();
        }
        public IActionResult users()
        {
            return View();
        }
    }
}
