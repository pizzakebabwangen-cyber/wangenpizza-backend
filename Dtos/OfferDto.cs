using WangenPizza.Models;

namespace WangenPizza.Dtos
{
    public class OfferDto: Offer
    {
        public IFormFile Photo { get; set; }

    }
}
