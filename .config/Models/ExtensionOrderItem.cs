namespace WangenPizza.Models
{
    public class ExtensionOrderItem
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; }
        public int CategoryId { get; set; }
        public int ProductId { get; set; }
        public String VisitorId { get; set; }

    }
}
