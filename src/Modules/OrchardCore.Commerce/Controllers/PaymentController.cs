using Microsoft.AspNetCore.Mvc;
using OrchardCore.Commerce.Abstractions;
using OrchardCore.Commerce.ViewModels;
using System.Threading.Tasks;

namespace OrchardCore.Commerce.Controllers;

public class PaymentController : Controller
{
    private readonly ICardPaymentService _cardPaymentService;

    public PaymentController(ICardPaymentService cardPaymentService) =>
        _cardPaymentService = cardPaymentService;

    [Route("checkout")]
    public IActionResult Index() =>
        View();

    [Route("checkout")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Index(CardPaymentViewModel viewModel)
    {
        var receiptViewModel = await _cardPaymentService.CreatePaymentAndOrderAsync(viewModel);
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
