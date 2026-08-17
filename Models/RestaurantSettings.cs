namespace WangenPizza.Models
{
    public class RestaurantSettings
    {
        public int Id { get; set; }
        public string Title { get; set; } = "Unser Restaurant";
        public string? Description { get; set; }
        public string? VideoUrl { get; set; }
    }
}
