using Microsoft.AspNetCore.Mvc;
using OrchardCore.Commerce.Abstractions;
using OrchardCore.Commerce.Models;
using OrchardCore.Commerce.ViewModels;
using OrchardCore.ContentManagement;
using Stripe;
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

    [Route("checkout")]
    public IActionResult Index() =>
        View();

    [Route("checkout")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Index(CardPaymentViewModel viewModel)
    {
        var receiptViewModel = await _cardPaymentService.CreateAsync(viewModel);
        var exception = receiptViewModel.Exception;

        if (exception == null)
        {
            return RedirectToAction("Receipt", "Payment", receiptViewModel);
        }

        var stripeError = exception.StripeError;
        return RedirectToAction(
            "Error",
            "Payment",
            new CardPaymentErrorViewModel { Descripton = stripeError.Message, Code = stripeError.Code });

    }

    [Route("receipt")]
    public IActionResult Receipt(CardPaymentReceiptViewModel viewModel) =>
        View(viewModel);

    [Route("error")]
    public IActionResult Error(CardPaymentErrorViewModel viewModel) =>
        View("Error", viewModel);
}
