using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using PostFinanceCheckout.Model;
using WangenPizza.Interfaces;
using WangenPizza.Services;

namespace WangenPizza.Api_s.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PaymentController : ControllerBase
    {
        private readonly ICartService _cartService;
        private readonly IOrderPaymentCompletionService _orderPaymentCompletion;
        private readonly IConfiguration _configuration;
        private readonly PostFinancePaymentService _postFinance;
        private readonly ILogger<PaymentController> _logger;

        public PaymentController(
            ICartService cartService,
            IOrderPaymentCompletionService orderPaymentCompletion,
            IConfiguration configuration,
            PostFinancePaymentService postFinance,
            ILogger<PaymentController> logger)
        {
            _cartService = cartService;
            _orderPaymentCompletion = orderPaymentCompletion;
            _configuration = configuration;
            _postFinance = postFinance;
            _logger = logger;
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

        /// <summary>
        /// Server-zu-Server-Benachrichtigung von PostFinance (Webhook). Macht die Bestellabwicklung
        /// unabhängig davon, ob der Kunde nach der Zahlung (z. B. TWINT) auf die SuccessUrl zurückkommt.
        /// PostFinance Webhook-Listener: Entity "Transaction", Zustände FULFILL (und optional AUTHORIZED/COMPLETED).
        /// Ziel-URL in PostFinance: {AppUrl}/api/Payment/webhook
        /// </summary>
        [AllowAnonymous]
        [HttpPost("webhook")]
        public async Task<IActionResult> Webhook()
        {
            string body;
            using (var reader = new StreamReader(Request.Body, System.Text.Encoding.UTF8))
                body = await reader.ReadToEndAsync();

            if (string.IsNullOrWhiteSpace(body))
                return Ok();

            long transactionId;
            string? listenerName;
            try
            {
                var payload = JsonConvert.DeserializeObject<PostFinanceWebhookPayload>(body);
                if (payload == null || payload.EntityId <= 0)
                    return Ok();
                transactionId = payload.EntityId;
                listenerName = payload.ListenerEntityTechnicalName;
            }
            catch (Exception ex)
            {
                // Unlesbarer Body: bestätigen, damit PostFinance einen kaputten Aufruf nicht endlos wiederholt.
                _logger.LogWarning(ex, "PostFinance-Webhook: Body konnte nicht gelesen werden.");
                return Ok();
            }

            // Nur Transaktions-Benachrichtigungen verarbeiten.
            if (!string.IsNullOrEmpty(listenerName) &&
                !listenerName.Contains("Transaction", StringComparison.OrdinalIgnoreCase))
                return Ok();

            try
            {
                // Status autoritativ direkt bei PostFinance lesen (nie dem Body allein vertrauen).
                var state = _postFinance.GetTransactionState(transactionId);
                var isPaid =
                    state == TransactionState.FULFILL ||
                    state == TransactionState.COMPLETED ||
                    state == TransactionState.AUTHORIZED;

                if (!isPaid)
                    return Ok(); // z. B. PENDING/FAILED — keine Aktion.

                var orderId = _postFinance.GetOrderIdByTransactionId(transactionId);
                if (orderId == null)
                {
                    _logger.LogWarning(
                        "PostFinance-Webhook: keine Bestellung zu TransactionId={TransactionId} gefunden.",
                        transactionId);
                    return Ok();
                }

                // Idempotent: CompleteSuccessfulPaymentAsync ignoriert bereits bezahlte Bestellungen.
                await _orderPaymentCompletion.CompleteSuccessfulPaymentAsync(orderId.Value);
                return Ok();
            }
            catch (Exception ex)
            {
                // Vorübergehender Fehler (PostFinance-API/DB): 500 → PostFinance wiederholt den Webhook später.
                _logger.LogError(ex, "PostFinance-Webhook fehlgeschlagen (TransactionId={TransactionId}).", transactionId);
                return StatusCode(500, "Webhook processing failed.");
            }
        }

        /// <summary>
        /// Minimale Bestelldaten für die Erfolgsseite (Google Customer Reviews) — war zuvor nicht implementiert (404 / leere UX).
        /// </summary>
        [AllowAnonymous]
        [HttpGet("order-review-data")]
        public async Task<IActionResult> OrderReviewData([FromQuery] int orderId)
        {
            if (orderId <= 0)
                return BadRequest();

            var order = await _cartService.GetOrderById(orderId);
            if (order == null)
                return NotFound();

            var wertgutscheinKauf = string.Equals(
                order.Pickup_type,
                "voucher",
                StringComparison.OrdinalIgnoreCase);

            var estimated = order.DeliveryDate != default
                ? order.DeliveryDate.ToString("yyyy-MM-dd")
                : DateTime.UtcNow.AddDays(7).ToString("yyyy-MM-dd");

            return Ok(new
            {
                orderId = order.Id,
                email = order.Email ?? "",
                deliveryCountry = "CH",
                estimatedDeliveryDate = estimated,
                wertgutscheinKauf,
            });
        }

        /// <summary>Minimales Abbild der PostFinance-Webhook-Payload (Json.NET matcht Property-Namen case-insensitiv).</summary>
        private sealed class PostFinanceWebhookPayload
        {
            public long EntityId { get; set; }
            public string? ListenerEntityTechnicalName { get; set; }
            public string? State { get; set; }
            public long SpaceId { get; set; }
        }
    }
}
