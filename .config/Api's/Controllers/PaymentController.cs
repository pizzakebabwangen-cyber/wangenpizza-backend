using Microsoft.AspNetCore.Mvc;
using WangenPizza.Interfaces;

namespace WangenPizza.Api_s.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PaymentController : ControllerBase
    {
        private readonly ICartService _cartService;
        private readonly IOrderPaymentCompletionService _orderPaymentCompletion;
        private readonly IConfiguration _configuration;

        public PaymentController(
            ICartService cartService,
            IOrderPaymentCompletionService orderPaymentCompletion,
            IConfiguration configuration)
        {
            _cartService = cartService;
            _orderPaymentCompletion = orderPaymentCompletion;
            _configuration = configuration;
        }

        private bool IsSpaFinalizeRequest() =>
            string.Equals(Request.Headers["X-Requested-With"], "XMLHttpRequest", StringComparison.OrdinalIgnoreCase);

        private string FrontendOrigin() =>
            (_configuration["FrontendAppUrl"] ?? "https://pizzawangen.ch").TrimEnd('/');

        [HttpGet("success")]
        public async Task<IActionResult> Success(int orderId)
        {
            try
            {
                var outcome = await _orderPaymentCompletion.CompleteSuccessfulPaymentAsync(orderId);
                if (outcome.NotFound)
                {
                    if (IsSpaFinalizeRequest())
                        return NotFound("Order not found");
                    return Redirect($"{FrontendOrigin()}/cart");
                }
                if (outcome.AlreadyProcessed)
                {
                    if (IsSpaFinalizeRequest())
                        return Ok(new { message = "Payment already processed", paymentWay = outcome.Order?.PaymentWay });
                    return Redirect($"{FrontendOrigin()}/success/{orderId}");
                }
                if (IsSpaFinalizeRequest())
                    return Ok(new { paymentWay = outcome.Order?.PaymentWay });
                return Redirect($"{FrontendOrigin()}/success/{orderId}");
            }
            catch (Exception ex)
            {
                if (IsSpaFinalizeRequest())
                    return StatusCode(500, $"Payment completion failed: {ex.Message}");
                return Redirect("https://admin.pizzawangen.ch/Templates/payment-failed.html");
            }
        }

        [HttpGet("failed")]
        public async Task<IActionResult> Failed(int orderId)
        {
            var order = await _cartService.GetOrderById(orderId);
            if (order != null && !order.IsPaymentSucceeded)
                _cartService.DeleteOrder(order);
            return Redirect("https://admin.pizzawangen.ch/Templates/payment-failed.html");
        }
    }
}
