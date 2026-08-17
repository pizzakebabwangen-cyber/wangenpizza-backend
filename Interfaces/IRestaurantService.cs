using WangenPizza.Dtos;
using WangenPizza.Models;

namespace WangenPizza.Interfaces
{
    public interface IRestaurantService
    {
        Task<RestaurantSettings> GetSettingsAsync();
        Task UpdateSettingsAsync(RestaurantSettings settings);
        Task<IEnumerable<RestaurantImage>> GetImagesAsync();
        Task<RestaurantImage?> GetImageByIdAsync(int id);
        Task<RestaurantImage> CreateImageAsync(RestaurantImage image);
        void UpdateImage(RestaurantImage image);
        void DeleteImage(RestaurantImage image);
        Task<RestaurantPageDto> GetPublicPageAsync();
    }
}
