namespace WangenPizza.Dtos
{
    public class CheckoutDto
    {
        public int? orderId { get; set; }
        public string? clientSecret { get; set; }
        public decimal CartTotalNumber { get; set; }
        public decimal DiscountValue { get; set; }
        public decimal TotalAfterDiscount { get; set; }

    }
}
