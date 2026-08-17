
using WangenPizza.Models;

namespace WangenPizza.Dtos
{
    public class OrderDetailsDto
    {
        public int Id { get; set; }
        public string? UserId { get; set; }
        public string? Name { get; set; }
        public string? Street { get; set; }
        public string? PostBox { get; set; }
        public string? City { get; set; }
        public string? Phone { get; set; }
        public string? Email { get; set; }
        public string? DiscountCode { get; set; }
        public decimal? TotalNumber { get; set; }
        public decimal? DiscountValue { get; set; }
        public decimal? FinalTotalNumber { get; set; }
        public string? Pickup_type { get; set; }

        public DateTime DateAdded { get; set; }
        public int? PaymentWay { get; set; }
        public DateTime DeliveryDate { get; set; }
        public string? DeliveryTime { get; set; }
        public string? Notes { get; set; }
        public List<OrderItem>? Items { get; set; }


		///
		public string? CompanyName{ get; set; }
		public string? CompanyStreet { get; set; }
		public string? CompanyPostbox { get; set; }
		public string? CompanyCity { get; set; }
		public string? CompanyPhone1 { get; set; }
		public string? CompanyPhone2 { get; set; }
		public string? CompanyEmail { get; set; }

		///


	}
}
