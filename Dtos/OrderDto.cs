using Newtonsoft.Json;

namespace WangenPizza.Dtos
{
    public class OrderDto
    {
        public int Id { get; set; }
        public string? UserId { get; set; }
        public string? Salute { get; set; }
        public string? Name { get; set; }
        public string? Mobile { get; set; }
        public string? Email { get; set; }
        public string? Street { get; set; }
        /// <summary>House number from checkout (combined into <see cref="Street"/> on the order).</summary>
        [JsonProperty("hausnummer")]
        public string? Hausnummer { get; set; }
        public string? PostBox { get; set; }
        public string? City { get; set; }
        [JsonProperty("discountCode")]
        public string? DiscountCode { get; set; }
        /// <summary>Wertgutschein (Restbetrag in DiscountCode.Value als CHF, nicht Prozent).</summary>
        [JsonProperty("gutscheinCode")]
        public string? GutscheinCode { get; set; }
        public DateTime? DateAdded { get; set; }
        public int PaymentWay { get; set; }
        public DateTime DeliveryDate { get; set; } 
        public string? DeliveryTime { get; set; }
        public string? Pickup_type { get; set; }

        public string? Notes { get; set; }
        public bool IsPrinted { get; set; }
        public bool IsPaymentSucceeded { get; set; }




    }
}
