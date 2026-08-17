using WangenPizza.Models;

namespace WangenPizza.Interfaces
{
    public interface ITempOrderService
    {
        Task StoreOrderAsync(string token, Order Order);
        Task<Order> GetOrderByTokenAsync(string token);
    }
}
