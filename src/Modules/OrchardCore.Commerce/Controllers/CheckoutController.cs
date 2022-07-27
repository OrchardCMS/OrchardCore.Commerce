using Microsoft.AspNetCore.Mvc;

namespace OrchardCore.Commerce.Controllers;
public class CheckoutController : Controller
{
    [Route("checkout")]
    public IActionResult Index() =>
        View();
}
