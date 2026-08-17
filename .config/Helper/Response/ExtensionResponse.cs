

using WangenPizza.Models;

namespace WangenPizza.Helper.Response
{
    public class ExtensionResponse
    {
        public string Code { get; set; }
        public string Status { get; set; }
        public string Message { get; set; }
        public IEnumerable<Extension> Data { get; set; }
    }
}
