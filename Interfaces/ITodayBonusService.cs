using WangenPizza.Models;

namespace WangenPizza.Interfaces
{
    public interface ITodayBonusService
    {
        Task<Product> Create(Product Product);
        void Update(Product Product);
        void Delete(Product Product);
        Task<Product> GetById(int id);
        Task<IEnumerable<Product>> Get();
    }
}
