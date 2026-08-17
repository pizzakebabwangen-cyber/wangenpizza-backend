using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WangenPizza.Helper.Response;
using WangenPizza.Interfaces;

namespace WangenPizza.Api_s.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoryController : ControllerBase
    {
        #region Ctor

        private readonly IMapper mapper;
        private readonly ICategoryService CategoryService;
        private readonly ISubCategoryService subCategoryService;

        public CategoryController(IMapper mapper, ICategoryService CategoryService, ISubCategoryService subCategoryService)
        {
            this.mapper = mapper;
            this.CategoryService = CategoryService;
            this.subCategoryService = subCategoryService;
        }
        #endregion

        #region Get All Categorys 
        [HttpGet("GetAllCategorys")]
        public async Task<IActionResult> GetAllCategorys()
        {
           try
            {
                var data =await CategoryService.Get();
                if (data != null)
                {
                    CategoryResponse response = new CategoryResponse
                    {
                        Code = "200",
                        Status = "Success",
                        Message = "Categorys Data Returned successfully !",
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
