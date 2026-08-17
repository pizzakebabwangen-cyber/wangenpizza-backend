
using WangenPizza.Dtos;
using WangenPizza.Models;

namespace WangenPizza.Helper.Response
{
    public class CheckoutResponse
    {
        public string Code { get; set; }
        public string Status { get; set; }
        public string Message { get; set; }
        public CheckoutDto Data { get; set; }
    }
}
