namespace WangenPizza.Models
{
    public class DiscountCode
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public double Value { get; set; }
        public DateTime ExpiryDate { get; set; }
      

    }
}
