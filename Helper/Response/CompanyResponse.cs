
using WangenPizza.Models;

namespace WangenPizza.Helper.Response
{
    public class CompanyResponse
    {
        public string Code { get; set; }
        public string Status { get; set; }
        public string Message { get; set; }
        public CompanyData Data { get; set; }
    }
}
