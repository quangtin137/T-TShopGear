using Microsoft.AspNetCore.Mvc;

namespace GearShop.Controllers
{
    public class HuongDanController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
