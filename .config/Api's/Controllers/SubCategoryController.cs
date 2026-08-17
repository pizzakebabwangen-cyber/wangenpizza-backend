using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WangenPizza.Helper.Response;
using WangenPizza.Interfaces;
using WangenPizza.Services;

namespace WangenPizza.Api_s.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SubCategoryController : ControllerBase
    {
        #region Ctor

        private readonly IMapper mapper;
        private readonly ICategoryService CategoryService;
        private readonly ISubCategoryService subCategoryService;

        public SubCategoryController(IMapper mapper, ICategoryService CategoryService, ISubCategoryService subCategoryService)
        {
            this.mapper = mapper;
            this.CategoryService = CategoryService;
            this.subCategoryService = subCategoryService;
        }
        #endregion

        #region Get All SubCategorys 
        [HttpGet("GetAllSubCategorys")]
        public async Task<IActionResult> GetAllSubCategorys()
        {
           try
            {
                var data =await subCategoryService.Get();
                if (data != null)
                {
                    SubCategorysResponse response = new SubCategorysResponse
                    {
                        Code = "200",
                        Status = "Success",
                        Message = "SubCategorys Data Returned successfully !",
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

        #region Get All SubCategorys 
        [HttpGet("GetAllSubCategorysByCategoryId")]
        public async Task<IActionResult> GetAllSubCategorysByCategoryId(int id)
        {
            try
            {
                var data = await subCategoryService.GetByCategoryId(id);
                if (data != null)
                {
                    SubCategorysResponse response = new SubCategorysResponse
                    {
                        Code = "200",
                        Status = "Success",
                        Message = "SubCategorys Data Returned successfully !",
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
