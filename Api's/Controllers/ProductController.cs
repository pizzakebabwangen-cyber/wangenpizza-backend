using System.Media;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using WangenPizza.Helper;
using WangenPizza.Helper.Response;
using WangenPizza.Interfaces;

namespace WangenPizza.Api_s.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductController : ControllerBase
    {
        #region Ctor

        private readonly IMapper mapper;
        private readonly IMailService mailService;
        private readonly IProductService ProductService;
        private readonly ISubCategoryService subCategoryService;
        private readonly IHubContext<NotificationHub> _hubContext;


        public ProductController(IMapper mapper,IMailService mailService, IProductService ProductService, ISubCategoryService subCategoryService, IHubContext<NotificationHub> hubContext)
        {
            this.mapper = mapper;
            this.mailService = mailService;
            this.ProductService = ProductService;
            this.subCategoryService = subCategoryService;
            _hubContext = hubContext;

        }
        #endregion

        #region Get All Products 
        [HttpGet("GetAllProducts")]
        public async Task<IActionResult> GetAllProducts()
        {
           try
            {
                var data =await ProductService.Get();
                if (data != null)
                {
                    ProductsResponse response = new ProductsResponse
                    {
                        Code = "200",
                        Status = "Success",
                        Message = "Products Data Returned successfully !",
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

        #region Get All Offers 
        [HttpGet("GetAllOffers")]
        public async Task<IActionResult> GetAllOffers()
        {
            try
            {
                var data = await ProductService.GetOffers();
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
            catch (Exception)
            {
                return StatusCode(400, new CustomResponse { Code = "400", Message = "Error" });

            }

        }
        #endregion

        #region Get All TodayBonus 
        [HttpGet("GetAllTodayBonus")]
        public async Task<IActionResult> GetAllTodayBonus()
        {
            try
            {
                var data = await ProductService.GetTodayBonus();
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
            catch (Exception)
            {
                return StatusCode(400, new CustomResponse { Code = "400", Message = "Error" });

            }

        }
        #endregion

        #region Get All Home Products 
        [HttpGet("GetHomeProducts")]
        public async Task<IActionResult> GetHomeProducts()
        {
            try
            {
                var data = await ProductService.GetProductsToHome();
                if (data != null)
                {
                    ProductsResponse response = new ProductsResponse
                    {
                        Code = "200",
                        Status = "Success",
                        Message = "Home Products Data Returned successfully !",
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

        #region Get All Products by categoryId
        [HttpGet("GetAllProductsByCategoryId")]
        public async Task<IActionResult> GetAllProductsByCategoryId(int id)
        {
            try
            {
                var data = await ProductService.GetByCatgoryId(id);
                if (data != null)
                {
                    ProductsResponse response = new ProductsResponse
                    {
                        Code = "200",
                        Status = "Success",
                        Message = "Products Data Returned successfully !",
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

        #region Get All Products by categoryId
        [HttpGet("GetAllProductsBySubCategoryId")]
        public async Task<IActionResult> GetAllProductsBySubCategoryId(int id)
        {
            try
            {
                var data = await ProductService.GetBySubCatgoryId(id);
                if (data != null)
                {
                    ProductsResponse response = new ProductsResponse
                    {
                        Code = "200",
                        Status = "Success",
                        Message = "Products Data Returned successfully !",
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
