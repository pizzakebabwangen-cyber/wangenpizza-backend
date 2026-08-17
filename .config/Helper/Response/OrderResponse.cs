using WangenPizza.Models;

namespace WangenPizza.Helper.Response
{
    public class OrderResponse
    {
        public string Code { get; set; }
        public string Status { get; set; }
        public string Message { get; set; }
        public string PaymentPageUrl { get; set; }
        public Order  Data { get; set; }

    }
}
