using WangenPizza.Models;

namespace WangenPizza.Helper.Response
{
    public class SubCategorysResponse
    {
        public string Code { get; set; }
        public string Status { get; set; }
        public string Message { get; set; }
        public IEnumerable <SubCategory> Data { get; set; }

    }
}
