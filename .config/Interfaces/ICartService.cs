using Microsoft.AspNetCore.Mvc;
using WangenPizza.Dtos;
using WangenPizza.Models;

namespace WangenPizza.Interfaces
{
    public interface ICartService
    {
        Task<ActionResult<CartWithExtensionsDTO>> GetCart();
        ActionResult<string> AddToCart( AddToCartDTO addToCartDTO);
        ActionResult<string> Pickup_AddToCart( AddToCartDTO addToCartDTO);
        Task<ActionResult<Order>> CreateOrder(OrderDto dto);
        Task<ActionResult<Order>> CreateWertgutscheinOrder(WertgutscheinCheckoutDto dto);
        Task<ActionResult<CheckoutDto>> Checkout(string DiscountCode);
        Task<IEnumerable<Order>> GetAllOrders();
        Task<IEnumerable<Order>> GetAllSucceededOrders();

        Task<Order> GetOrderById(int id);
        void DeleteOrder(Order order);
        Task<Order> GetOrderItemById(int id);
        Task<CartItem> GetCartItemById(int id);
       // Task<IEnumerable<OrderItem>> GetOrderItemByOrderId(int id);
      //  IEnumerable<OrderItem> GetOrderItemByOrderId2(int id);

        void DeleteCartItem(int cartItemId);
        void UpdateOrder(Order order);
        /// <summary>Nach erfolgreicher Zahlung: Wertgutschein-Restbetrag (DiscountCode.Value) um genutzten Betrag reduzieren.</summary>
        Task ConsumeAppliedGutscheinAfterPaymentAsync(int orderId);
      }
}
