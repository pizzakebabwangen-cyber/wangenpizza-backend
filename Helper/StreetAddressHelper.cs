namespace WangenPizza.Helper
{
    public static class StreetAddressHelper
    {
        public static string CombineStreetAndHausnummer(string? street, string? hausnummer)
        {
            var s = (street ?? "").Trim();
            var h = (hausnummer ?? "").Trim();
            if (string.IsNullOrEmpty(h)) return s;
            if (string.IsNullOrEmpty(s)) return h;
            return $"{s} {h}".Trim();
        }
    }
}
