using WangenPizza.Models;

namespace WangenPizza.Dtos
{
    public class AddToCartDTO
    {
        public List<CartItemDto> Items { get; set; }
        public string? Pickup_type  { get; set; }

        public bool ReplaceExistingItems { get; set; }
    }
}
