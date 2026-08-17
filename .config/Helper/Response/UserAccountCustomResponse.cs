using WangenPizza.Api_s.Models;

namespace WangenPizza.Helper.Response
{
    public class UserAccountCustomResponse
    {
        public string Code { get; set; }
        public string Status { get; set; }
        public string Message { get; set; }
        public UserModel Record { get; set; }

    }
}
