namespace WangenPizza.Models
{
    public class Offer
    {
        public int Id { get; set; }

        public string Name { get; set; }
        public string? Description1 { get; set; }
        public string? Description2 { get; set; }
        public string? Description3 { get; set; }
        public string? OfferNr { get; set; }
        public decimal Price { get; set; }
        public string PhotoName { get; set; }
    }
}
