using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System;
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
            if (discount == null || discount.ExpiryDate < DateTime.Now)
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
            else
            {
                double discountPercentage = discount.Value;
                decimal discountAmount = (cartTotalNumber * (decimal)discountPercentage) / 100;
                decimal totalAfterDiscount = cartTotalNumber - discountAmount;

                var checkout = new CheckoutDto
                {
                    CartTotalNumber = cartTotalNumber,
                  //  FinalTotalNumber = totalAfterDiscount,
                    DiscountValue = discountAmount,
                    TotalAfterDiscount = totalAfterDiscount
                };

                return checkout;
            }
        }



        public async Task<ActionResult<Order>> CreateOrder(OrderDto dto)
        {
            var visitorId = dto.UserId;

            //  var picktype = dto.Pickup_type;
            dto.DeliveryDate = DateTime.Now;
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

            const int barzahlungPaymentWay = 1;
            if (dto.PaymentWay == barzahlungPaymentWay && !string.IsNullOrWhiteSpace(dto.GutscheinCode))
            {
                return new BadRequestObjectResult(
                    "Wertgutscheine sind nur bei Online-Zahlung möglich. Bitte Online-Zahlung wählen oder den Gutschein-Code entfernen.");
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

                decimal discountAmount = 0;
                if (!cancelDiscount) // Apply discount only if no "Offer" product exists
                {
                    var discount = await discountCodeService.GetByName(dto.DiscountCode);
                    if (discount != null && discount.ExpiryDate >= DateTime.Now)
                    {
                        discountAmount = (totalPrice * (decimal)discount.Value) / 100;
                    }
                }

                decimal gutscheinDeduction = 0;
                string? appliedGutscheinCode = null;
                try
                {
                    (gutscheinDeduction, appliedGutscheinCode) = await ResolveWertgutscheinDeductionOrFailAsync(
                        gutscheinCodeForOrder, totalPrice, discountAmount, cancelDiscount);
                }
                catch (InvalidOperationException ex)
                {
                    return new BadRequestObjectResult(ex.Message);
                }

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
                    Street = dto.Street,
                    City = dto.City,
                    PostBox = dto.PostBox,
                    DeliveryTime = dto.DeliveryTime,
                    DeliveryDate = dto.DeliveryDate,
                    Notes = dto.Notes,
                    Items = userCart.Items.ToList(),
                    OrderItems = orderItems,
                    Email = dto.Email,
                    Mobile = dto.Mobile,
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

                var delivery = await deliveryService.GetByPostBox(dto.PostBox);
                if (delivery != null && totalPrice < delivery.OrderAb)
                {
                    return new BadRequestObjectResult($"Lieferungen nach {dto.PostBox} sind ab einem Bestellwert von CHF {delivery.OrderAb} möglich.");
                }

                var discount = await discountCodeService.GetByName(dto.DiscountCode);
                decimal discountAmount = 0;
                if (!cancelDiscount) // Apply discount only if no "Offer" product exists
                {
                    if (discount != null && discount.ExpiryDate >= DateTime.Now)
                    {
                        discountAmount = (totalPrice * (decimal)discount.Value) / 100;
                    }
                }

                decimal gutscheinDeduction = 0;
                string? appliedGutscheinCode = null;
                try
                {
                    (gutscheinDeduction, appliedGutscheinCode) = await ResolveWertgutscheinDeductionOrFailAsync(
                        gutscheinCodeForOrder, totalPrice, discountAmount, cancelDiscount);
                }
                catch (InvalidOperationException ex)
                {
                    return new BadRequestObjectResult(ex.Message);
                }

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
                    Street = dto.Street,
                    City = dto.City,
                    PostBox = dto.PostBox,
                    DeliveryTime = dto.DeliveryTime,
                    DeliveryDate = dto.DeliveryDate,
                    Notes = dto.Notes,
                    Items = userCart.Items.ToList(),
                    OrderItems = orderItems,
                    Email = dto.Email,
                    Mobile = dto.Mobile,
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
    }
}
