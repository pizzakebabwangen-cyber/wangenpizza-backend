namespace WangenPizza.Models
{
    public class CartItem
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public Product? Product { get; set; }
        public int Quantity { get; set; }
        public List<Extension>? Extensions { get; set; } = new List<Extension>();

    }
}
