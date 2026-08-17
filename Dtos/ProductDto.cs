using WangenPizza.Models;

namespace WangenPizza.Dtos
{
    public class ProductDto: Product
    {
        public IFormFile Photo { get; set; }
    }
}
