using WangenPizza.Models;

namespace WangenPizza.Dtos
{
    public class CategoryDto: Category
    {
        public IFormFile Photo { get; set; }
    }
}
