using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WangenPizza.Helper.Response;
using WangenPizza.Interfaces;

namespace WangenPizza.Api_s.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ExtensionController : ControllerBase
    {

        #region Ctor

        private readonly IMapper mapper;
        private readonly IExtensionService ExtensionService;
        private readonly ISubCategoryService subCategoryService;

        public ExtensionController(IMapper mapper, IExtensionService ExtensionService, ISubCategoryService subCategoryService)
        {
            this.mapper = mapper;
            this.ExtensionService = ExtensionService;
            this.subCategoryService = subCategoryService;
        }
        #endregion

        #region Get All Extensions 
        [HttpGet("GetAllExtensions")]
        public async Task<IActionResult> GetAllExtensions()
        {
           try
            {
                var data =await ExtensionService.Get();
                if (data != null)
                {
                    ExtensionResponse response = new ExtensionResponse
                    {
                        Code = "200",
                        Status = "Success",
                        Message = "Extensions Data Returned successfully !",
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

        #region Get All Extensions By CategoryId
        [HttpGet("GetAllExtensionsByCategoryId")]
        public async Task<IActionResult> GetAllExtensionsByCategoryId(int categoryId)
        {
            try
            {
                var data = await ExtensionService.GetByCategoryId(categoryId);
                if (data != null)
                {
                    ExtensionResponse response = new ExtensionResponse
                    {
                        Code = "200",
                        Status = "Success",
                        Message = "Extensions Data Returned successfully !",
                        Data = data
                    };
                    return Ok(response);
                }
                return StatusCode(400, new CustomResponse { Code = "400", Message = "Error" });
            }
            catch (Exception)
            {
                return StatusCode(400, new CustomResponse { Code = "400", Message = "Error" });

            }

        }
        #endregion


    }
}
