using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WangenPizza.Helper.Response;
using WangenPizza.Interfaces;

namespace WangenPizza.Api_s.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DeliveryController : ControllerBase
    {
        #region Ctor

        private readonly IMapper mapper;
        private readonly IDeliveryService deliveryService;

        public DeliveryController(IMapper mapper, IDeliveryService deliveryService)
        {
            this.mapper = mapper;
            this.deliveryService = deliveryService;
        }
        #endregion

        #region Get All Categorys 
        [HttpGet("GetAllDelivery")]
        public async Task<IActionResult> GetAllDelivery()
        {
           try
            {
                var data = await deliveryService.Get();
                if (data != null)
                {
                    // Explizit camelCase — ohne globalen ContractResolver (der u. a. Admin/Serialization stören kann).
                    var rows = data.Select(d => new
                    {
                        id = d.Id,
                        postBox = d.PostBox,
                        city = d.City,
                        orderAb = d.OrderAb,
                    });
                    return Ok(new
                    {
                        code = "200",
                        status = "Success",
                        message = "Delivery Data Returned successfully !",
                        data = rows,
                    });
                }
                return StatusCode(400, new CustomResponse { Code = "400", Message = "Error" });
            }
            catch(Exception)
            {
                return StatusCode(400, new CustomResponse { Code = "400", Message = "Error" });

            }

        }
        #endregion

        

        
    }
}
