using Newtonsoft.Json;

namespace WangenPizza.Models
{
    public class Product
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string? Description { get; set; }
        public string PhotoName { get; set; }

        [JsonProperty("price")]
        public decimal Price { get; set; }

        [JsonProperty("pickup_Price")]
        public decimal Pickup_Price { get; set; }

        /** Reihenfolge innerhalb der Unterkategorie im Shop-Menü (kleiner = weiter oben). */
        [JsonProperty("displayOrder")]
        public int DisplayOrder { get; set; }

        public bool AddToHome { get; set; }
        public string? ProductType { get; set; }

        public int? SubCategoryId { get; set; }
        public SubCategory? SubCategory { get; set; }
        public List<Extension>? Extensions { get; set; } // Updated property name

    }
}
