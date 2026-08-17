namespace WangenPizza.Models
{
    public class Category
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string PhotoName { get; set; }
        public List<SubCategory>? SubCategory { get; set; }

    }
}
