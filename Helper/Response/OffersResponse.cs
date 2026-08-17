using WangenPizza.Models;

namespace WangenPizza.Helper.Response
{
    public class OffersResponse
    {
        public string Code { get; set; }
        public string Status { get; set; }
        public string Message { get; set; }
        public IEnumerable <Offer> Data { get; set; }

    }
}
