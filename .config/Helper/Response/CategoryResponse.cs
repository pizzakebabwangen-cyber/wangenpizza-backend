using WangenPizza.Models;

namespace WangenPizza.Helper.Response
{
    public class CategoryResponse
    {
        public string Code { get; set; }
        public string Status { get; set; }
        public string Message { get; set; }
        public IEnumerable <Category> Data { get; set; }

    }
}
