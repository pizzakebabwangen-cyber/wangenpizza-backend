using Microsoft.AspNetCore.Mvc;
using WangenPizza.Helper.Response;
using WangenPizza.Interfaces;

namespace WangenPizza.Api_s.Controllers
{
    [Route("api/Restaurant")]
    [ApiController]
    public class RestaurantApiController : ControllerBase
    {
        private readonly IRestaurantService restaurantService;

        public RestaurantApiController(IRestaurantService restaurantService)
        {
            this.restaurantService = restaurantService;
        }

        [HttpGet("GetRestaurantPage")]
        public async Task<IActionResult> GetRestaurantPage()
        {
            try
            {
                var data = await restaurantService.GetPublicPageAsync();
                return Ok(new RestaurantPageResponse
                {
                    Code = "200",
                    Status = "Success",
                    Message = "Restaurant page data returned successfully.",
                    Data = data,
                });
            }
            catch (Exception)
            {
                return StatusCode(400, new CustomResponse { Code = "400", Message = "Error" });
            }
        }
    }
}
