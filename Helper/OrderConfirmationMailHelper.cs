using System.Linq;
using System.Globalization;
using System.Text.RegularExpressions;
using WangenPizza.Models;

namespace WangenPizza.Helper
{
	public static class OrderConfirmationMailHelper
	{
		/// <summary>Klartext ohne „so schnell wie möglich“ (E-Mails / Verwechslung mit Minuten/CHF).</summary>
		public const string DeliveryTimeAsapDisplay = "so schnell wie möglich";
		private const string ReviewUrl = "https://www.google.com/maps/place/Wangen+Pizza+kebab/@47.1911786,8.8948139,17z/data=!3m1!5s0x479ac9f21b03359b:0xc5b10c44ee722e4f!4m8!3m7!1s0x479ac9f21b004d51:0xa0638e9a28a0dded!8m2!3d47.1911786!4d8.8948139!9m1!1b1!16s%2Fg%2F11g890yt6m?hl=de&entry=ttu";

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

		public static string FormatOrderWishDisplay(Order order)
		{
			return FormatOrderWishDisplay(order.DeliveryDate, order.DeliveryTime);
		}

		public static string FormatOrderWishDisplay(DateTime deliveryDate, string? deliveryTime)
		{
			var timeDisplay = FormatDeliveryTimeDisplay(deliveryTime);
			if (IsAsapOrEmptyDeliveryWish(deliveryTime))
				return timeDisplay;

			if (deliveryDate.Year < 2000)
				return timeDisplay;

			var culture = new CultureInfo("de-CH");
			var dayName = culture.DateTimeFormat.GetDayName(deliveryDate.DayOfWeek);
			return $"{dayName}, {deliveryDate:dd.MM.yyyy} um {timeDisplay}";
		}

		public static string FormatOrderDateOnlyDisplay(Order order)
		{
			if (order.DeliveryDate.Year < 2000)
				return "—";

			var culture = new CultureInfo("de-CH");
			var dayName = culture.DateTimeFormat.GetDayName(order.DeliveryDate.DayOfWeek);
			return $"{dayName}, {order.DeliveryDate:dd.MM.yyyy}";
		}

		public static string PickupTypeDisplay(string? pickupType)
		{
			if (string.Equals(pickupType, "delivery", StringComparison.OrdinalIgnoreCase))
				return "Lieferung";
			if (string.Equals(pickupType, "Pickup", StringComparison.OrdinalIgnoreCase))
				return "Abholung";
			if (string.Equals(pickupType, "voucher", StringComparison.OrdinalIgnoreCase))
				return "Wertgutschein";
			return "—";
		}

		public static string PaymentWayDisplay(int paymentWay) =>
			paymentWay == 1 ? "Barzahlung" : "Online-Zahlung";

		public static string WishTerminLabel(string? pickupType)
		{
			if (string.Equals(pickupType, "delivery", StringComparison.OrdinalIgnoreCase))
				return "Gewünschter Liefertermin";
			if (string.Equals(pickupType, "Pickup", StringComparison.OrdinalIgnoreCase))
				return "Gewünschter Abholtermin";
			return "Gewünschter Termin";
		}

		private static string GreetingForZurichNow()
		{
			var tz = TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time");
			var now = TimeZoneInfo.ConvertTime(DateTime.UtcNow, tz);
			if (now.Hour < 11) return "Guten Morgen";
			if (now.Hour < 18) return "Guten Tag";
			return "Guten Abend";
		}

		private static string Html(string? value) =>
			System.Net.WebUtility.HtmlEncode(value ?? "");

		private static string BuildGreetingLine(Order order)
		{
			var greeting = GreetingForZurichNow();
			var salute = (order.Salute ?? "").Trim();
			var name = (order.Name ?? "").Trim();

			if (!string.IsNullOrWhiteSpace(salute) && !string.IsNullOrWhiteSpace(name))
				return $"{greeting}, {salute} {name}";
			if (!string.IsNullOrWhiteSpace(name))
				return $"{greeting}, {name}";
			return greeting;
		}

		private static string BuildCustomerRows(Order order)
		{
			var rows = "";
			if (order.OrderItems == null) return rows;

			foreach (var item in order.OrderItems)
			{
				var name = Html(item.Product?.Name ?? "Artikel");
				rows += $@"
                  <tr>
                    <td style='padding:8px 0; border-bottom:1px solid #eee; font-weight:600;'>{item.Quantity}x {name}</td>
                    <td style='padding:8px 0; border-bottom:1px solid #eee; text-align:right;'>CHF {item.Subtotal:F2}</td>
                  </tr>";
			}

			return rows;
		}

		private static string BuildCustomerTotals(Order order)
		{
			var totals = $@"
                          <p style='font-size:14px; margin:14px 0 0; text-align:right;'>
                            <b>Zwischensumme:</b> CHF {order.TotalNumber:F2}
                          </p>";

			if (order.DiscountValue > 0)
			{
				totals += $@"
                          <p style='font-size:14px; margin:4px 0 0; text-align:right;'>
                            <b>Rabatt:</b> -CHF {order.DiscountValue:F2}
                          </p>";
			}

			if (order.GutscheinDeduction > 0)
			{
				totals += $@"
                          <p style='font-size:14px; margin:4px 0 0; text-align:right;'>
                            <b>Wertgutschein:</b> -CHF {order.GutscheinDeduction:F2}
                          </p>";
			}

			totals += $@"
                          <p style='font-size:15px; margin:6px 0 0; text-align:right;'>
                            <b>Gesamt:</b> CHF {order.FinalTotalNumber:F2}
                          </p>";

			return totals;
		}

		/// <summary>Nur bei ASAP/leer: Kunden-Mail zeigt Fertigstellungs-Minuten vom POS. Sonst nur Wunschzeit aus Bestellung (Uhrzeit / ca. Min.).</summary>
		public static bool IsAsapOrEmptyDeliveryWish(string? deliveryTime)
		{
			if (string.IsNullOrWhiteSpace(deliveryTime)) return true;
			return deliveryTime.Trim().Equals("so schnell wie möglich", StringComparison.OrdinalIgnoreCase);
		}

		/// <summary><paramref name="preparationMinutes"/> nur für Anzeige im POS/intern; in der Kunden-E-Mail bei Wunschzeit nicht mehr als zweiter Satz.</summary>
		public static string BuildCustomerOrderConfirmationEmailSubject(int orderId, Order order, int preparationMinutes)
		{
			var wishDisplay = FormatOrderWishDisplay(order);
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
			var greetingLine = BuildGreetingLine(order);
			var wishDisplay = FormatOrderWishDisplay(order);
			var prepPhrase = PhrasePreparationMinutes(preparationMinutes);
			var showOnlyWish = !IsAsapOrEmptyDeliveryWish(order.DeliveryTime);
			var orderRows = BuildCustomerRows(order);
			var totals = BuildCustomerTotals(order);
			var addressLine = $"{order.Street ?? ""}, {order.PostBox ?? ""} {order.City ?? ""}".Trim().Trim(',');
			var timeParagraph = showOnlyWish
				? $@"<p style='font-size:16px; color:#333; margin-top:0;'>
                    {Html(wishDisplay)}
                  </p>"
				: $@"<p style='font-size:16px; color:#333; margin-top:0;'>
                    {Html(wishDisplay)}
                  </p>
                  <p style='font-size:15px; color:#333; margin-top:14px;'>
                    <b>Voraussichtliche Fertigstellung:</b> {prepPhrase}
                  </p>";
			return $@"
            <html>
              <body style='font-family: Arial, sans-serif; background-color:#f4f4f4; padding:20px; color:#222;'>
                <div style='max-width:680px; margin:auto; background:#fff; padding:28px;'>
                  <h2 style='margin:0 0 12px; color:#111;'>Ihre Bestellung bei Wangen Pizza Kebab</h2>
                  <p style='font-size:15px; margin:0 0 6px;'><b>{Html(greetingLine)}</b></p>
                  <p style='font-size:15px; line-height:1.45; margin:0 0 18px;'>
                    Vielen Dank für Ihre Bestellung. Ihre Bestellung ist bei uns eingegangen und wird frisch für Sie vorbereitet.
                  </p>

                  <div style='display:block; border-top:1px solid #ddd; border-bottom:1px solid #ddd; padding:16px 0; margin:18px 0;'>
                    <table style='width:100%; border-collapse:collapse;'>
                      <tr>
                        <td style='vertical-align:top; width:48%; padding-right:18px;'>
                          <p style='font-size:15px; margin:0 0 8px;'><b>{WishTerminLabel(order.Pickup_type)}</b></p>
                          {timeParagraph}
                          <p style='font-size:14px; line-height:1.4; margin:16px 0 0;'>
                            {Html(addressLine)}
                          </p>
                        </td>
                        <td style='vertical-align:top; width:52%;'>
                          <p style='font-size:15px; margin:0 0 8px;'><b>Bestellung</b><br/>Bestellnummer: #{order.Id}</p>
                          <table style='width:100%; border-collapse:collapse; font-size:14px;'>
                            {orderRows}
                          </table>
                          {totals}
                        </td>
                      </tr>
                    </table>
                  </div>

                  <p style='font-size:15px; margin:0 0 18px;'>
                    <b>Zahlungsmittel</b><br/>
                    Ihr gewähltes Zahlungsmittel: {PaymentWayDisplay(order.PaymentWay)}
                  </p>

                  <p style='font-size:14px; line-height:1.45;'>
                    Bei Rückfragen erreichen Sie uns gerne telefonisch unter 055 460 33 66.
                  </p>
                  <p style='font-size:15px; margin-top:18px;'>Guten Appetit!<br/>Ihr Team von Wangen Pizza Kebab</p>

                  <div style='margin-top:24px; padding-top:16px; border-top:1px solid #ddd;'>
                    <p style='font-size:14px; margin:0 0 8px;'>
                      Ihre Meinung ist uns wichtig. Bewerten Sie Ihre Bestellung und helfen Sie uns, unseren Service weiter zu verbessern.
                    </p>
                    <p style='font-size:24px; letter-spacing:4px; margin:8px 0;'>★★★★★</p>
                    <a href='{ReviewUrl}' style='display:inline-block; padding:10px 18px; border:2px solid #111; color:#111; text-decoration:none; font-weight:bold;'>
                      Jetzt Ihre Bestellung bewerten
                    </a>
                  </div>

                  <p style='font-size:11px; color:#777; line-height:1.45; margin-top:24px;'>
                    Dies ist eine automatisch generierte Bestätigungs-E-Mail. Antworten auf diese E-Mail werden nicht bearbeitet.
                    Sollten Sie Änderungen an Ihrer Bestellung wünschen, kontaktieren Sie bitte das Restaurant telefonisch.
                  </p>
                </div>
              </body>
            </html>";
		}

		/// <summary>Eine interne Mail ans Restaurant nach POS „Akzeptieren“ (Bestätigungsmail ging an den Kunden).</summary>
		public static string BuildPosAcknowledgeAdminNotifyHtml(Order order, int preparationMinutes)
		{
			bool isLieferung = KurierQrHelper.IsDeliveryPickupType(order.Pickup_type);
			string deliveryTimeLabel = isLieferung ? "Liefertermin" : "Abholtermin";
			var wishDisplay = FormatOrderWishDisplay(order);
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
