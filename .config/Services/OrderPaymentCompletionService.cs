using System.Linq;
using AutoMapper;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using WangenPizza.Dtos;
using WangenPizza.Helper;
using WangenPizza.Interfaces;
using WangenPizza.Models;

namespace WangenPizza.Services
{
    public class OrderPaymentCompletionService : IOrderPaymentCompletionService
    {
        private readonly ICartService _cartService;
        private readonly IMailService _mailService;
        private readonly IConfiguration _configuration;
        private readonly IContactService _contactService;
        private readonly IMapper _mapper;
        private readonly IHubContext<NotificationHub> _hubContext;
        private readonly ILogger<OrderPaymentCompletionService> _logger;

        public OrderPaymentCompletionService(
            ICartService cartService,
            IMailService mailService,
            IConfiguration configuration,
            IContactService contactService,
            IMapper mapper,
            IHubContext<NotificationHub> hubContext,
            ILogger<OrderPaymentCompletionService> logger)
        {
            _cartService = cartService;
            _mailService = mailService;
            _configuration = configuration;
            _contactService = contactService;
            _mapper = mapper;
            _hubContext = hubContext;
            _logger = logger;
        }

        public async Task<OrderPaymentCompletionResult> CompleteSuccessfulPaymentAsync(int orderId)
        {
            var order = await _cartService.GetOrderById(orderId);
            if (order == null)
                return new OrderPaymentCompletionResult { NotFound = true };

            if (order.IsPaymentSucceeded)
                return new OrderPaymentCompletionResult { AlreadyProcessed = true, Order = order };

            // Wertgutschein nur bei Online-Zahlung (PaymentWay != Barzahlung); Bar-Bestellungen nie Guthaben abbuchen
            var hadGutscheinBalance =
                order.PaymentWay != 1
                && order.GutscheinDeduction > 0m
                && !string.IsNullOrWhiteSpace(order.AppliedGutscheinCode);

            order.IsPaymentSucceeded = true;
            _cartService.UpdateOrder(order);

            if (hadGutscheinBalance)
            {
                try
                {
                    await _cartService.ConsumeAppliedGutscheinAfterPaymentAsync(order.Id);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Wertgutschein-Abbuchung nach Zahlung fehlgeschlagen (OrderId={OrderId}).", order.Id);
                }
            }

            try
            {
                if (order.PaymentWay == 1)
                {
                    var date = DateTime.Now;
                    var formattedDate = date.ToString("HH:mm tt");
                    await _hubContext.Clients.All.SendAsync(
                        "ReceiveNotification",
                        $"Neue Bestellung Barzahlung {formattedDate}   Name :{order.Name} {order.PostBox} {order.City} ",
                        "cash");
                }
                else
                {
                    var date = DateTime.Now;
                    var formattedDate = date.ToString("HH:mm tt");
                    await _hubContext.Clients.All.SendAsync(
                        "ReceiveNotification",
                        $"Neue Bestellung {formattedDate}   Name :{order.Name} {order.PostBox} {order.City} ",
                        "order");
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "SignalR-Benachrichtigung fehlgeschlagen (OrderId={OrderId}).", order.Id);
            }

            try
            {
                await SendRestaurantNewOrderEmailAsync(order);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Restaurant-E-Mail für Bestellung fehlgeschlagen (OrderId={OrderId}).", order.Id);
            }

            try
            {
                var email = order.Email;
                if (!string.IsNullOrWhiteSpace(email))
                {
                    var existingContact = await _contactService.GetByEmail(email);
                    if (existingContact == null)
                    {
                        var dto = new ContactDto
                        {
                            Email = email,
                            Name = order.Name,
                            Phone = "1",
                            Message = "Created from order"
                        };
                        var data = _mapper.Map<Contact>(dto);
                        await _contactService.Create(data);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Kontakt aus Bestellung anlegen fehlgeschlagen (OrderId={OrderId}).", order.Id);
            }

            return new OrderPaymentCompletionResult { Order = order };
        }

        private async Task SendRestaurantNewOrderEmailAsync(Order order)
        {
            var mailSection = _configuration.GetSection("MailSettings");
            var restaurantTo =
                mailSection["RestaurantOrdersEmail"]?.Trim()
                ?? mailSection["OrderAdminNotifyEmail"]?.Trim()
                ?? mailSection["Mail"]?.Trim();

            if (string.IsNullOrEmpty(restaurantTo))
            {
                _logger.LogWarning(
                    "MailSettings: kein RestaurantOrdersEmail / OrderAdminNotifyEmail / Mail — Restaurant-Mail für OrderId={OrderId} übersprungen.",
                    order.Id);
                return;
            }

            var orderItemsHtml = BuildOrderItemsHtml(order);
            var deliveryTimeLabel = order.Pickup_type == "delivery"
                ? "Liefertermin"
                : order.Pickup_type == "voucher"
                    ? "Hinweis"
                    : "Abholtermin";
            var deliveryPriceLabel = order.Pickup_type == "delivery"
                ? "Lieferpreis"
                : order.Pickup_type == "voucher"
                    ? "Wertgutschein"
                    : "Abholpreis";
            var paidNote = order.PaymentWay == 1
                ? "Barzahlung bei Abholung/Lieferung"
                : "Betrag online bezahlt";

            var adminBody = $@"
<html>
  <body style='font-family: Arial, sans-serif; background-color:#f9f9f9; padding:20px;'>
    <div style='max-width:600px; margin:auto; background:#fff; border-radius:8px; padding:20px; box-shadow:0 2px 8px rgba(0,0,0,0.1);'>

      <h2 style='color:#f66f00; text-align:center; border-bottom:2px solid #f66f00; padding-bottom:10px;'>
        Neue Bestellung (Restaurant)
      </h2>

      <p style='font-size:15px; color:#333;'>
        Bestellung <b>#{order.Id}</b> — {deliveryPriceLabel}<br/>
        Kunde: <b>{System.Net.WebUtility.HtmlEncode(order.Name ?? "")}</b><br/>
        E-Mail: {System.Net.WebUtility.HtmlEncode(order.Email ?? "")}<br/>
        Tel.: {System.Net.WebUtility.HtmlEncode(order.Mobile ?? "—")}
      </p>

      <table border='1' style='border-collapse: collapse; width: 100%; margin-top:15px; font-size:14px;'>
        <thead style='background-color:#f1f1f1;'>
          <tr>
            <th style='padding:8px; text-align:center;'>Produkt</th>
            <th style='padding:8px; text-align:center;'>Menge</th>
            <th style='padding:8px; text-align:center;'>Preis (pro Stück)</th>
            <th style='padding:8px; text-align:center;'>Total</th>
          </tr>
        </thead>
        <tbody>
          {orderItemsHtml}
        </tbody>
      </table>

      <p style='margin-top:15px; font-size:15px; color:#333;'>
        <b>Total:</b>
        <span style='color:#2a9d8f; font-size:16px;'>CHF {order.FinalTotalNumber:F2}</span>
        ({paidNote})
      </p>

      <p style='margin-top:8px; font-size:15px;'>
        <b style='color:#e63946;'>{deliveryTimeLabel}:</b> {System.Net.WebUtility.HtmlEncode(order.DeliveryTime ?? "—")}
      </p>

      <p style='margin-top:8px; font-size:14px; color:#555;'>
        {System.Net.WebUtility.HtmlEncode(order.Street ?? "")}, {System.Net.WebUtility.HtmlEncode(order.PostBox ?? "")} {System.Net.WebUtility.HtmlEncode(order.City ?? "")}
      </p>
      {(string.IsNullOrWhiteSpace(order.Notes) ? "" : $"<p style='margin-top:10px;font-size:13px;color:#444;'><b>Notiz:</b> {System.Net.WebUtility.HtmlEncode(order.Notes)}</p>")}
    </div>
  </body>
</html>";

            await _mailService.SendEmailAsync(new MailRequest
            {
                ToEmail = restaurantTo,
                Subject = $"Neue Bestellung #{order.Id} — {order.Name}",
                Body = adminBody
            }, default);
        }

        private static string BuildOrderItemsHtml(Order order)
        {
            var items = order.OrderItems;
            if (items != null && items.Any())
            {
                var rows = new System.Text.StringBuilder();
                foreach (var item in items)
                {
                    var name = item.Product?.Name ?? "—";
                    var price = item.Product?.Price ?? 0m;
                    rows.Append($@"
        <tr>
            <td style='text-align: center;'>{System.Net.WebUtility.HtmlEncode(name)}</td>
            <td style='text-align: center;'>{item.Quantity}</td>
            <td style='text-align: center;'>CHF {price:F2}</td>
            <td style='text-align: center;'>CHF {(item.Quantity * price):F2}</td>
        </tr>");
                }
                return rows.ToString();
            }

            if (!string.IsNullOrEmpty(order.Notes) && order.Notes.StartsWith("WERTGUTSCHEIN", StringComparison.Ordinal))
            {
                return $@"
        <tr>
            <td colspan='4' style='text-align: center; padding:12px;'>{System.Net.WebUtility.HtmlEncode(order.Notes)}</td>
        </tr>";
            }

            return @"
        <tr>
            <td colspan='4' style='text-align: center;'>—</td>
        </tr>";
        }
    }
}
