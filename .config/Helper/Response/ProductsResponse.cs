using WangenPizza.Models;

namespace WangenPizza.Helper.Response
{
    public class ProductsResponse
    {
        public string Code { get; set; }
        public string Status { get; set; }
        public string Message { get; set; }
        public IEnumerable <Product> Data { get; set; }

    }
}
