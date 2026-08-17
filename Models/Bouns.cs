namespace WangenPizza.Models
{
    public class TodayBonus
    {
        public int Id { get; set; }

        public string Name { get; set; }
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public string PhotoName { get; set; }
    }
}
