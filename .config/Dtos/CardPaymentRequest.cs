namespace WangenPizza.Dtos
{
    public class PaymentRequest
    {
        public string CardNumber { get; set; }
        public int ExpMonth { get; set; }
        public int ExpYear { get; set; }
        public string Cvc { get; set; }
        public int Amount { get; set; } // Amount in cents
        public string Currency { get; set; } // Currency code (e.g., "usd")
        public string PaymentMethodId { get; set; } // Optional: Payment method ID for attaching existing payment methods
    }
}

