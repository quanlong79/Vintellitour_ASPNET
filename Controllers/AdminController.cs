using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Threading.Tasks;
using Vintellitour_Framework.Services;
using Vintellitour_Framework.Models;
namespace Vintellitour_Framework.Controllers
{
    public class AdminController : Controller
    {
        private readonly IPostService _postService;
       
        public IActionResult dashboard()
        {

            return View();
        }

        public IActionResult locations()
        {
            return View();
        }
        //public IActionResult posts()
        //{
        //    return View();
        //}
        public IActionResult users()
        {
            return View();
        }
    }
}
