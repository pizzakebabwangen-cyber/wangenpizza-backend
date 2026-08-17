using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WangenPizza.Helper.Response;
using WangenPizza.Interfaces;

namespace WangenPizza.Api_s.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TodayBonusController : ControllerBase
    {
        #region Ctor

        private readonly IMapper mapper;
        private readonly ITodayBonusService TodayBonusService;
        private readonly ISubCategoryService subCategoryService;

        public TodayBonusController(IMapper mapper, ITodayBonusService TodayBonusService, ISubCategoryService subCategoryService)
        {
            this.mapper = mapper;
            this.TodayBonusService = TodayBonusService;
            this.subCategoryService = subCategoryService;
        }
        #endregion

        #region Get All TodayBonuss 
        [HttpGet("GetAllTodayBonus")]
        public async Task<IActionResult> GetAllTodayBonuss()
        {
           try
            {
                var data =await TodayBonusService.Get();
                if (data != null)
                {
                    ProductsResponse response = new ProductsResponse
                    {
                        Code = "200",
                        Status = "Success",
                        Message = "TodayBonus Data Returned successfully !",
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
