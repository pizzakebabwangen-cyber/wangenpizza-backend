using System.Linq;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using WangenPizza.Interfaces;
using WangenPizza.Models;

namespace WangenPizza.Helper
{
	/// <summary>E-Mail an Restaurant bei neuer Bestellung (Bar sofort nach CreateOrder, Online nach Zahlung in Payment/Success).</summary>
	public static class RestaurantOrderNotificationHelper
	{
		public static string? GetRestaurantNotifyEmail(IConfiguration configuration)
		{
			var section = configuration.GetSection("MailSettings");
			var adminNotify = section["OrderAdminNotifyEmail"];
			var email = string.IsNullOrWhiteSpace(adminNotify) ? section["Mail"] : adminNotify.Trim();
			return string.IsNullOrWhiteSpace(email) ? null : email;
		}

		public static async Task TrySendRestaurantOrderEmailAsync(
			Order order,
			IConfiguration configuration,
			IMailService mailService,
			ILogger? logger,
			bool isCashBarzahlungNewOrder)
		{
			try
			{
				var to = GetRestaurantNotifyEmail(configuration);
				if (string.IsNullOrWhiteSpace(to))
				{
					logger?.LogWarning("Restaurant mail: MailSettings Mail / OrderAdminNotifyEmail fehlt.");
					return;
				}

				var orderItemsHtml = BuildOrderItemsTableRows(order);
				var deliveryTimeLabel = order.Pickup_type == "delivery" ? "Liefertermin" : "Abholtermin";
				var deliveryPriceLabel = order.Pickup_type == "delivery" ? "Lieferpreis" : "Abholpreis";

				string subject;
				string headline;
				string introHtml;
				string totalParagraph;

				if (isCashBarzahlungNewOrder)
				{
					subject = $"Neue Bestellung #{order.Id} (Barzahlung)";
					headline = "Neue Barzahlungs-Bestellung";
					introHtml = @"<p style=""font-size:15px; color:#333;"">
        Hallo <b>Admin Pizzawangen.ch</b>,<br><br>
        Der Kunde hat die Bestellung im Shop abgeschlossen (<b>Barzahlung</b>). Die Bestellung liegt im System; der Gast bestätigt die AGB auf der nächsten Seite — bitte Bestellung im Admin/POS prüfen.
      </p>";
					totalParagraph = $@"<p style=""margin-top:15px; font-size:15px; color:#333;"">
        <b>Total (Bar bei Lieferung/Abholung):</b>
        <span style=""color:#2a9d8f; font-size:16px;"">CHF {order.FinalTotalNumber:F2}</span>
      </p>";
				}
				else
				{
					subject = "Bestellung erfolgreich gesendet";
					headline = "Neue Bestellung erfolgreich gesendet";
					introHtml = $@"<p style=""font-size:15px; color:#333;"">
        Hallo <b>Admin Pizzawangen.ch</b>,<br><br>
        Hiermit möchte ich Sie darüber informieren, dass folgende Bestellung 
        über Ihr System mit <span style=""color:#e63946; font-weight:bold""> {deliveryPriceLabel}</span> gesendet wurde (online bezahlt).
      </p>";
					totalParagraph = $@"<p style=""margin-top:15px; font-size:15px; color:#333;"">
        <b>Total:</b>
        <span style=""color:#2a9d8f; font-size:16px;"">CHF {order.FinalTotalNumber:F2}</span>
        (Betrag online bezahlt)
      </p>";
				}

				var body = $@"
<html>
  <body style='font-family: Arial, sans-serif; background-color:#f9f9f9; padding:20px;'>
    <div style='max-width:600px; margin:auto; background:#fff; border-radius:8px; padding:20px; box-shadow:0 2px 8px rgba(0,0,0,0.1);'>
      <h2 style='color:#f66f00; text-align:center; border-bottom:2px solid #f66f00; padding-bottom:10px;'>{headline}</h2>
      {introHtml}
      <table border='1' style='border-collapse: collapse; width: 100%; margin-top:15px; font-size:14px;'>
        <thead style='background-color:#f1f1f1;'>
          <tr>
            <th style='padding:8px; text-align:center;'>Produkt</th>
            <th style='padding:8px; text-align:center;'>Menge</th>
            <th style='padding:8px; text-align:center;'>Preis (pro Stück)</th>
            <th style='padding:8px; text-align:center;'>Total</th>
          </tr>
        </thead>
        <tbody>{orderItemsHtml}</tbody>
      </table>
      {totalParagraph}
      <p style='margin-top:8px; font-size:15px;'>
        <b style='color:#e63946;'>{deliveryTimeLabel}:</b> {System.Net.WebUtility.HtmlEncode(order.DeliveryTime ?? "")}
      </p>
      <div style='margin-top:20px; font-size:14px; color:#555;'>
        <p><b>Bestellung Nr.</b> {order.Id}</p>
        <p style='margin-top:10px; font-weight:bold; color:#264653;'>{System.Net.WebUtility.HtmlEncode(order.Name ?? "")}</p>
        <p style='color:#333;'>{System.Net.WebUtility.HtmlEncode(order.Street ?? "")}, {System.Net.WebUtility.HtmlEncode(order.PostBox ?? "")}, {System.Net.WebUtility.HtmlEncode(order.City ?? "")}</p>
        <p style='color:#333;'>Tel.: {System.Net.WebUtility.HtmlEncode(order.Mobile ?? "")} · E-Mail: {System.Net.WebUtility.HtmlEncode(order.Email ?? "")}</p>
      </div>
    </div>
  </body>
</html>";

				await mailService.SendEmailAsync(new MailRequest
				{
					ToEmail = to,
					Subject = subject,
					Body = body
				}, default);
			}
			catch (Exception ex)
			{
				logger?.LogError(ex, "Restaurant-Benachrichtigungs-Mail für Order {OrderId} fehlgeschlagen.", order.Id);
			}
		}

		private static string BuildOrderItemsTableRows(Order order)
		{
			if (order.OrderItems == null || !order.OrderItems.Any())
				return "<tr><td colspan='4' style='text-align:center;'>—</td></tr>";

			var sb = new System.Text.StringBuilder();
			foreach (var item in order.OrderItems)
			{
				var productName = System.Net.WebUtility.HtmlEncode(item.Product?.Name ?? "Artikel");
				var unitPrice = item.Product?.Price ?? 0m;
				sb.Append($@"
        <tr>
            <td style='text-align: center;'>{productName}</td>
            <td style='text-align: center;'>{item.Quantity}</td>
            <td style='text-align: center;'>CHF {unitPrice:F2}</td>
            <td style='text-align: center;'>CHF {(item.Quantity * unitPrice):F2}</td>
        </tr>");
			}
			return sb.ToString();
		}
	}
}
