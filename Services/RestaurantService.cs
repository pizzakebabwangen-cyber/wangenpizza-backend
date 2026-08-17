using Microsoft.EntityFrameworkCore;
using WangenPizza.Context;
using WangenPizza.Dtos;
using WangenPizza.Interfaces;
using WangenPizza.Models;

namespace WangenPizza.Services
{
    public class RestaurantService : IRestaurantService
    {
        private readonly ApplicationDbContext context;

        public RestaurantService(ApplicationDbContext context)
        {
            this.context = context;
        }

        public async Task<RestaurantSettings> GetSettingsAsync()
        {
            await EnsureSettingsAsync();
            return await context.RestaurantSettings.AsNoTracking().FirstAsync();
        }

        public async Task UpdateSettingsAsync(RestaurantSettings settings)
        {
            await EnsureSettingsAsync();
            var existing = await context.RestaurantSettings.FirstAsync();
            existing.Title = settings.Title;
            existing.Description = settings.Description;
            existing.VideoUrl = settings.VideoUrl;
            await context.SaveChangesAsync();
        }

        public async Task<IEnumerable<RestaurantImage>> GetImagesAsync()
        {
            return await context.RestaurantImages
                .AsNoTracking()
                .OrderByDescending(i => i.IsHero)
                .ThenBy(i => i.SortOrder)
                .ThenBy(i => i.Id)
                .ToListAsync();
        }

        public async Task<RestaurantImage?> GetImageByIdAsync(int id)
        {
            return await context.RestaurantImages.FirstOrDefaultAsync(i => i.Id == id);
        }

        public async Task<RestaurantImage> CreateImageAsync(RestaurantImage image)
        {
            if (image.SortOrder == 0)
            {
                var maxOrder = await context.RestaurantImages.MaxAsync(i => (int?)i.SortOrder) ?? 0;
                image.SortOrder = maxOrder + 1;
            }

            if (image.IsHero)
            {
                await ClearHeroFlagsAsync();
            }

            await context.RestaurantImages.AddAsync(image);
            await context.SaveChangesAsync();
            return image;
        }

        public void UpdateImage(RestaurantImage image)
        {
            if (image.IsHero)
            {
                var others = context.RestaurantImages.Where(i => i.Id != image.Id && i.IsHero);
                foreach (var row in others)
                {
                    row.IsHero = false;
                }
            }

            context.Entry(image).State = EntityState.Modified;
            context.SaveChanges();
        }

        public void DeleteImage(RestaurantImage image)
        {
            context.Entry(image).State = EntityState.Deleted;
            context.SaveChanges();
        }

        public async Task<RestaurantPageDto> GetPublicPageAsync()
        {
            await EnsureSettingsAsync();
            var settings = await context.RestaurantSettings.AsNoTracking().FirstAsync();
            var images = await GetImagesAsync();

            return new RestaurantPageDto
            {
                Settings = new RestaurantSettingsDto
                {
                    Id = settings.Id,
                    Title = settings.Title,
                    Description = settings.Description,
                    VideoUrl = settings.VideoUrl,
                },
                Images = images.Select(i => new RestaurantImageDto
                {
                    Id = i.Id,
                    PhotoName = i.PhotoName,
                    Caption = i.Caption,
                    SortOrder = i.SortOrder,
                    IsHero = i.IsHero,
                }).ToList(),
            };
        }

        private async Task EnsureSettingsAsync()
        {
            if (await context.RestaurantSettings.AnyAsync())
            {
                return;
            }

            await context.RestaurantSettings.AddAsync(new RestaurantSettings
            {
                Title = "Unser Restaurant",
                Description =
                    "Bei uns geniessen Sie Pizza, Kebab und Döner in einem entspannten Ambiente in Wangen SZ.",
            });
            await context.SaveChangesAsync();
        }

        private async Task ClearHeroFlagsAsync()
        {
            var heroes = await context.RestaurantImages.Where(i => i.IsHero).ToListAsync();
            foreach (var row in heroes)
            {
                row.IsHero = false;
            }
        }
    }
}
