namespace WangenPizza.Models
{
    public class Extension
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string? Kind { get; set; }
        public decimal Price { get; set; }
        public int CategoryId { get; set; }
        public Category? Category { get; set; }

        /// <summary>Sort order (Reihenfolge) for extras in the menu.</summary>
        public int DisplayOrder { get; set; }

    }
}
