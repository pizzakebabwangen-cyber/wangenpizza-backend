using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using WangenPizza.Models;

namespace WangenPizza.Helper
{
    /// <summary>Kurze PDF-Bestellübersicht als E-Mail-Anhang (kein Ersatz für AGB/Steuerbelege).</summary>
    public static class OrderSummaryPdf
    {
        public static byte[] Generate(Order order, int preparationMinutes)
        {
            QuestPDF.Settings.License = LicenseType.Community;
            var createdAtZurich = ToZurichTime(order.DateAdded);

            return Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Margin(36);
                    page.DefaultTextStyle(x => x.FontSize(10));
                    page.Header().Column(c =>
                    {
                        c.Item().Text("Wangen Pizza").SemiBold().FontSize(14);
                        c.Item().Text($"Bestellbestätigung #{order.Id}").SemiBold().FontSize(16);
                    });

                    page.Content().PaddingTop(14).Column(column =>
                    {
                        column.Spacing(5);
                        column.Item().Text($"Erstellt am: {createdAtZurich:dd.MM.yyyy HH:mm}");
                        column.Item().Text($"Name: {order.Name ?? "—"}");
                        column.Item().Text($"Adresse: {order.Street ?? ""}, {order.PostBox ?? ""} {order.City ?? ""}");
                        column.Item().Text($"E-Mail: {order.Email ?? "—"}");
                        column.Item().Text($"Tel.: {order.Mobile ?? "—"}");
                        column.Item().Text($"Art: {OrderConfirmationMailHelper.PickupTypeDisplay(order.Pickup_type)}");
                        column.Item().Text($"Zahlungsmittel: {OrderConfirmationMailHelper.PaymentWayDisplay(order.PaymentWay)}");
                        if (!string.IsNullOrWhiteSpace(order.IssuedVoucherCodes))
                        {
                            column.Item().PaddingTop(4).Text($"Gutschein-Code(s): {order.IssuedVoucherCodes}").SemiBold();
                            column.Item().Text(
                                "Einlösung: auf der Kasse unter «Gutschein-Code» bei Online-Zahlung eingeben.");
                        }
                        column.Item().Text($"{OrderConfirmationMailHelper.WishTerminLabel(order.Pickup_type)}: {OrderConfirmationMailHelper.FormatOrderWishDisplay(order)}").SemiBold();
                        if (preparationMinutes > 0 && OrderConfirmationMailHelper.IsAsapOrEmptyDeliveryWish(order.DeliveryTime))
                        {
                            column.Item().Text(
                                $"Voraussichtliche Fertigstellung: {OrderConfirmationMailHelper.PhrasePreparationMinutes(preparationMinutes)}");
                        }

                        var items = order.OrderItems?.ToList() ?? new List<OrderItem>();
                        if (items.Count > 0)
                        {
                            column.Item().PaddingTop(10).Text("Positionen").SemiBold();
                            column.Item().Table(table =>
                            {
                                table.ColumnsDefinition(cols =>
                                {
                                    cols.RelativeColumn(4);
                                    cols.RelativeColumn(1);
                                    cols.RelativeColumn(1);
                                });
                                table.Header(h =>
                                {
                                    h.Cell().Element(HeadCell).Text("Artikel");
                                    h.Cell().Element(HeadCell).Text("Menge");
                                    h.Cell().Element(HeadCell).Text("CHF");
                                });
                                foreach (var oi in items)
                                {
                                    var name = oi.Product?.Name ?? $"Produkt #{oi.ProductId}";
                                    table.Cell().Element(BodyCell).Text(name);
                                    table.Cell().Element(BodyCell).Text(oi.Quantity.ToString());
                                    table.Cell().Element(BodyCell).Text(oi.Subtotal.ToString("F2"));
                                }
                            });
                        }

                        column.Item().PaddingTop(12).AlignRight().Column(sum =>
                        {
                            sum.Item().Text($"Zwischensumme: CHF {order.TotalNumber:F2}");
                            if (order.DiscountValue > 0)
                                sum.Item().Text($"Rabatt: CHF {order.DiscountValue:F2}");
                            sum.Item().Text($"Gesamt: CHF {order.FinalTotalNumber:F2}").SemiBold();
                        });

                        if (!string.IsNullOrWhiteSpace(order.Notes))
                            column.Item().PaddingTop(8).Text($"Notiz: {order.Notes}");
                    });
                });
            }).GeneratePdf();
        }

        private static DateTime ToZurichTime(DateTime dateTime)
        {
            var utc = dateTime.Kind == DateTimeKind.Utc
                ? dateTime
                : DateTime.SpecifyKind(dateTime, DateTimeKind.Utc);
            var zurichTimeZone = TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time");
            return TimeZoneInfo.ConvertTimeFromUtc(utc, zurichTimeZone);
        }

        private static IContainer HeadCell(IContainer c) =>
            c.DefaultTextStyle(x => x.SemiBold())
                .BorderBottom(1).BorderColor(Colors.Grey.Medium)
                .PaddingVertical(4);

        private static IContainer BodyCell(IContainer c) =>
            c.BorderBottom(1).BorderColor(Colors.Grey.Lighten2).PaddingVertical(4);
    }
}
