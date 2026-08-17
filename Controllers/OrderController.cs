using AutoMapper;
using MailKit;
using MessagePack;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using WangenPizza.Context;
using WangenPizza.Dtos;
using WangenPizza.Helper.Response;
using WangenPizza.Helper;
using WangenPizza.Interfaces;
using WangenPizza.Migrations;
using WangenPizza.Models;
using WangenPizza.Services;

namespace WangenPizza.Controllers
{
    public class OrderController : Controller
    {
        private readonly IHubContext<NotificationHub> hubContext;
		private readonly ICompanyService companyService;
		private readonly ApplicationDbContext context;
        private readonly Interfaces.IMailService mailService;
        private readonly ITempOrderService tempOrderService;
        private readonly IEmailHtmlTemplateService emailHtmlTemplateService;

        #region Ctor

        private readonly IMapper mapper;
        private readonly ICartService cartService;
		private readonly IRazorViewEngine _razorViewEngine;
		private readonly ITempDataProvider _tempDataProvider;
		private readonly IConfiguration _configuration;

        public OrderController(IHubContext<NotificationHub> hubContext, ICompanyService companyService, ApplicationDbContext context,Interfaces.IMailService mailService ,ITempOrderService tempOrderService,IEmailHtmlTemplateService emailHtmlTemplateService, IMapper mapper, ICartService cartService , IRazorViewEngine razorViewEngine, ITempDataProvider tempDataProvider, IConfiguration configuration)
        {
            this.hubContext = hubContext;
			this.companyService = companyService;
			this.context = context;
            this.mailService = mailService;
            this.tempOrderService = tempOrderService;
            this.emailHtmlTemplateService = emailHtmlTemplateService;
            this.mapper = mapper;
            this.cartService = cartService;
			_razorViewEngine = razorViewEngine;
			_tempDataProvider = tempDataProvider;
			_configuration = configuration;
		}



		
		#endregion


		#region get all arders 
		[ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
		public async Task<IActionResult> Pos()
		{
			try
			{
				var loc = Assembly.GetExecutingAssembly().Location;
				if (!string.IsNullOrEmpty(loc) && System.IO.File.Exists(loc))
					ViewBag.PosBuildUtc = System.IO.File.GetLastWriteTimeUtc(loc).ToString("yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture) + " UTC (DLL)";
				else
					ViewBag.PosBuildUtc = (Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "?") + " (Version)";
			}
			catch
			{
				ViewBag.PosBuildUtc = "?";
			}

			var data = await context.Orders
				.AsNoTracking()
				.Where(o => o.IsPaymentSucceeded || o.PaymentWay == 1)
				.Include(o => o.OrderItems!)
					.ThenInclude(oi => oi.Product)
				.Include(o => o.OrderItems!)
					.ThenInclude(oi => oi.ExtensionOrderItem)
				.OrderByDescending(o => o.Id)
				.ToListAsync();
			// View heißt absichtlich nicht „Pos“: ein alter Views/Order/Pos.cshtml auf dem Host überschrieb früher die kompilierte Ansicht.
			return View("PosStepper", data);
		}

		[HttpGet]
		[ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
		public async Task<IActionResult> PosPendingStatus()
		{
			var pending = await context.Orders
				.AsNoTracking()
				.Where(o => (o.IsPaymentSucceeded || o.PaymentWay == 1) && !o.PosAcknowledged)
				.Select(o => o.Id)
				.ToListAsync();

			return Json(new
			{
				hasPendingUnacceptedOrders = pending.Count > 0,
				pendingCount = pending.Count,
				latestOrderId = pending.Count == 0 ? 0 : pending.Max()
			});
		}

		public async Task<IActionResult> Index()
        {
            var data = await cartService.GetAllSucceededOrders();
            var model = mapper.Map<IEnumerable<OrderDto>>(data);

            return View(model);
        }

        /// <summary>Online-Wertgutschein-Bestellungen (Pickup_type voucher / Notes WERTGUTSCHEIN) — Zahlstatus & Betrag.</summary>
        [Authorize(Roles = "Admin")]
        [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
        public async Task<IActionResult> Wertgutscheine()
        {
            var data = await context.Orders
                .AsNoTracking()
                .Where(o =>
                    o.Pickup_type == "voucher" ||
                    (o.Notes != null && o.Notes.Contains("WERTGUTSCHEIN")))
                .OrderByDescending(o => o.Id)
                .Take(500)
                .ToListAsync();
            return View(data);
        }
        #endregion

        #region Edit Order
        public async Task<IActionResult> Details(int id)
        {
			var data = await cartService.GetOrderItemById(id);
			var companyData = await companyService.GetById(1);

			TempData["OrderId"] = id;

            OrderDetailsDto model = new OrderDetailsDto()
            {
                Id = data.Id,
                Name = data.Name,
				Phone = data.Mobile,
				Email = data.Email,
                Street = data.Street,
                PostBox = data.PostBox,
                City = data.City,
                DiscountValue = data.DiscountValue,
                TotalNumber = data.TotalNumber,
                FinalTotalNumber = data.FinalTotalNumber,
                DateAdded = data.DateAdded,
                DeliveryDate = data.DeliveryDate,
                DeliveryTime = data.DeliveryTime,
                Notes = data.Notes,
                Items = data.OrderItems?.ToList(),
				CompanyCity = companyData.City,
				CompanyEmail = companyData.Email,
				CompanyPhone1 = companyData.Phone1,
				CompanyPhone2 = companyData.Phone2,
				CompanyPostbox = companyData.Postbox,
				CompanyStreet = companyData.Street,
				CompanyName = companyData.Name,
                PaymentWay = data.PaymentWay,
                Pickup_type = data.Pickup_type

            };
         
            return View(model);
        }


		#endregion

		#region Print 
		[HttpGet]
		public async Task<IActionResult> Print(int id)
			{
            // Check if the orderId is valid
            if (id <= 0)
            {
                return StatusCode(400, "Invalid order ID.");
            }

            // Get order data
            var data = await cartService.GetOrderItemById(id);
            if (data == null)
            {
                return StatusCode(404, "Order not found.");
            }

            var companyData =await companyService.GetById(1);

            // Prepare the model
            OrderDetailsDto model = new OrderDetailsDto()
            {
                Id = data.Id,
                Name = data.Name,
                Phone = data.Mobile,
                Email = data.Email,
                Street = data.Street,
                PostBox = data.PostBox,
                City = data.City,
                DiscountValue = data.DiscountValue,
                TotalNumber = data.TotalNumber,
                Pickup_type = data.Pickup_type,
                FinalTotalNumber = data.FinalTotalNumber,
                DateAdded = data.DateAdded,
                DeliveryDate = data.DeliveryDate ,// Handle potential nulls safely
                DeliveryTime = data.DeliveryTime,
                Notes = data.Notes,
                Items = data.OrderItems.ToList(), // Ensure the collection is in the expected format
                CompanyCity = companyData.City,
                CompanyEmail = companyData.Email,
                CompanyPhone1 = companyData.Phone1,
                CompanyPhone2 = companyData.Phone2,
                CompanyPostbox = companyData.Postbox,
                CompanyStreet = companyData.Street,
                CompanyName = companyData.Name,
                PaymentWay = data.PaymentWay,
            };

            // Set payment way

            // Mark as printed and save changes
            data.IsPrinted = true;
            context.Orders.Update(data);
            await context.SaveChangesAsync(); // Use async version

            // Return the view with the model
            return View(model);
        }

		public async Task<IActionResult> Delete(int id)
		{
			try
			{


                var orderId = Convert.ToInt32(TempData["OrderId"]);
				orderId = id;
            
                // Retrieve the order by its ID including related order items
                var order = await context.Orders
										
										.FirstOrDefaultAsync(o => o.Id == orderId);

				if (order != null)
				{
					// Remove the order items from the database
					context.OrderItem.RemoveRange(order.OrderItems);
					context.Entry(order).State = EntityState.Deleted;
					await context.SaveChangesAsync();

                    return RedirectToAction("Index");
                }
                else
				{
                    return RedirectToAction("Index");
                }
            }
			catch (Exception ex)
			{
				return StatusCode(500, "An error occurred while deleting items: " + ex.Message); // Return error response
			}
		}


        #endregion


        #region DeleteHistoryByDate
        [HttpPost]
        public IActionResult DeleteHistoryByDate(string fromDate, string toDate)
        {
            var fromDateParsed = DateTime.ParseExact(fromDate, "dd.MM.yyyy", CultureInfo.InvariantCulture);
            var toDateParsed = DateTime.ParseExact(toDate, "dd.MM.yyyy", CultureInfo.InvariantCulture);

            // Get Orders within the date range
            var ordersToDelete = context.Orders
                .Where(o => o.DateAdded >= fromDateParsed && o.DateAdded <= toDateParsed)
                .ToList();

            context.Orders.RemoveRange(ordersToDelete);

            // Get ShoppingCarts within the date range
            var shoppingCartsToDelete = context.ShoppingCarts
                .Where(sc => sc.LastOperationTimestamp >= fromDateParsed && sc.LastOperationTimestamp <= toDateParsed)
                .Include(sc => sc.Items) // Include related CartItems
                .Include(sc => sc.OrderItems) // Include related OrderItems
                .ToList();

            foreach (var cart in shoppingCartsToDelete)
            {
                // Remove CartItems associated with the ShoppingCart
                context.CartItems.RemoveRange(cart.Items);

                // Remove OrderItems associated with the ShoppingCart
                context.OrderItem.RemoveRange(cart.OrderItems);
            }

            // Now remove the ShoppingCarts
            context.ShoppingCarts.RemoveRange(shoppingCartsToDelete);

            // Save changes
            context.SaveChanges();

            return RedirectToAction("Index");
        }



        #endregion

		private static readonly int[] PreparationEmailMinutes = { 15, 20, 30, 45, 50, 60 };

		[HttpPost]
		[ValidateAntiForgeryToken]
		public IActionResult SendPreparationTimeEmail(int id, int minutes)
		{
			TempData["PosError"] =
				"Kein zweiter E-Mail-Versand: Lieferzeit geht nur über «Akzeptieren» (einmalige Kunden-Mail).";
			return RedirectToAction(nameof(Pos));
		}

		/// <summary>PNG-QR für Kurier (Google Maps Route) — ohne externe Bild-API.</summary>
		[HttpGet]
		public async Task<IActionResult> KurierQrPng(int id)
		{
			var order = await cartService.GetOrderById(id);
			if (order == null)
				return NotFound();
			if (!KurierQrHelper.IsDeliveryPickupType(order.Pickup_type))
				return NotFound();
			var dest = $"{order.Street} {order.PostBox} {order.City}".Trim();
			if (string.IsNullOrWhiteSpace(dest))
				return NotFound();
			var png = KurierQrHelper.MapsRoutePng(dest);
			return File(png, "image/png");
		}

		/// <summary>POS: Bestellung annehmen ohne Druck — Druck nur über Details → Print.</summary>
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> AcknowledgePosOrder([FromForm] int id, [FromForm] int minutes)
		{
			if (!PreparationEmailMinutes.Contains(minutes))
			{
				TempData["PosError"] = "Ungültige Minutenangabe für die Kunden-E-Mail.";
				return RedirectToAction(nameof(Pos));
			}

			var exists = await context.Orders.AnyAsync(o => o.Id == id);
			if (!exists)
			{
				TempData["PosError"] = "Bestellung nicht gefunden.";
				return RedirectToAction(nameof(Pos));
			}

			// Genau eine Kunden-E-Mail: erst DB-Zeile „sperren“, dann senden (Doppelklick zweiter Request sendet nichts).
			var rows = await context.Database.ExecuteSqlInterpolatedAsync(
				$"UPDATE Orders SET PosAcknowledged = CAST(1 AS bit), PreparationMinutesEmailed = {minutes} WHERE Id = {id} AND PosAcknowledged = CAST(0 AS bit)");
			if (rows != 1)
			{
				TempData["PosMessage"] =
					$"Bestellung #{id} war bereits akzeptiert. Drucken: Details → Print.";
				return RedirectToAction(nameof(Pos));
			}

			var order = await cartService.GetOrderById(id);
			if (order == null)
			{
				await context.Database.ExecuteSqlRawAsync(
					"UPDATE Orders SET PosAcknowledged = CAST(0 AS bit), PreparationMinutesEmailed = NULL WHERE Id = {0}", id);
				TempData["PosError"] = $"Bestellung #{id}: Daten nicht geladen – bitte Support.";
				return RedirectToAction(nameof(Pos));
			}

			var email = order.Email?.Trim();
			if (string.IsNullOrEmpty(email))
			{
				await context.Database.ExecuteSqlRawAsync(
					"UPDATE Orders SET PosAcknowledged = CAST(0 AS bit), PreparationMinutesEmailed = NULL WHERE Id = {0}", id);
				TempData["PosError"] = $"Bestellung #{id}: keine E-Mail – Bestätigung nicht gesendet.";
				return RedirectToAction(nameof(Pos));
			}

			var timePhrase = OrderConfirmationMailHelper.PhrasePreparationMinutes(minutes);
			var wishDisplay = OrderConfirmationMailHelper.FormatDeliveryTimeDisplay(order.DeliveryTime);
			var confirmSummary = OrderConfirmationMailHelper.IsAsapOrEmptyDeliveryWish(order.DeliveryTime)
				? timePhrase
				: wishDisplay;
			try
			{
				var body = OrderConfirmationMailHelper.BuildCustomerOrderConfirmationHtml(order, minutes);
				var subject = OrderConfirmationMailHelper.BuildCustomerOrderConfirmationEmailSubject(id, order, minutes);
				var pdfBytes = OrderSummaryPdf.Generate(order, minutes);
				await mailService.SendEmailAsync(new MailRequest
				{
					ToEmail = email,
					Subject = subject,
					Body = body,
					Attachments = new List<FileAttachment>
					{
						new FileAttachment
						{
							File = pdfBytes,
							Name = $"Bestellbestaetigung-{order.Id}.pdf",
							ContentType = "application/pdf"
						}
					}
				}, default);
			}
			catch (Exception ex)
			{
				await context.Database.ExecuteSqlRawAsync(
					"UPDATE Orders SET PosAcknowledged = CAST(0 AS bit), PreparationMinutesEmailed = NULL WHERE Id = {0}", id);
				var reason = ex.GetBaseException().Message;
				if (reason.Length > 220)
					reason = reason[..220] + "…";
				TempData["PosError"] =
					$"Bestellung #{id}: E-Mail an Kunde fehlgeschlagen. {reason}";
				return RedirectToAction(nameof(Pos));
			}

			var mailSection = _configuration.GetSection("MailSettings");
			var adminNotify = mailSection["OrderAdminNotifyEmail"];
			var adminEmail = string.IsNullOrWhiteSpace(adminNotify) ? mailSection["Mail"] : adminNotify;
			if (!string.IsNullOrWhiteSpace(adminEmail))
			{
				try
				{
					var adminBody = OrderConfirmationMailHelper.BuildPosAcknowledgeAdminNotifyHtml(order, minutes);
					await mailService.SendEmailAsync(new MailRequest
					{
						ToEmail = adminEmail.Trim(),
						Subject = $"POS: Bestellung #{id} akzeptiert – {order.Name}",
						Body = adminBody
					}, default);
				}
				catch (Exception ex)
				{
					var reason = ex.GetBaseException().Message;
					if (reason.Length > 180)
						reason = reason[..180] + "…";
					TempData["PosMessage"] =
						$"Bestellung #{id}: Bestätigung ({confirmSummary}) an Kunde gesendet. Interne Restaurant-Mail fehlgeschlagen: {reason}";
					return RedirectToAction(nameof(Pos));
				}
			}

			TempData["PosMessage"] =
				$"Bestellung #{id}: Bestätigung ({confirmSummary}) an Kunde gesendet. Restaurant wurde informiert. Drucken: Details → Print.";
			return RedirectToAction(nameof(Pos));
		}

        #region Verify Email


        public async Task<IActionResult> VerifyEmail(string token)
        {
            try
            {
                // Retrieve the reservation data using the token
                var order = await tempOrderService.GetOrderByTokenAsync(token);
                if (order != null)
                {
                    order.Verified = true;
                    var data = mapper.Map<OrderDto>(order);
                    await cartService.CreateOrder(data);

                    CustomResponse response = new CustomResponse
                    {
                        Code = "200",
                        Status = "Success",
                        Message = "Reservation verified and created successfully!"
                    };

                    string body = emailHtmlTemplateService.GetOrderTemplate(order);
                    MailRequest mailRequest = new MailRequest
                    {
                        ToEmail = order.Email,
                        Subject = "Wangen",
                        Body = body,

                    };
                    await mailService.SendEmailAsync(mailRequest, default);
                    DateTime date = DateTime.Now;
                    string formattedDate = date.ToString("HH:mm tt");
                    await hubContext.Clients.All.SendAsync("ReceiveNotification", $"Neue Bestellung {formattedDate}   Name :{order.Name} {order.PostBox} {order.City} ", "order");
                    return View();
                }

                return StatusCode(400, new CustomResponse { Code = "400", Message = "Invalid token or reservation not found." });
            }
            catch (Exception)
            {
                return StatusCode(400, new CustomResponse { Code = "400", Message = "Error" });
            }
        }
        #endregion

    
    }
}
