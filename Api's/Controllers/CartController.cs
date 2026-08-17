using AutoMapper;
using MailKit;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Stripe.Climate;
using System.Net.NetworkInformation;
using System.Security.Claims;
using WangenPizza.Dtos;
using WangenPizza.Helper;
using WangenPizza.Helper.Response;
using WangenPizza.Interfaces;
using WangenPizza.Models;
using WangenPizza.Services;

namespace WangenPizza.Api_s.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CartController : ControllerBase
    {
        /// <summary>Schlankes JSON für SPA — ohne OrderItems/Items/EF-Navigations (vermeidet 500 / Zyklen / Riesenpayload).</summary>
        private static object ToSpaOrderPayload(WangenPizza.Models.Order o) => new
        {
            id = o.Id,
            userId = o.UserId,
            salute = o.Salute,
            name = o.Name,
            mobile = o.Mobile,
            email = o.Email,
            street = o.Street,
            postBox = o.PostBox,
            city = o.City,
            totalNumber = o.TotalNumber,
            discountValue = o.DiscountValue,
            gutscheinDeduction = o.GutscheinDeduction,
            appliedGutscheinCode = o.AppliedGutscheinCode,
            finalTotalNumber = o.FinalTotalNumber,
            pickup_type = o.Pickup_type,
            paymentWay = o.PaymentWay,
            deliveryTime = o.DeliveryTime,
            deliveryDate = o.DeliveryDate,
            notes = o.Notes,
            isPaymentSucceeded = o.IsPaymentSucceeded,
            isPrinted = o.IsPrinted,
        };

        private readonly ICartService _cartService;
        private readonly Interfaces.IMailService mailService;
        private readonly IEmailHtmlTemplateService emailHtmlTemplateService;
        private readonly IMapper mapper;
        private readonly ITempOrderService tempOrderService;
        private readonly PostFinancePaymentService postFinancePaymentService;
        private readonly IHubContext<NotificationHub> _hubContext;
        private readonly StripeService stripeService;

        public CartController(ICartService cartService,Interfaces.IMailService mailService, IEmailHtmlTemplateService emailHtmlTemplateService, IMapper mapper,ITempOrderService tempOrderService, PostFinancePaymentService postFinancePaymentService, IHubContext<NotificationHub> hubContext, StripeService stripeService)
        {
            _cartService = cartService;
            this.mailService = mailService;
            this.emailHtmlTemplateService = emailHtmlTemplateService;
            this.mapper = mapper;
            this.tempOrderService = tempOrderService;
            this.postFinancePaymentService = postFinancePaymentService;
            _hubContext = hubContext;
            this.stripeService = stripeService;
        }

        [HttpGet("active-menu-offer")]
        public async Task<IActionResult> ActiveMenuOffer()
        {
            var preview = await _cartService.GetActiveMenuOfferAsync();
            if (preview == null)
                return Ok(new { active = false });

            return Ok(new
            {
                active = true,
                title = string.IsNullOrWhiteSpace(preview.Note) ? "Aktion" : preview.Note,
                code = preview.Name,
                value = preview.Value,
                expiryDate = preview.ExpiryDate.ToString("dd.MM.yyyy")
            });
        }

        [HttpGet("cart")]
        public async Task<IActionResult> GetCart()
        {
            try
            {
                var result = await _cartService.GetCart();
                if (result != null)
                {
                    
                    GetCartResponse response = new GetCartResponse()
                    {
                        Code = "200",
                        Status = "Success",
                        Message = "Cart date returned successfully !",
                        Data= result.Value
                    };
                    return Ok(response);
                }
                return new BadRequestObjectResult("Cart is empty");

            }
            catch (Exception ex)
            {
                return new BadRequestObjectResult("Error");

            }
        }

        [HttpPost("cart/add")]
        public async Task<IActionResult> AddToCart([FromBody] AddToCartDTO addToCartDTO)
        {
            var result = _cartService.AddToCart(addToCartDTO);
            return Ok(result.Value);
        }
        [HttpPost("pickup_cart/add")]
        public async Task<IActionResult> Pickup_AddToCart([FromBody] AddToCartDTO addToCartDTO)
        {
            var result = _cartService.Pickup_AddToCart(addToCartDTO);
            return Ok(result.Value);
        }

        [HttpPost("order")]
        public async Task<IActionResult> CreateOrder(OrderDto dto)
        {
            try
            {
                var createResult = await _cartService.CreateOrder(dto);
                if (createResult.Result != null)
                    return createResult.Result;

                var order = createResult.Value;
                if (order == null)
                    return BadRequest("Die Bestellung konnte nicht erstellt werden.");

                // Barzahlung: nur Order anlegen — Freigabe/E-Mails erst nach AGB + GET /api/Payment/success (wie SPA-Flow).
                if (dto.PaymentWay == 1)
                {
                    return Ok(new
                    {
                        code = "200",
                        status = "Success",
                        message = "Order created. Please confirm terms on the next page.",
                        paymentPageUrl = "",
                        data = ToSpaOrderPayload(order),
                    });
                }

                if (order.FinalTotalNumber <= 0m &&
                    order.GutscheinDeduction > 0m &&
                    !string.IsNullOrWhiteSpace(order.AppliedGutscheinCode))
                {
                    return Ok(new
                    {
                        code = "200",
                        status = "Success",
                        message = "Order created. Gutschein deckt den Gesamtbetrag.",
                        paymentPageUrl = "",
                        data = ToSpaOrderPayload(order),
                    });
                }

                // CASE 2: Online Payment (default)
                var transaction = postFinancePaymentService.CreateTransaction(order);
                var paymentPageUrl = postFinancePaymentService.GetPaymentPageUrl(transaction.Id);
                return Ok(new
                {
                    code = "200",
                    status = "Success",
                    message = "Order Created successfully !",
                    paymentPageUrl,
                    data = ToSpaOrderPayload(order),
                });
            }

            catch (Exception ex)
            {
                // Tritt bei Barzahlung («Weiter») und Online gleich auf — Message in Browser-Network / Logs prüfen.
                return StatusCode(500, $"CreateOrder failed: {ex.Message}");

			}
		}

        [HttpPost("wertgutschein-checkout")]
        public async Task<IActionResult> WertgutscheinCheckout([FromBody] WertgutscheinCheckoutDto dto)
        {
            try
            {
                if (dto == null)
                    return BadRequest("Ungültige Anfrage.");

                var result = await _cartService.CreateWertgutscheinOrder(dto);
                if (result.Value == null)
                    return BadRequest(result);

                var order = result.Value;
                var lineName = dto.VoucherQuantity > 1
                    ? $"Wertgutschein CHF {dto.FaceValueChf:F0} × {dto.VoucherQuantity}"
                    : $"Wertgutschein CHF {dto.FaceValueChf:F0}";

                var transaction = postFinancePaymentService.CreateTransaction(order, lineName, "voucher");
                var paymentPageUrl = postFinancePaymentService.GetPaymentPageUrl(transaction.Id);
                return Ok(new
                {
                    code = "200",
                    status = "Success",
                    message = "Wertgutschein-Zahlung vorbereitet.",
                    paymentPageUrl,
                    data = ToSpaOrderPayload(order),
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Zahlung konnte nicht vorbereitet werden: {ex.Message}");
            }
        }

        [HttpPost("pickup_order")]
        public async Task<IActionResult> CreatePickup_Order(OrderDto dto)
        {
            try
            {
                var data = mapper.Map<WangenPizza.Models.Order>(dto);
                data.Verified = false; // Set verified to false initially

                // Generate a unique token
                var token = Guid.NewGuid().ToString();

                // Store the token and reservation data temporarily (e.g., in-memory cache or a temporary table)
                await tempOrderService.StoreOrderAsync(token, data);

                string verificationUrl = Url.Action("VerifyEmail", "Order", new { token = token }, Request.Scheme);
                string body = emailHtmlTemplateService.GetOrderTemplate(dto, verificationUrl);

                MailRequest mailRequest = new MailRequest
                {
                    ToEmail = dto.Email,
                    Subject = "Verify your reservation",
                    Body = body,
                };
                await mailService.SendEmailAsync(mailRequest, default);

                CustomResponse response = new CustomResponse
                {
                    Code = "200",
                    Status = "Success",
                    Message = "Verification email sent successfully! Please check your email to verify your reservation."
                };

                return Ok(response);
            }
            catch (Exception)
            {
                return StatusCode(400, new CustomResponse { Code = "400", Message = "Error" });
            }
        }


        [HttpPost("Checkout")]
        public async Task<IActionResult> GetCheckout(string? DiscountCode)
        {
            try
            {
                var result = await _cartService.Checkout(DiscountCode);
                if (result != null)
                {
                    CheckoutResponse response = new CheckoutResponse()
                    {
                        Code = "200",
                        Status = "Success",
                        Message = "Checkout data returned successfully !",
                        Data = result.Value
                    };
                    return Ok(response);
                }
                return new BadRequestObjectResult("Cart is empty");
            }

            catch (Exception ex)
            {
                return new BadRequestObjectResult("Error");

            }
        }


        #region Delete From Cart

        [HttpDelete("DeleteFromCart")]
        public IActionResult DeleteFromCart(int cartItemId)
        {
            try
            {

                if (ModelState.IsValid)
                {
                    _cartService.DeleteCartItem(cartItemId);

                    CustomResponse Cusotm = new CustomResponse
                    {

                        Code = "200",
                        Message = "Item Deleted Successfully ! ",
                        Status = "Done"

                    };
                    return Ok(Cusotm);

                }

                return StatusCode(400, new CustomResponse { Code = "400", Message = "Invalid Data Annotation" });

            }
            catch (Exception ex)
            {
                return NotFound(new CustomResponse
                {
                    Code = "400",
                    Message = ex.Message,
                    Status = "Faild"
                });
            }
        }
        #endregion


        [HttpPost("create-payment-intent")]
        public async Task<IActionResult> CreatePaymentIntent([FromBody] ClientSecretDto dto)
        {
            try
            {
                var order = await _cartService.GetOrderById(dto.OrderId);
                var clientSecret = await stripeService.CreatePaymentIntent(dto);
                if(order !=null)
                {
                    CheckoutDto checkoutDto = new CheckoutDto()
                    {
                        orderId= order.Id,
                        clientSecret = clientSecret,
                        DiscountValue = order.DiscountValue,
                        CartTotalNumber = order.TotalNumber,
                        TotalAfterDiscount = order.FinalTotalNumber
                    };
                    return Ok(checkoutDto);

                }
                return StatusCode(500, $"Failed to create payment intent");

            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Failed to create payment intent: {ex.Message}");
            }
        }

        #region Delete From Cart

        [HttpPost("PaymentSucceeded")]
        public async Task<IActionResult> IsPaymentSucceeded(int orderId)
        {
            try
            {

                if (ModelState.IsValid)
                {
                 var order = await  _cartService.GetOrderById(orderId);
                    order.IsPaymentSucceeded = true;
                     _cartService.UpdateOrder(order);

                    CustomResponse Cusotm = new CustomResponse
                    {

                        Code = "200",
                        Message = " Payment completed Successfully ! ",
                        Status = "Done"

                    };
                    return Ok(Cusotm);

                }

                return StatusCode(400, new CustomResponse { Code = "400", Message = "Invalid Data Annotation" });

            }
            catch (Exception ex)
            {
                return NotFound(new CustomResponse
                {
                    Code = "400",
                    Message = ex.Message,
                    Status = "Faild"
                });
            }
        }
        #endregion

    }
}
