namespace WangenPizza.Models
{
    public class OrderItem
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public Product? Product { get; set; }
        public int Quantity { get; set; }
        public decimal Subtotal { get; set; }
		public DateTime CreatedAt { get; set; } // Add this property
        public List<ExtensionOrderItem>? ExtensionOrderItem { get; set; } = new List<ExtensionOrderItem>();

    }
}
