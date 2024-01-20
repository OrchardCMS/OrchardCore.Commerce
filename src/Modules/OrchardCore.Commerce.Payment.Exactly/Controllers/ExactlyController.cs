using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace OrchardCore.Commerce.Payment.Exactly.Controllers;

public class ExactlyController : Controller
{
    public async Task<IActionResult> CreateTransaction()
    {

    }

    public async Task<IActionResult> GetRedirectUrl()
    {

    }

    [HttpGet("checkout/middleware/Exactly")]
    public async Task<IActionResult> Middleware()
    {

    }
}
