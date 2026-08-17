using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using WangenPizza.Context;
using WangenPizza.Dtos;
using WangenPizza.Helper;
using WangenPizza.Interfaces;
using WangenPizza.Models;

namespace WangenPizza.Services
{
    public class CartService : ICartService
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly IDiscountCodeService discountCodeService;
        private readonly IHttpContextAccessor httpContextAccessor;
        private readonly IDeliveryService deliveryService;


        public CartService(ApplicationDbContext dbContext , IDiscountCodeService discountCodeService , IHttpContextAccessor httpContextAccessor, IDeliveryService deliveryService)
        {
            _dbContext = dbContext;
            this.discountCodeService = discountCodeService;
            this.httpContextAccessor = httpContextAccessor;
            this.deliveryService = deliveryService;

        }

        /// <summary>Extracts a 4-digit Swiss-style PLZ from PostBox and/or City fields (same idea as the SPA).</summary>
        private static string? NormalizeSwissPostCode(string? postBox, string? city)
        {
            var pb = (postBox ?? "").Trim();
            if (Regex.IsMatch(pb, @"^\d{4}$")) return pb;
            var m1 = Regex.Match(pb, @"\b(\d{4})\b");
            if (m1.Success) return m1.Groups[1].Value;
            var c = (city ?? "").Trim();
            var m2 = Regex.Match(c, @"^(\d{4})\b");
            if (m2.Success) return m2.Groups[1].Value;
            var m3 = Regex.Match(c, @"\b(\d{4})\b");
            return m3.Success ? m3.Groups[1].Value : null;
        }

        private static readonly Regex SwissHausnummerRegex = new(
            @"^[0-9]{1,6}([a-zA-Z\u00C0-\u024F]{1,6})?(\s*[-–\/]\s*[0-9]{1,4}([a-zA-Z\u00C0-\u024F]{0,6})?)?$",
            RegexOptions.CultureInvariant | RegexOptions.Compiled);

        private static bool IsValidSwissHausnummer(string? value)
        {
            var s = (value ?? "").Trim();
            return s.Length > 0 && SwissHausnummerRegex.IsMatch(s);
        }

        private static bool IsValidSwissPhone(string? raw)
        {
            var d = Regex.Replace(raw ?? "", @"\D", "");
            if (d.Length < 9) return false;
            if (d.StartsWith("41", StringComparison.Ordinal)) return d.Length >= 11;
            if (d.StartsWith("0", StringComparison.Ordinal)) return d.Length >= 10;
            return d.Length >= 9;
        }

        public async Task<ActionResult<CartWithExtensionsDTO>> GetCart()
        {
            var visitorId = httpContextAccessor.HttpContext.Session.GetString("VisitorId");
            if (string.IsNullOrEmpty(visitorId))
            {
                return new UnauthorizedResult();
            }

            var cart = await _dbContext.ShoppingCarts.Include(sc => sc.Items)
                                                      .ThenInclude(ci => ci.Extensions)
                                                      .Include("Items.Product")
                                                      .FirstOrDefaultAsync(sc => sc.UserId == visitorId);
            var extensions = await _dbContext.ExtensionOrderItem.Where(z => z.VisitorId == visitorId).ToListAsync();

            if (cart == null)
            {
                return new NotFoundResult();
            }
            var cartWithExtensionsDTO = new CartWithExtensionsDTO
            {
                Cart = cart,
                Extensions = extensions
            };


            return cartWithExtensionsDTO;
        }


        private void ClearShoppingCartContents(ShoppingCart userCart)
        {
            foreach (var cartItem in userCart.Items.ToList())
            {
                if (cartItem.Extensions != null && cartItem.Extensions.Count > 0)
                    _dbContext.Extension.RemoveRange(cartItem.Extensions);
            }
            if (userCart.Items.Count > 0)
                _dbContext.CartItems.RemoveRange(userCart.Items);
            userCart.Items.Clear();

            if (userCart.OrderItems != null)
            {
                foreach (var oi in userCart.OrderItems.ToList())
                {
                    if (oi.ExtensionOrderItem != null && oi.ExtensionOrderItem.Count > 0)
                        _dbContext.ExtensionOrderItem.RemoveRange(oi.ExtensionOrderItem);
                }
                if (userCart.OrderItems.Count > 0)
                    _dbContext.OrderItem.RemoveRange(userCart.OrderItems);
                userCart.OrderItems.Clear();
            }
        }

        public  ActionResult<string> AddToCart(AddToCartDTO addToCartDTO)
        {

            var visitorId = httpContextAccessor.HttpContext.Session.GetString("VisitorId");
            if (string.IsNullOrEmpty(visitorId))
            {
                visitorId = Guid.NewGuid().ToString();
                httpContextAccessor.HttpContext.Session.SetString("VisitorId", visitorId);
            }

            var userCart = _dbContext.ShoppingCarts
                                         .Include(sc => sc.Items)
                                         .ThenInclude(ci => ci.Extensions)
                                         .Include(sc => sc.OrderItems)
                                          .ThenInclude(oi => oi.ExtensionOrderItem)

                                         .FirstOrDefault(sc => sc.UserId == visitorId);


            if (userCart == null)
            {
                userCart = new ShoppingCart { UserId = visitorId ,Pickup_type = "delivery" };
                _dbContext.ShoppingCarts.Add(userCart);
            }
            userCart.LastOperationTimestamp = DateTime.Now;
            userCart.Pickup_type = "delivery";

            if (addToCartDTO.ReplaceExistingItems && userCart.Items.Count > 0)
            {
                ClearShoppingCartContents(userCart);
                _dbContext.SaveChanges();
            }

            foreach (var item in addToCartDTO.Items)
            {

                    var product = _dbContext.Product.Include(p => p.Extensions).FirstOrDefault(p => p.Id == item.ProductId);
                    if (product == null)
                    {
                        return new NotFoundObjectResult($"Product with ID {item.ProductId} not found");
                    }

                    var cartItem = new CartItem
                    {
                        ProductId = item.ProductId,
                        Quantity = item.Quantity,
                        Extensions = item.Extensions?.Select(e => new Extension { Name = e.Name, Price = e.Price, CategoryId = e.CategoryId }).ToList()
                    }; userCart.Items.Add(cartItem);
                    _dbContext.CartItems.Add(cartItem);


                // Copy the item to OrderItems
                var orderItem = new OrderItem
                {
                    ProductId = item.ProductId,
                    Quantity = item.Quantity,
                    Subtotal = cartItem.Quantity * product.Price,
                    CreatedAt = DateTime.Now,
                    ExtensionOrderItem = cartItem.Extensions?.Select(e => new ExtensionOrderItem
                    {
                        Name = e.Name,
                        Price = e.Price,
                        CategoryId = e.CategoryId,
                        ProductId = item.ProductId,
                        VisitorId = visitorId
                    }).ToList()
                };
                userCart.OrderItems.Add(orderItem);

            }
            _dbContext.SaveChanges();

            return visitorId;
        }
        public ActionResult<string> Pickup_AddToCart(AddToCartDTO addToCartDTO)
        {

            var visitorId = httpContextAccessor.HttpContext.Session.GetString("VisitorId");

            if (string.IsNullOrEmpty(visitorId))
            {
                visitorId = Guid.NewGuid().ToString();
                httpContextAccessor.HttpContext.Session.SetString("VisitorId", visitorId);
              
            }

            var userCart = _dbContext.ShoppingCarts
                                         .Include(sc => sc.Items)
                                         .ThenInclude(ci => ci.Extensions)
                                         .Include(sc => sc.OrderItems)
                                          .ThenInclude(oi => oi.ExtensionOrderItem)

                                         .FirstOrDefault(sc => sc.UserId == visitorId);


            if (userCart == null)
            {
                userCart = new ShoppingCart { UserId = visitorId, Pickup_type = "Pickup" };
                _dbContext.ShoppingCarts.Add(userCart);
            }
            userCart.LastOperationTimestamp = DateTime.Now;
            userCart.Pickup_type = "Pickup";

            if (addToCartDTO.ReplaceExistingItems && userCart.Items.Count > 0)
            {
                ClearShoppingCartContents(userCart);
                _dbContext.SaveChanges();
            }

            foreach (var item in addToCartDTO.Items)
            {

                var product = _dbContext.Product.Include(p => p.Extensions).FirstOrDefault(p => p.Id == item.ProductId);
                if (product == null)
                {
                    return new NotFoundObjectResult($"Product with ID {item.ProductId} not found");
                }

                var cartItem = new CartItem
                {
                    ProductId = item.ProductId,
                    Quantity = item.Quantity,
                    Extensions = item.Extensions?.Select(e => new Extension { Name = e.Name, Price = e.Price, CategoryId = e.CategoryId }).ToList()
                }; userCart.Items.Add(cartItem);
                _dbContext.CartItems.Add(cartItem);


                // Copy the item to OrderItems
                var orderItem = new OrderItem
                {
                    ProductId = item.ProductId,
                    Quantity = item.Quantity,
                    Subtotal = cartItem.Quantity * product.Pickup_Price,
                    CreatedAt = DateTime.Now,
                    ExtensionOrderItem = cartItem.Extensions?.Select(e => new ExtensionOrderItem
                    {
                        Name = e.Name,
                        Price = e.Price,
                        CategoryId = e.CategoryId,
                        ProductId = item.ProductId,
                        VisitorId = visitorId
                    }).ToList()
                };
                userCart.OrderItems.Add(orderItem);

            }
            _dbContext.SaveChanges();

            return visitorId;
        }



        public async Task<ActionResult<CheckoutDto>> Checkout(string DiscountCode)
        {
            var visitorId = httpContextAccessor.HttpContext.Session.GetString("VisitorId");

            var userCart = _dbContext.ShoppingCarts.Include(sc => sc.Items)
                                                   .ThenInclude(ci => ci.Extensions)
                                                   .FirstOrDefault(sc => sc.UserId == visitorId);
            if (userCart == null || !userCart.Items.Any())
            {
                return new BadRequestObjectResult("Cart is empty");
            }

            decimal cartTotalNumber = userCart.Items.Sum(ci =>
            {
                var productPrice = _dbContext.Product.FirstOrDefault(p => p.Id == ci.ProductId)?.Price ?? 0;
                var extensionsPrice = ci.Extensions?.Sum(e => e.Price) ?? 0;
                return ci.Quantity * (productPrice + extensionsPrice);
            });

            var discount = await discountCodeService.GetByName(DiscountCode);
            if (!IsDiscountApplicableNow(discount))
            {
                decimal discountAmount = 0;
                decimal totalAfterDiscount = cartTotalNumber - discountAmount;

                var checkout = new CheckoutDto
                {
                    CartTotalNumber = cartTotalNumber,
                    //FinalTotalNumber = totalAfterDiscount,
                    DiscountValue = discountAmount,
                    TotalAfterDiscount = totalAfterDiscount
                };

                return checkout;
            }

            decimal previewDeduction;
            if (IsWertgutscheinRow(discount))
            {
                var balance = (decimal)discount.Value;
                previewDeduction = Math.Min(balance, cartTotalNumber);
            }
            else
            {
                previewDeduction = (cartTotalNumber * (decimal)discount.Value) / 100m;
            }

            var totalAfter = cartTotalNumber - previewDeduction;
            return new CheckoutDto
            {
                CartTotalNumber = cartTotalNumber,
                DiscountValue = previewDeduction,
                TotalAfterDiscount = totalAfter
            };
        }


        public async Task<DiscountCode?> GetActiveMenuOfferAsync()
        {
            var codes = await discountCodeService.Get();
            return codes
                .Where(code => IsProzentRabattValidToday(code))
                .OrderBy(code => string.Equals(code.Name, "wangen15", StringComparison.OrdinalIgnoreCase) ? 0 : 1)
                .ThenBy(code => code.ExpiryDate)
                .FirstOrDefault();
        }


        public async Task<ActionResult<Order>> CreateOrder(OrderDto dto)
        {
            var visitorId = dto.UserId;

            //  var picktype = dto.Pickup_type;
            if (dto.DeliveryDate == default)
            {
                var restaurantTimeZone = TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time");
                dto.DeliveryDate = TimeZoneInfo.ConvertTime(DateTime.UtcNow, restaurantTimeZone).Date;
            }
            else
            {
                dto.DeliveryDate = dto.DeliveryDate.Date;
            }
            dto.DateAdded = DateTime.UtcNow;

            var userCart = _dbContext.ShoppingCarts
                                     .Include(sc => sc.Items)
                                         .ThenInclude(ci => ci.Extensions)
                                     .Include(sc => sc.OrderItems)
                                     .Where(sc => sc.UserId == visitorId).OrderByDescending(sc => sc.Id) // Assuming DateAdded represents the cart creation time
                             .FirstOrDefault(); 

            if (userCart == null || !userCart.Items.Any())
            {
                return new BadRequestObjectResult("Cart is empty");
            }

            if (string.IsNullOrWhiteSpace(dto.Name))
                return new BadRequestObjectResult("Name fehlt.");
            if (string.IsNullOrWhiteSpace(dto.Street))
                return new BadRequestObjectResult("Strasse fehlt.");
            if (!IsValidSwissHausnummer(dto.Hausnummer))
                return new BadRequestObjectResult("Ungültige oder fehlende Hausnummer (z. B. 12, 12a, 12–14).");
            if (!IsValidSwissPhone(dto.Mobile))
                return new BadRequestObjectResult("Ungültige Telefonnummer (mind. 9 Ziffern, z. B. 076 123 45 67).");

            var streetLine = StreetAddressHelper.CombineStreetAndHausnummer(dto.Street, dto.Hausnummer);

            const int barzahlungPaymentWay = 1;
            // Bar: separaten Wertgutschein blocken — nicht denselben Text wie Rabattcode (ein Eingabefeld kann beides mitschicken).
            if (dto.PaymentWay == barzahlungPaymentWay && !string.IsNullOrWhiteSpace(dto.GutscheinCode))
            {
                var g = dto.GutscheinCode.Trim();
                var d = (dto.DiscountCode ?? string.Empty).Trim();
                if (!string.Equals(d, g, StringComparison.OrdinalIgnoreCase))
                {
                    return new BadRequestObjectResult(
                        "Wertgutscheine sind nur bei Online-Zahlung möglich. Bitte Online-Zahlung wählen oder den Gutschein-Code entfernen.");
                }
            }

            var gutscheinCodeForOrder = dto.PaymentWay == barzahlungPaymentWay ? null : dto.GutscheinCode;

            decimal totalPrice = 0;
            // Determine if discount should be canceled based on ProductType
            bool cancelDiscount = userCart.Items.Any(ci =>
                _dbContext.Product.FirstOrDefault(p => p.Id == ci.ProductId)?.ProductType == "Offer");

            if (userCart.Pickup_type == "Pickup")
            {
                totalPrice = userCart.Items.Sum(ci =>
                {
                    var productPrice = _dbContext.Product.FirstOrDefault(p => p.Id == ci.ProductId)?.Pickup_Price ?? 0;
                    var extensionsPrice = ci.Extensions?.Sum(e => e.Price) ?? 0;
                    return ci.Quantity * (productPrice + extensionsPrice);
                });

                // Mindestbestellwert gilt nur für Lieferung — nicht für Abholung.

                var (discountAmount, gutscheinDeduction, appliedGutscheinCode, codesError) =
                    await ResolveRabattUndGutscheinForOrderAsync(
                        dto.DiscountCode,
                        dto.GutscheinCode,
                        gutscheinCodeForOrder,
                        totalPrice,
                        cancelDiscount,
                        dto.PaymentWay,
                        barzahlungPaymentWay);
                if (codesError != null)
                    return codesError;

                decimal finalTotalPrice = totalPrice - discountAmount - gutscheinDeduction;

                var orderItems = userCart.OrderItems.Where(oi => oi.CreatedAt >= userCart.LastOperationTimestamp).ToList();

                var order = new Order
                {
                    UserId = dto.UserId,
                    Salute = dto.Salute,
                    TotalNumber = totalPrice,
                    FinalTotalNumber = finalTotalPrice,
                    DiscountValue = discountAmount,
                    GutscheinDeduction = gutscheinDeduction,
                    AppliedGutscheinCode = appliedGutscheinCode,
                    Name = dto.Name,
                    Street = streetLine,
                    City = dto.City,
                    PostBox = dto.PostBox,
                    DeliveryTime = dto.DeliveryTime,
                    DeliveryDate = dto.DeliveryDate,
                    Notes = dto.Notes,
                    Items = userCart.Items.ToList(),
                    OrderItems = orderItems,
                    Email = dto.Email,
                    Mobile = dto.Mobile?.Trim(),
                    Pickup_type = "Pickup",
                    PaymentWay = dto.PaymentWay
                };

                _dbContext.Orders.Add(order);
                _dbContext.SaveChanges();

                // Clear the cart after creating the order
                foreach (var cartItem in userCart.Items.ToList())
                {
                    if (cartItem.Extensions != null)
                    {
                        _dbContext.Extension.RemoveRange(cartItem.Extensions);
                    }
                }
                _dbContext.CartItems.RemoveRange(userCart.Items);
                _dbContext.SaveChanges();
                httpContextAccessor.HttpContext.Session.Remove("VisitorId");

                return order;
            }
            else if (userCart.Pickup_type == "delivery")
            {
                totalPrice = userCart.Items.Sum(ci =>
                {
                    var productPrice = _dbContext.Product.FirstOrDefault(p => p.Id == ci.ProductId)?.Price ?? 0;
                    var extensionsPrice = ci.Extensions?.Sum(e => e.Price) ?? 0;
                    return ci.Quantity * (productPrice + extensionsPrice);
                });

                var plz = NormalizeSwissPostCode(dto.PostBox, dto.City);
                if (string.IsNullOrEmpty(plz))
                {
                    return new BadRequestObjectResult("Bitte geben Sie eine gültige 4-stellige Postleitzahl an.");
                }

                var delivery = await deliveryService.GetByPostBox(plz);
                if (delivery == null)
                {
                    return new BadRequestObjectResult("Diese Postleitzahl liegt ausserhalb unseres Liefergebiets.");
                }

                dto.PostBox = plz;

                if (totalPrice < delivery.OrderAb)
                {
                    return new BadRequestObjectResult($"Lieferungen nach {dto.PostBox} sind ab einem Bestellwert von CHF {delivery.OrderAb} möglich.");
                }

                var (discountAmount, gutscheinDeduction, appliedGutscheinCode, codesError) =
                    await ResolveRabattUndGutscheinForOrderAsync(
                        dto.DiscountCode,
                        dto.GutscheinCode,
                        gutscheinCodeForOrder,
                        totalPrice,
                        cancelDiscount,
                        dto.PaymentWay,
                        barzahlungPaymentWay);
                if (codesError != null)
                    return codesError;

                decimal finalTotalPrice = totalPrice - discountAmount - gutscheinDeduction;

                var orderItems = userCart.OrderItems.Where(oi => oi.CreatedAt >= userCart.LastOperationTimestamp).ToList();

                var order = new Order
                {
                    UserId = dto.UserId,
                    Salute = dto.Salute,
                    TotalNumber = totalPrice,
                    FinalTotalNumber = finalTotalPrice,
                    DiscountValue = discountAmount,
                    GutscheinDeduction = gutscheinDeduction,
                    AppliedGutscheinCode = appliedGutscheinCode,
                    Name = dto.Name,
                    Street = streetLine,
                    City = dto.City,
                    PostBox = dto.PostBox,
                    DeliveryTime = dto.DeliveryTime,
                    DeliveryDate = dto.DeliveryDate,
                    Notes = dto.Notes,
                    Items = userCart.Items.ToList(),
                    OrderItems = orderItems,
                    Email = dto.Email,
                    Mobile = dto.Mobile?.Trim(),
                    Pickup_type = "delivery",
                    PaymentWay = dto.PaymentWay
                };

                _dbContext.Orders.Add(order);
                _dbContext.SaveChanges();

                // Clear the cart after creating the order
                foreach (var cartItem in userCart.Items.ToList())
                {
                    if (cartItem.Extensions != null)
                    {
                        _dbContext.Extension.RemoveRange(cartItem.Extensions);
                    }
                }
                _dbContext.CartItems.RemoveRange(userCart.Items);
                _dbContext.SaveChanges();
                httpContextAccessor.HttpContext.Session.Remove("VisitorId");


                return order;
            }

            return new BadRequestObjectResult("Error ");
        }

        public async Task<ActionResult<Order>> CreateWertgutscheinOrder(WertgutscheinCheckoutDto dto)
        {
            var allowed = new[] { 25m, 50m, 100m, 150m, 200m };
            if (!allowed.Contains(dto.FaceValueChf))
                return new BadRequestObjectResult("Ungültiger Gutschein-Wert.");

            var qty = dto.VoucherQuantity < 1 ? 1 : dto.VoucherQuantity;
            if (qty > 50)
                return new BadRequestObjectResult("Zu viele Gutscheine.");

            if (string.IsNullOrWhiteSpace(dto.Vorname) || string.IsNullOrWhiteSpace(dto.Nachname))
                return new BadRequestObjectResult("Name fehlt.");

            if (string.IsNullOrWhiteSpace(dto.Strasse) || string.IsNullOrWhiteSpace(dto.Hausnummer) ||
                string.IsNullOrWhiteSpace(dto.Plz) || string.IsNullOrWhiteSpace(dto.Ort) ||
                string.IsNullOrWhiteSpace(dto.Email))
                return new BadRequestObjectResult("Adresse oder E-Mail fehlt.");

            if (dto.DifferentDelivery)
            {
                if (string.IsNullOrWhiteSpace(dto.LieferVorname) || string.IsNullOrWhiteSpace(dto.LieferNachname) ||
                    string.IsNullOrWhiteSpace(dto.LieferStrasse) || string.IsNullOrWhiteSpace(dto.LieferHausnummer) ||
                    string.IsNullOrWhiteSpace(dto.LieferPlz) || string.IsNullOrWhiteSpace(dto.LieferOrt))
                    return new BadRequestObjectResult("Lieferadresse unvollständig.");
            }

            var totals = WertgutscheinPricing.ComputeTotals(dto.FaceValueChf, qty);
            var total = totals.Total;

            var billingStreet = $"{dto.Strasse.Trim()} {dto.Hausnummer.Trim()}".Trim();
            var billingPlz = dto.Plz.Trim();
            var billingCity = dto.Ort.Trim();

            var shipStreet = dto.DifferentDelivery
                ? $"{dto.LieferStrasse!.Trim()} {dto.LieferHausnummer!.Trim()}".Trim()
                : billingStreet;
            var shipPlz = dto.DifferentDelivery ? dto.LieferPlz!.Trim() : billingPlz;
            var shipCity = dto.DifferentDelivery ? dto.LieferOrt!.Trim() : billingCity;

            var visitorId = httpContextAccessor.HttpContext?.Session.GetString("VisitorId");

            var notes = $"WERTGUTSCHEIN|Nominal CHF {dto.FaceValueChf:F0} × {qty}|Bearbeitung netto CHF {totals.FeeNet:F2} · MwSt CHF {totals.Mwst:F2}|Rechnung: {billingStreet}, {billingPlz} {billingCity}";
            if (dto.DifferentDelivery)
                notes += $"|Post an: {shipStreet}, {shipPlz} {shipCity}";
            if (!string.IsNullOrWhiteSpace(dto.Firma))
                notes = $"Firma: {dto.Firma.Trim()}|{notes}";
            if (!string.IsNullOrWhiteSpace(dto.Bemerkungen))
                notes += $"|Bemerkung: {dto.Bemerkungen.Trim()}";

            var order = new Order
            {
                UserId = visitorId,
                Salute = string.IsNullOrWhiteSpace(dto.Salute) ? null : dto.Salute.Trim(),
                Name = $"{dto.Vorname.Trim()} {dto.Nachname.Trim()}".Trim(),
                Street = shipStreet,
                PostBox = shipPlz,
                City = shipCity,
                Email = dto.Email.Trim(),
                Mobile = string.IsNullOrWhiteSpace(dto.Telefon) ? null : dto.Telefon.Trim(),
                TotalNumber = total,
                DiscountValue = 0,
                FinalTotalNumber = total,
                Pickup_type = "voucher",
                PaymentWay = 2,
                DeliveryDate = DateTime.Now,
                DeliveryTime = "—",
                Notes = notes,
                OrderItems = new List<OrderItem>(),
                Verified = false,
                IsPaymentSucceeded = false
            };

            _dbContext.Orders.Add(order);
            await _dbContext.SaveChangesAsync();

            return order;
        }

        public async Task<IEnumerable<Order>> GetAllOrders()
        {
            return await _dbContext.Orders.OrderByDescending(o=>o.Id).ToListAsync();
        }
        public async Task<IEnumerable<Order>> GetAllSucceededOrders()
        {
            return await _dbContext.Orders.Where(a=>a.IsPaymentSucceeded==true || a.PaymentWay==1).OrderByDescending(o => o.Id).ToListAsync();
        }

        public async Task<CartItem> GetCartItemById(int id)
        {
            return await _dbContext.CartItems.FirstOrDefaultAsync(a => a.Id == id  );
        }

        public void DeleteCartItem(int cartItemId)
        {
            var data = _dbContext.CartItems.Where(a => a.Id == cartItemId).FirstOrDefault();
            _dbContext.Remove(data);
            _dbContext.SaveChanges();
        }
        public void UpdateOrder(Order order)
        {
            _dbContext.Entry(order).State = EntityState.Modified;
            _dbContext.SaveChanges();
        }
        public async Task<Order> GetOrderById(int id)
        {
            return await _dbContext.Orders.Include("OrderItems").Include("OrderItems.Product").FirstOrDefaultAsync(a => a.Id == id);
        }
        public async Task<Order> GetOrderItemById(int id)
        {
            return await _dbContext.Orders
                   .Include(o => o.OrderItems)
                       .ThenInclude(oi => oi.Product)
                           .ThenInclude(p => p!.SubCategory)
                   .Include(o => o.OrderItems)
                       .ThenInclude(oi => oi.ExtensionOrderItem)
                   .FirstOrDefaultAsync(o => o.Id == id);
        }

        public void DeleteOrder(Order order)
        {
            _dbContext.Entry(order).State = EntityState.Deleted;
            _dbContext.SaveChanges();
        }


        //public async Task<IEnumerable<OrderItem>> GetOrderItemByOrderId(int id)
        //{
        //    return await _dbContext.OrderItem.Where(a => a.O == id).ToListAsync();
        //}
        //public IEnumerable<OrderItem> GetOrderItemByOrderId2(int id)
        //{
        //    return  _dbContext.OrderItem.Where(a=>a.Id ==id).ToList();
        //}

        private static string? TrimOrderCode(string? s)
        {
            var t = (s ?? string.Empty).Trim();
            return t.Length == 0 ? null : t;
        }

        /// <summary>
        /// Ausgestellte Wertgutschein-Codes beginnen mit «WPV-»; ältere DB-Zeilen können <see cref="DiscountCode.IsWertgutschein"/> noch false haben.
        /// </summary>
        private static bool IsWertgutscheinRow(DiscountCode code) =>
            code != null &&
            (code.IsWertgutschein ||
             (code.Name ?? "").StartsWith("WPV-", StringComparison.OrdinalIgnoreCase));

        private static DateTime RestaurantLocalToday()
        {
            var restaurantTimeZone = TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time");
            return TimeZoneInfo.ConvertTime(DateTime.UtcNow, restaurantTimeZone).Date;
        }

        /// <summary>
        /// Prozent-Rabatt (Menü-Aktion): <see cref="DiscountCode.ExpiryDate"/> = Spieltag, nur an diesem Kalendertag gültig.
        /// </summary>
        private static bool IsProzentRabattValidToday(DiscountCode code)
        {
            if (code == null || !code.IsActive || IsWertgutscheinRow(code) || code.Value <= 0)
                return false;

            return code.ExpiryDate.Date == RestaurantLocalToday();
        }

        private static bool IsDiscountApplicableNow(DiscountCode discount)
        {
            if (discount == null || !discount.IsActive)
                return false;

            if (IsWertgutscheinRow(discount))
                return discount.ExpiryDate >= DateTime.Now;

            return IsProzentRabattValidToday(discount);
        }

        /// <summary>
        /// Ein gemeinsames Eingabefeld (Rabattcode / Wertgutschein): gleicher Text in beiden DTO-Feldern oder nur Rabattcode.
        /// Zwei unterschiedliche Codes: bisheriges Verhalten (Rabatt % + separater Gutschein).
        /// </summary>
        private async Task<(decimal discountAmount, decimal gutscheinDeduction, string? appliedGutscheinCode, ActionResult<Order>? error)>
            ResolveRabattUndGutscheinForOrderAsync(
                string? discountCodeRaw,
                string? gutscheinCodeRaw,
                string? gutscheinCodeForOrder,
                decimal totalPrice,
                bool cancelDiscount,
                int paymentWay,
                int barzahlungPaymentWay)
        {
            var d = TrimOrderCode(discountCodeRaw);
            var g = TrimOrderCode(gutscheinCodeRaw);
            var twoDifferentCodes =
                d != null && g != null &&
                !string.Equals(d, g, StringComparison.OrdinalIgnoreCase);

            decimal discountAmount = 0m;
            const decimal zero = 0m;
            string? appliedGutscheinCode = null;

            if (twoDifferentCodes)
            {
                if (!cancelDiscount && d != null)
                {
                    var disc = await discountCodeService.GetByName(d);
                    if (IsProzentRabattValidToday(disc))
                        discountAmount = (totalPrice * (decimal)disc!.Value) / 100m;
                }

                if (string.IsNullOrWhiteSpace(gutscheinCodeForOrder))
                    return (discountAmount, zero, null, null);

                var gPay = TrimOrderCode(gutscheinCodeForOrder);
                if (gPay == null)
                    return (discountAmount, zero, null, null);

                try
                {
                    var (deduction, applied) = await ResolveWertgutscheinDeductionOrFailAsync(
                        gPay, totalPrice, discountAmount, cancelDiscount);
                    return (discountAmount, deduction, applied, null);
                }
                catch (InvalidOperationException ex)
                {
                    return (zero, zero, null, new BadRequestObjectResult(ex.Message));
                }
            }

            var unified = d ?? g;
            if (unified == null)
                return (zero, zero, null, null);

            var resolved = await discountCodeService.GetByName(unified);
            if (!IsDiscountApplicableNow(resolved))
                return (zero, zero, null, null);

            if (IsWertgutscheinRow(resolved))
            {
                if (paymentWay == barzahlungPaymentWay)
                {
                    return (zero, zero, null, new BadRequestObjectResult(
                        "Wertgutscheine sind nur bei Online-Zahlung möglich. Bitte Online-Zahlung wählen oder den Code entfernen."));
                }

                try
                {
                    var (deduction, applied) = await ResolveWertgutscheinDeductionOrFailAsync(
                        unified, totalPrice, 0m, cancelDiscount);
                    return (zero, deduction, applied, null);
                }
                catch (InvalidOperationException ex)
                {
                    return (zero, zero, null, new BadRequestObjectResult(ex.Message));
                }
            }

            if (!cancelDiscount)
                discountAmount = (totalPrice * (decimal)resolved.Value) / 100m;

            return (discountAmount, zero, null, null);
        }

        /// <summary>
        /// Wertgutschein aus «Bonus verwalten»: <see cref="DiscountCode.Value"/> = verbleibendes CHF-Guthaben (nicht Prozent).
        /// Rabatt-Prozent bleibt im Feld <c>DiscountCode</c> / Rabattcode.
        /// </summary>
        private async Task<(decimal deductionChf, string? appliedName)> ResolveWertgutscheinDeductionOrFailAsync(
            string? gutscheinCode,
            decimal totalPrice,
            decimal rabattChf,
            bool cancelCodes)
        {
            if (cancelCodes || string.IsNullOrWhiteSpace(gutscheinCode))
                return (0m, null);

            var gc = await discountCodeService.GetByName(gutscheinCode.Trim());
            if (gc == null || gc.ExpiryDate < DateTime.Now)
                throw new InvalidOperationException("Ungültiger oder abgelaufener Gutschein-Code.");
            if (!gc.IsActive)
                throw new InvalidOperationException("Gutschein-Code ist deaktiviert.");

            var balance = (decimal)gc.Value;
            if (balance < 0.01m)
                throw new InvalidOperationException("Gutschein-Guthaben ist aufgebraucht.");

            var afterRabatt = Math.Max(0m, totalPrice - rabattChf);
            var use = Math.Min(balance, afterRabatt);
            if (afterRabatt >= 0.01m && use < 0.005m)
                throw new InvalidOperationException("Gutschein-Guthaben reicht für diese Bestellung nicht.");

            return (use, gc.Name);
        }

        public async Task ConsumeAppliedGutscheinAfterPaymentAsync(int orderId)
        {
            var order = await _dbContext.Orders.AsNoTracking().FirstOrDefaultAsync(o => o.Id == orderId);
            if (order == null || order.GutscheinDeduction <= 0m || string.IsNullOrWhiteSpace(order.AppliedGutscheinCode))
                return;

            var code = await discountCodeService.GetByName(order.AppliedGutscheinCode);
            if (code == null) return;

            var remaining = (decimal)code.Value - order.GutscheinDeduction;
            if (remaining < 0m) remaining = 0m;
            code.Value = (double)remaining;
            discountCodeService.Update(code);
            await Task.CompletedTask;
        }

        private static bool IsPurchasedWertgutscheinOrder(Order order) =>
            string.Equals(order.Pickup_type, "voucher", StringComparison.OrdinalIgnoreCase) ||
            (!string.IsNullOrWhiteSpace(order.Notes) &&
             order.Notes.Contains("WERTGUTSCHEIN", StringComparison.OrdinalIgnoreCase));

        private static bool TryParseWertgutscheinPurchase(string? notes, out decimal faceValueChf, out int qty)
        {
            faceValueChf = 0;
            qty = 0;
            if (string.IsNullOrWhiteSpace(notes))
                return false;

            // Optional «× Anzahl» — fehlt die Menge, wird 1 angenommen (ältere / manuelle Notizen).
            var m = Regex.Match(
                notes,
                @"Nominal\s+CHF\s+(\d+(?:[.,]\d+)?)(?:\s*[×x]\s*(\d+))?",
                RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
            if (!m.Success)
                return false;

            if (!decimal.TryParse(
                    m.Groups[1].Value.Replace(',', '.'),
                    System.Globalization.NumberStyles.Any,
                    System.Globalization.CultureInfo.InvariantCulture,
                    out faceValueChf))
                return false;

            qty = 1;
            if (m.Groups[2].Success &&
                int.TryParse(
                    m.Groups[2].Value,
                    System.Globalization.NumberStyles.Integer,
                    System.Globalization.CultureInfo.InvariantCulture,
                    out var parsedQty) &&
                parsedQty >= 1)
                qty = parsedQty;

            return true;
        }

        /// <inheritdoc />
        public async Task IssuePurchasedWertgutscheinCodesIfNeededAsync(int orderId)
        {
            var order = await _dbContext.Orders.FirstOrDefaultAsync(o => o.Id == orderId);
            if (order == null)
                return;
            if (!IsPurchasedWertgutscheinOrder(order))
                return;
            if (!string.IsNullOrWhiteSpace(order.IssuedVoucherCodes))
                return;
            if (!TryParseWertgutscheinPurchase(order.Notes, out var faceValueChf, out var qty))
                return;

            if (qty > 50)
                qty = 50;

            var codes = new List<string>(qty);
            for (var i = 0; i < qty; i++)
            {
                string code = "";
                for (var attempt = 0; attempt < 80; attempt++)
                {
                    var rnd = new byte[5];
                    RandomNumberGenerator.Fill(rnd);
                    code = $"WPV-{orderId}-{(i + 1)}-{Convert.ToHexString(rnd)}";
                    if (await discountCodeService.GetByName(code) == null && !codes.Contains(code))
                        break;
                }

                if (await discountCodeService.GetByName(code) != null || codes.Contains(code))
                    throw new InvalidOperationException("Eindeutigen Gutscheincode konnte nicht erzeugt werden.");

                await discountCodeService.Create(new DiscountCode
                {
                    Name = code,
                    Value = (double)faceValueChf,
                    ExpiryDate = DateTime.UtcNow.AddYears(3),
                    IsWertgutschein = true,
                    IsActive = true,
                    CreatedAtUtc = DateTime.UtcNow,
                    OriginalValueChf = faceValueChf,
                    Note = $"Webkauf Order #{order.Id}",
                });
                codes.Add(code);
            }

            order.IssuedVoucherCodes = string.Join(',', codes);
            UpdateOrder(order);
        }
    }
}
