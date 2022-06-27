using Microsoft.AspNetCore.Mvc;
using OrchardCore.Commerce.Abstractions;
using OrchardCore.Commerce.Models;
using OrchardCore.Commerce.ViewModels;
using OrchardCore.ContentManagement;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace OrchardCore.Commerce.Controllers;

public class PaymentController : Controller
{
    private readonly ICardPaymentService _cardPaymentService;
    private readonly IContentManager _contentManager;

    public PaymentController(ICardPaymentService cardPaymentService, IContentManager contentManager)
    {
        _cardPaymentService = cardPaymentService;
        _contentManager = contentManager;
    }

    [Route("payment")]
    [HttpGet]
    public IActionResult Index() =>
        View();

    [Route("payment")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Index(CardPaymentViewModel viewModel)
    {
        var receiptViewModel = await _cardPaymentService.CreateAsync(viewModel);

        return RedirectToAction("Receipt", "Payment", receiptViewModel);
    }

    [HttpGet]
    public IActionResult Receipt(CardPaymentReceiptViewModel viewModel) =>
        View(viewModel);

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error() =>
        View(new CardPaymentErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
}
