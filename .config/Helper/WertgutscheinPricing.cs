namespace WangenPizza.Helper
{
    /// <summary>Muss mit Front <c>wertgutscheinPricing.js</c> übereinstimmen.</summary>
    public static class WertgutscheinPricing
    {
        public const decimal BearbeitungNetChf = 5.0m;
        public const decimal MwstSatz = 0.081m;

        public static decimal PortoChf(int voucherQuantity)
        {
            var q = voucherQuantity < 1 ? 1 : voucherQuantity;
            return q >= 10 ? 1.7m : 1.2m;
        }

        public static (decimal FeeNet, decimal Mwst, decimal FeeBrutto, decimal Total) ComputeTotals(decimal faceValueChf, int voucherQuantity = 1)
        {
            var porto = PortoChf(voucherQuantity);
            var feeNet = BearbeitungNetChf + porto;
            var mwst = Math.Round(feeNet * MwstSatz * 100m) / 100m;
            var feeBrutto = feeNet + mwst;
            var total = faceValueChf + feeBrutto;
            return (feeNet, mwst, feeBrutto, total);
        }
    }
}
