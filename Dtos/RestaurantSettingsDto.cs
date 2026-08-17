using Microsoft.AspNetCore.Http;
using WangenPizza.Models;

namespace WangenPizza.Dtos
{
    public class RestaurantSettingsDto : RestaurantSettings
    {
        public IFormFile? Video { get; set; }
    }
}
