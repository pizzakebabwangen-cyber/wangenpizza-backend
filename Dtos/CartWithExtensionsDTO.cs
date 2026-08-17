using WangenPizza.Models;

namespace WangenPizza.Dtos
{
    public class CartWithExtensionsDTO
    {
        public ShoppingCart Cart { get; set; }
        public List<ExtensionOrderItem> Extensions { get; set; }
    }
}
