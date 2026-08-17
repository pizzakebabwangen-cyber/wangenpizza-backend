using System.Text.RegularExpressions;
using QRCoder;

namespace WangenPizza.Helper;

public static class KurierQrHelper
{
    public static bool IsDeliveryPickupType(string? pickupType)
    {
        if (string.IsNullOrWhiteSpace(pickupType)) return false;
        var p = pickupType.Trim();
        return p.Equals("delivery", StringComparison.OrdinalIgnoreCase)
               || p.Equals("lieferung", StringComparison.OrdinalIgnoreCase);
    }

    public static byte[] MapsRoutePng(string destinationAddressLine, int pixelsPerModule = 6)
    {
        var mapsUrl = "https://www.google.com/maps/dir/?api=1&destination="
                       + Uri.EscapeDataString(destinationAddressLine.Trim());
        return PngBytes(mapsUrl, pixelsPerModule);
    }

    public static string MapsRouteUrl(string destinationAddressLine) =>
        "https://www.google.com/maps/dir/?api=1&destination="
        + Uri.EscapeDataString(destinationAddressLine.Trim());

    private static byte[] PngBytes(string payload, int pixelsPerModule)
    {
        using var gen = new QRCodeGenerator();
        using var data = gen.CreateQrCode(payload, QRCodeGenerator.ECCLevel.Q);
        var png = new PngByteQRCode(data);
        return png.GetGraphic(pixelsPerModule);
    }

    /// <summary>Valid deliveryTime: ASAP, clock HH:mm, or ca. NN Minuten / ca. 1 Stunde.</summary>
    public static bool IsAllowedCustomerDeliveryTime(string? t)
    {
        if (string.IsNullOrWhiteSpace(t)) return false;
        var s = t.Trim();
        if (s.Equals("so schnell wie möglich", StringComparison.OrdinalIgnoreCase)) return true;
        if (Regex.IsMatch(s, @"^\d{1,2}:\d{2}$")) return true;
        if (Regex.IsMatch(s, @"^ca\.\s*\d+\s*Minuten$", RegexOptions.IgnoreCase)) return true;
        if (s.Equals("ca. 1 Stunde", StringComparison.OrdinalIgnoreCase)) return true;
        return false;
    }

    public static string NormalizeDeliveryTimeOrAsap(string? t)
    {
        if (string.IsNullOrWhiteSpace(t)) return "so schnell wie möglich";
        var s = t.Trim();
        return IsAllowedCustomerDeliveryTime(s) ? s : "so schnell wie möglich";
    }
}
