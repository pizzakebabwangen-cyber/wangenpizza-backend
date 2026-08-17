namespace WangenPizza.Dtos
{
    public class RestaurantPageDto
    {
        public RestaurantSettingsDto Settings { get; set; } = new();
        public List<RestaurantImageDto> Images { get; set; } = new();
    }
}
