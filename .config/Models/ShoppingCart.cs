namespace WangenPizza.Models
{
    public class ShoppingCart
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public string? Pickup_type { get; set; }

        public ICollection<CartItem> Items { get; set; } = new List<CartItem>();
        public ICollection<OrderItem>? OrderItems { get; set; } = new List<OrderItem>();
		public DateTime LastOperationTimestamp { get; set; } // Add this property

	}
}
