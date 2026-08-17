using WangenPizza.Dtos;
using WangenPizza.Models;

namespace WangenPizza.Helper.Response
{
    public class GetCartResponse
    {
        public string Code { get; set; }
        public string Status { get; set; }
        public string Message { get; set; }
        public CartWithExtensionsDTO Data { get; set; }

    }
}
