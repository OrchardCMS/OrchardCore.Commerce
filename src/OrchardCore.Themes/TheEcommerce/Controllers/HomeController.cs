using Microsoft.AspNetCore.Mvc;

namespace OrchardCore.TheEcommerce.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
