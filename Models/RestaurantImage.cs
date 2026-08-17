namespace WangenPizza.Models
{
    public class RestaurantImage
    {
        public int Id { get; set; }
        public string PhotoName { get; set; } = "";
        public string? Caption { get; set; }
        public int SortOrder { get; set; }
        public bool IsHero { get; set; }
    }
}
