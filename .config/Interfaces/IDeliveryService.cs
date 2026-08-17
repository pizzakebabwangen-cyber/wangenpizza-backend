using WangenPizza.Models;

namespace WangenPizza.Interfaces
{
    public interface IDeliveryService
    {
        Task<Delivery> Create(Delivery delivery);
        void Update(Delivery delivery);
        void Delete(Delivery delivery);
        Task<Delivery> GetById(int id);
        Task<Delivery> GetByPostBox(string postbox);
        Task<IEnumerable<Delivery>> Get();

    }
}
