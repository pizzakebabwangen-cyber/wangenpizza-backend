using System.Linq;
using System.Text.RegularExpressions;
using WangenPizza.Models;

namespace WangenPizza.Helper
{
	public static class OrderConfirmationMailHelper
	{
		/// <summary>Klartext ohne „so schnell wie möglich“ (E-Mails / Verwechslung mit Minuten/CHF).</summary>
		public const string DeliveryTimeAsapDisplay = "Schnellstmöglich (ASAP)";

		public static string FormatDeliveryTimeDisplay(string? deliveryTime)
		{
			if (string.IsNullOrWhiteSpace(deliveryTime))
				return DeliveryTimeAsapDisplay;
			var s = deliveryTime.Trim();
			if (s.Equals("so schnell wie möglich", StringComparison.OrdinalIgnoreCase))
				return DeliveryTimeAsapDisplay;
			// Reine Zahl (z. B. "30") oder Minuten ohne "ca." — nie anzeigen (Verwechslung mit CHF-Betrag).
			if (Regex.IsMatch(s, @"^\d{1,3}$"))
				return DeliveryTimeAsapDisplay;
			if (Regex.IsMatch(s, @"^\d{1,2}:\d{2}$"))
			{
				var parts = s.Split(':');
				if (parts.Length == 2
					&& int.TryParse(parts[0], out var h) && h is >= 0 and <= 23
					&& int.TryParse(parts[1], out var m) && m is >= 0 and <= 59)
					return $"{s} Uhr";
				return DeliveryTimeAsapDisplay;
			}
			if (Regex.IsMatch(s, @"^ca\.\s*\d+\s*Minuten$", RegexOptions.IgnoreCase))
				return s;
			if (s.Equals("ca. 1 Stunde", StringComparison.OrdinalIgnoreCase))
				return s;
			return DeliveryTimeAsapDisplay;
		}

		public static string PhrasePreparationMinutes(int minutes) =>
			minutes == 60 ? "circa 1 Stunde" : $"circa {minutes} Minuten";

		/// <summary>Nur bei ASAP/leer: Kunden-Mail zeigt Fertigstellungs-Minuten vom POS. Sonst nur Wunschzeit aus Bestellung (Uhrzeit / ca. Min.).</summary>
		public static bool IsAsapOrEmptyDeliveryWish(string? deliveryTime)
		{
			if (string.IsNullOrWhiteSpace(deliveryTime)) return true;
			return deliveryTime.Trim().Equals("so schnell wie möglich", StringComparison.OrdinalIgnoreCase);
		}

		/// <summary><paramref name="preparationMinutes"/> nur für Anzeige im POS/intern; in der Kunden-E-Mail bei Wunschzeit nicht mehr als zweiter Satz.</summary>
		public static string BuildCustomerOrderConfirmationEmailSubject(int orderId, Order order, int preparationMinutes)
		{
			var wishDisplay = FormatDeliveryTimeDisplay(order.DeliveryTime);
			if (IsAsapOrEmptyDeliveryWish(order.DeliveryTime))
				return $"Bestellung #{orderId} – {PhrasePreparationMinutes(preparationMinutes)}";
			return $"Bestellung #{orderId} – {wishDisplay}";
		}

		/// <summary>„ca. 25 Minuten“ / „ca. 1 Stunde“ aus Bestellfeld — für POS-Vorauswahl.</summary>
		public static int? TryParseCustomerApproximateMinutes(string? deliveryTime)
		{
			if (string.IsNullOrWhiteSpace(deliveryTime)) return null;
			var s = deliveryTime.Trim();
			if (s.Equals("ca. 1 Stunde", StringComparison.OrdinalIgnoreCase)) return 60;
			var m = Regex.Match(s, @"^ca\.\s*(\d+)\s*Minuten$", RegexOptions.IgnoreCase);
			if (m.Success && int.TryParse(m.Groups[1].Value, out var min)) return min;
			return null;
		}

		/// <summary>Nächster erlaubter POS-Schritt (z. B. Kundenwunsch 40 → 45).</summary>
		public static int ClosestPreparationStep(int[] allowedSteps, int customerMinutes)
		{
			if (allowedSteps == null || allowedSteps.Length == 0) throw new ArgumentException(nameof(allowedSteps));
			return allowedSteps.OrderBy(x => Math.Abs(x - customerMinutes)).First();
		}

		/// <summary>Kunden-Mail nach POS „Akzeptieren“: bei Wunschzeit nur diese; bei ASAP nur Fertigstellung in Minuten (POS).</summary>
		public static string BuildCustomerOrderConfirmationHtml(Order order, int preparationMinutes)
		{
			var salute = string.IsNullOrWhiteSpace(order.Salute) ? "" : $"{order.Salute} ";
			var name = string.IsNullOrWhiteSpace(order.Name) ? "" : order.Name;
			var wishDisplay = FormatDeliveryTimeDisplay(order.DeliveryTime);
			var prepPhrase = PhrasePreparationMinutes(preparationMinutes);
			var showOnlyWish = !IsAsapOrEmptyDeliveryWish(order.DeliveryTime);
			var timeParagraph = showOnlyWish
				? $@"<p style='font-size:15px; color:#333; margin-top:14px;'>
                    <b>Ihre bei der Bestellung gewählte Wunschzeit:</b> {wishDisplay}
                  </p>"
				: $@"<p style='font-size:15px; color:#333; margin-top:14px;'>
                    <b>Voraussichtliche Fertigstellung:</b> {prepPhrase}
                  </p>";
			return $@"
            <html>
              <body style='font-family: Arial, sans-serif; background-color:#f9f9f9; padding:20px;'>
                <div style='max-width:600px; margin:auto; background:#fff; border-radius:8px; padding:20px; box-shadow:0 2px 8px rgba(0,0,0,0.1);'>

                  <h2 style='color:#f66f00; text-align:center; border-bottom:2px solid #f66f00; padding-bottom:10px;'>
                    Bestellung erfolgreich erstellt 🎉
                  </h2>

                  <p style='font-size:15px; color:#333;'>
                    Guten Tag <b>{salute}{name}</b>,
                  </p>

                  <p style='font-size:15px; color:#333;'>
                    Vielen Dank für Ihre Bestellung. Wir haben Ihre Bestellung <strong>angenommen</strong>.
                  </p>

                  {timeParagraph}

                  <p style='font-size:12px; color:#888; margin-top:14px;'>(CHF-Beträge auf der Rechnung sind Preise in Franken, keine Minuten.)</p>

                  <div style='margin-top:20px;'>
                    <p style='font-size:14px; color:#555;'>Freundliche Grüsse,</p>
                    <p style='font-weight:bold; font-size:15px; color:#264653;'>Ihr Team von Pizza Wangen 🍕</p>
                  </div>
                </div>
              </body>
            </html>";
		}

		/// <summary>Eine interne Mail ans Restaurant nach POS „Akzeptieren“ (Bestätigungsmail ging an den Kunden).</summary>
		public static string BuildPosAcknowledgeAdminNotifyHtml(Order order, int preparationMinutes)
		{
			bool isLieferung = KurierQrHelper.IsDeliveryPickupType(order.Pickup_type);
			string deliveryTimeLabel = isLieferung ? "Liefertermin" : "Abholtermin";
			var wishDisplay = FormatDeliveryTimeDisplay(order.DeliveryTime);
			var prepPhrase = PhrasePreparationMinutes(preparationMinutes);
			var timeLine = IsAsapOrEmptyDeliveryWish(order.DeliveryTime)
				? $"Verbindlich bestätigt: <b>{prepPhrase}</b> (vom POS)"
				: $"Kundenwunsch: <b>{wishDisplay}</b> — Bestätigungsmail ohne zweite Fertigstellungszeile gesendet.";

			var rows = "";
			if (order.OrderItems != null)
			{
				foreach (var item in order.OrderItems)
				{
					var pname = item.Product?.Name ?? "Artikel";
					var price = item.Product?.Price ?? 0m;
					rows += $@"
        <tr>
          <td style='padding:8px; text-align:center;'>{pname}</td>
          <td style='padding:8px; text-align:center;'>{item.Quantity}</td>
          <td style='padding:8px; text-align:center;'>CHF {price:F2}</td>
          <td style='padding:8px; text-align:center;'>CHF {(item.Quantity * price):F2}</td>
        </tr>";
				}
			}

			return $@"
<html>
  <body style='font-family: Arial, sans-serif; background-color:#f9f9f9; padding:20px;'>
    <div style='max-width:600px; margin:auto; background:#fff; border-radius:8px; padding:20px;'>
      <h2 style='color:#f66f00;'>POS: Bestellung #{order.Id} akzeptiert</h2>
      <p style='font-size:15px; color:#333;'>
        Die Kunden-Bestätigungsmail wurde gesendet an <b>{order.Email}</b>.
      </p>
      <p style='font-size:15px;'><b>{deliveryTimeLabel} / Zeit:</b> {timeLine}</p>
      <p style='font-size:14px;'><b>Typ:</b> {(isLieferung ? "Lieferung" : "Abholung")} · <b>Zahlung:</b> {(order.PaymentWay == 1 ? "Bar" : "Online")}</p>
      <table border='1' style='border-collapse:collapse; width:100%; margin-top:12px; font-size:14px;'>
        <thead style='background:#f1f1f1;'><tr><th>Produkt</th><th>Menge</th><th>Preis</th><th>Total</th></tr></thead>
        <tbody>{rows}</tbody>
      </table>
      <p style='margin-top:12px;'><b>Total:</b> CHF {order.FinalTotalNumber:F2}</p>
      <p style='margin-top:16px; font-size:14px; color:#333;'><b>{order.Name}</b><br/>{order.Street} {order.PostBox} {order.City}</p>
    </div>
  </body>
</html>";
		}
	}
}
