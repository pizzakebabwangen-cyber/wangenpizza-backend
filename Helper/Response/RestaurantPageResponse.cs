using WangenPizza.Dtos;

namespace WangenPizza.Helper.Response
{
    public class RestaurantPageResponse
    {
        public string Code { get; set; } = "200";
        public string Status { get; set; } = "Success";
        public string Message { get; set; } = "";
        public RestaurantPageDto Data { get; set; } = new();
    }
}
