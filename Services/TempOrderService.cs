using WangenPizza.Interfaces;
using WangenPizza.Models;

namespace WangenPizza.Services
{
    public class TempOrderService:ITempOrderService
    {
        private readonly Dictionary<string, Order> _orders = new Dictionary<string, Order>();

        public Task StoreOrderAsync(string token, Order reservation)
        {
            _orders[token] = reservation;
            return Task.CompletedTask;
        }

        public Task<Order> GetOrderByTokenAsync(string token)
        {
            _orders.TryGetValue(token, out var reservation);
            return Task.FromResult(reservation);
        }
    }
}
