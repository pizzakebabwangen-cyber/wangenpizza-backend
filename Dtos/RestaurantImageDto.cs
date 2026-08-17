using WangenPizza.Models;

namespace WangenPizza.Dtos
{
    public class RestaurantImageDto : RestaurantImage
    {
        public IFormFile? Photo { get; set; }
    }
}
