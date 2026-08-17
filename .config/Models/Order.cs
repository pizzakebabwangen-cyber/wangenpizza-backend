namespace WangenPizza.Models
{
    public class Order
    {
        public int Id { get; set; }
        public string? UserId { get; set; }
        public string? Salute { get; set; }
        public string? Name { get; set; }
        public string? Mobile { get; set; }
        public string? Email { get; set; }
        public string? Street { get; set; }
        public string? PostBox { get; set; }
        public string? City { get; set; }
        public decimal TotalNumber { get; set; }
        public decimal DiscountValue { get; set; }
        /// <summary>Abzug in CHF vom Wertgutschein (DiscountCode.Value = Restguthaben). Rabatt-Prozent bleibt in <see cref="DiscountValue"/>.</summary>
        public decimal GutscheinDeduction { get; set; }
        /// <summary>Name des eingelösten Gutscheins — nach erfolgreicher Zahlung wird Restguthaben in DiscountCode reduziert.</summary>
        public string? AppliedGutscheinCode { get; set; }
        /// <summary>Bei gekauften Wertgutscheinen: ausgestellte Einlöse-Codes (kommagetrennt).</summary>
        public string? IssuedVoucherCodes { get; set; }
        public decimal FinalTotalNumber { get; set; }
        public string? Pickup_type { get; set; }

        public DateTime DateAdded { get; set; }
        public int PaymentWay { get; set; }
        public DateTime DeliveryDate { get; set; }
        public string? DeliveryTime { get; set; }
        public string? Notes { get; set; }
        public bool IsPrinted { get; set; }
        /// <summary>POS: "Akzeptieren" ohne Druck — verhindert doppelte Aktion.</summary>
        public bool PosAcknowledged { get; set; }
        /// <summary>Einmalige Lieferzeit-E-Mail: gespeicherte Minuten (15,20,…) nach erstem Versand.</summary>
        public int? PreparationMinutesEmailed { get; set; }
        /// <summary>Order tracking status: New, Preparing, OutForDelivery, Delivered</summary>
        public string? OrderStatus { get; set; } = "New";
        /// <summary>Timestamp when order status was last updated</summary>
        public DateTime? StatusUpdatedAt { get; set; }
        public bool Verified { get; set; }
        public long? TransactionId { get; set; }

        public bool IsPaymentSucceeded { get; set; } = false;
        public ICollection<OrderItem>? OrderItems { get; set; } = new List<OrderItem>();
        public ICollection<CartItem> Items { get; set; } = new List<CartItem>();

        public Order()
        {
            DateAdded = DateTime.UtcNow;
            DeliveryDate = DateTime.Now;
        }

    }
}
