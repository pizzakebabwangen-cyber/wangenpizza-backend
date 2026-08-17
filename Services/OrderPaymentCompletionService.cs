using System.Linq;
using AutoMapper;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using System.Text;
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

            if (IsVoucherOrder(order))
            {
                try
                {
                    await _cartService.IssuePurchasedWertgutscheinCodesIfNeededAsync(order.Id);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Wertgutschein-Codes konnten nicht ausgestellt werden (OrderId={OrderId}).", order.Id);
                }

                try
                {
                    var refreshed = await _cartService.GetOrderById(order.Id);
                    if (refreshed != null)
                        order = refreshed;
                    await SendVoucherCustomerEmailAsync(order);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Wertgutschein-Kundenmail (PDF) fehlgeschlagen (OrderId={OrderId}).", order.Id);
                }
            }
            else
            {
                // Normale Bestellung: keine Kunden-Mail bei Zahlungsabschluss — PDF-Bestätigung nur nach POS «Akzeptieren»
                // (OrderController.AcknowledgePosOrder mit Anhang).
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
            var orderWishDisplay = OrderConfirmationMailHelper.FormatOrderWishDisplay(order);
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
        <b style='color:#e63946;'>{deliveryTimeLabel}:</b> {System.Net.WebUtility.HtmlEncode(orderWishDisplay)}
      </p>

      <p style='margin-top:8px; font-size:14px; color:#555;'>
        {System.Net.WebUtility.HtmlEncode(order.Street ?? "")}, {System.Net.WebUtility.HtmlEncode(order.PostBox ?? "")} {System.Net.WebUtility.HtmlEncode(order.City ?? "")}
      </p>
      {(string.IsNullOrWhiteSpace(order.Notes) ? "" : $"<p style='margin-top:10px;font-size:13px;color:#444;'><b>Notiz:</b> {System.Net.WebUtility.HtmlEncode(order.Notes)}</p>")}
    </div>
  </body>
</html>";

            var pdfBytes = OrderSummaryPdf.Generate(order, 0);

            await _mailService.SendEmailAsync(new MailRequest
            {
                ToEmail = restaurantTo,
                Subject = $"Neue Bestellung #{order.Id} — {order.Name}",
                Body = adminBody,
                Attachments = new List<FileAttachment>
                {
                    new FileAttachment
                    {
                        File = pdfBytes,
                        Name = $"Bestellung-{order.Id}.pdf",
                        ContentType = "application/pdf"
                    }
                }
            }, default);
        }

        private static bool IsVoucherOrder(Order order) =>
            string.Equals(order.Pickup_type, "voucher", StringComparison.OrdinalIgnoreCase) ||
            (!string.IsNullOrWhiteSpace(order.Notes) &&
             order.Notes.Contains("WERTGUTSCHEIN", StringComparison.OrdinalIgnoreCase));

        private async Task SendVoucherCustomerEmailAsync(Order order)
        {
            var toEmail = order.Email?.Trim();
            if (string.IsNullOrWhiteSpace(toEmail))
                return;

            var (nominalText, invoiceText, remarkText) = ParseVoucherNotes(order.Notes);
            var customerName = System.Net.WebUtility.HtmlEncode(order.Name ?? "");
            var nominalHtml = System.Net.WebUtility.HtmlEncode(nominalText ?? "Wertgutschein");
            var invoiceHtml = System.Net.WebUtility.HtmlEncode(invoiceText ?? "—");
            var remarkHtml = System.Net.WebUtility.HtmlEncode(remarkText ?? "—");
            var codesHtml = string.IsNullOrWhiteSpace(order.IssuedVoucherCodes)
                ? ""
                : $@"<p style='font-size:15px; color:#1b4332; margin-top:10px; padding:10px 12px; background:#d8f3dc; border-radius:8px;'>
        <b>Ihre Einlöse-Code(s):</b><br/>
        <span style='font-family:Consolas,monospace; font-weight:700;'>{System.Net.WebUtility.HtmlEncode(order.IssuedVoucherCodes)}</span><br/>
        <span style='font-size:13px; color:#333;'>Bitte bei einer späteren Bestellung unter <b>Gutschein-Code</b> (Online-Zahlung) eingeben.</span>
      </p>";

            var body = $@"
<html>
  <body style='font-family: Arial, sans-serif; background-color:#f9f9f9; padding:20px;'>
    <div style='max-width:620px; margin:auto; background:#fff; border-radius:8px; padding:20px; box-shadow:0 2px 8px rgba(0,0,0,0.1);'>
      <h2 style='color:#f66f00; text-align:center; border-bottom:2px solid #f66f00; padding-bottom:10px;'>
        Ihr Wertgutschein (PDF)
      </h2>
      <p style='font-size:15px; color:#333;'>Guten Tag <b>{customerName}</b>,</p>
      <p style='font-size:15px; color:#333;'>
        vielen Dank für Ihre Zahlung. Ihr Wertgutschein ist im Anhang als PDF beigefügt.
      </p>
      <p style='font-size:15px; color:#333; margin-top:10px;'>
        <b>Bestellung:</b> #{order.Id}<br/>
        <b>Gutschein:</b> {nominalHtml}<br/>
        <b>Betrag bezahlt:</b> CHF {order.FinalTotalNumber:F2}
      </p>
      {codesHtml}
      <p style='font-size:14px; color:#555; margin-top:8px;'>
        <b>Rechnungsadresse:</b> {invoiceHtml}<br/>
        <b>Bemerkung:</b> {remarkHtml}
      </p>
      <p style='font-size:13px; color:#777; margin-top:14px;'>
        Falls Sie diese E-Mail auf dem Handy öffnen, laden Sie bitte das PDF im Anhang herunter.
      </p>
      <div style='margin-top:20px;'>
        <p style='font-size:14px; color:#555;'>Freundliche Grüsse,</p>
        <p style='font-weight:bold; font-size:15px; color:#264653;'>Ihr Team von Pizza Wangen 🍕</p>
      </div>
    </div>
  </body>
</html>";

            var pdfBytes = OrderSummaryPdf.Generate(order, 0);
            var mailRequest = new MailRequest
            {
                ToEmail = toEmail,
                Subject = $"Ihr Wertgutschein #{order.Id} (PDF)",
                Body = body,
                Attachments = new List<FileAttachment>
                {
                    new FileAttachment
                    {
                        File = pdfBytes,
                        Name = $"Wertgutschein-{order.Id}.pdf",
                        ContentType = "application/pdf"
                    }
                }
            };

            await _mailService.SendEmailAsync(mailRequest, default);
        }

        private static (string? nominal, string? invoice, string? remark) ParseVoucherNotes(string? notes)
        {
            if (string.IsNullOrWhiteSpace(notes))
                return (null, null, null);

            string? nominal = null;
            string? invoice = null;
            string? remark = null;

            var parts = notes.Split('|', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            foreach (var p in parts)
            {
                if (p.StartsWith("Nominal", StringComparison.OrdinalIgnoreCase))
                    nominal = p;
                else if (p.StartsWith("Rechnung:", StringComparison.OrdinalIgnoreCase))
                    invoice = p["Rechnung:".Length..].Trim();
                else if (p.StartsWith("Bemerkung:", StringComparison.OrdinalIgnoreCase))
                    remark = p["Bemerkung:".Length..].Trim();
            }

            return (nominal, invoice, remark);
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

            if (!string.IsNullOrEmpty(order.Notes) && order.Notes.Contains("WERTGUTSCHEIN", StringComparison.OrdinalIgnoreCase))
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
