namespace WangenPizza.Models
{
    public class DiscountCode
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public double Value { get; set; }
        public DateTime ExpiryDate { get; set; }

        /// <summary>Wenn true: <see cref="Value"/> = verbleibendes CHF-Guthaben (Wertgutschein), kein Prozent-Rabatt.</summary>
        public bool IsWertgutschein { get; set; }

        public bool IsActive { get; set; }
        public DateTime CreatedAtUtc { get; set; }
        public string? Note { get; set; }
        public decimal OriginalValueChf { get; set; }
    }
}
