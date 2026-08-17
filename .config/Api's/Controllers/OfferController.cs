using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WangenPizza.Helper.Response;
using WangenPizza.Interfaces;

namespace WangenPizza.Api_s.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OfferController : ControllerBase
    {
        #region Ctor

        private readonly IMapper mapper;
        private readonly IOfferService OfferService;
        private readonly ISubCategoryService subCategoryService;

        public OfferController(IMapper mapper, IOfferService OfferService, ISubCategoryService subCategoryService)
        {
            this.mapper = mapper;
            this.OfferService = OfferService;
            this.subCategoryService = subCategoryService;
        }
        #endregion

        #region Get All Offers 
        [HttpGet("GetAllOffers")]
        public async Task<IActionResult> GetAllOffers()
        {
           try
            {
                var data =await OfferService.Get();
                if (data != null)
                {
                    ProductsResponse response = new ProductsResponse
                    {
                        Code = "200",
                        Status = "Success",
                        Message = "Offers Data Returned successfully !",
                        Data = data
                    };
                    return Ok(response);
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
