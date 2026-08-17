using WangenPizza.Models;

namespace WangenPizza.Dtos
{
    public class ExtensionDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string? Kind { get; set; }

        public decimal Price { get; set; }
        public int? CategoryId { get; set; } // Add this property if needed
        public Category? Category { get; set; }

    }
}
