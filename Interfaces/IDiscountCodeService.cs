using WangenPizza.Models;

namespace WangenPizza.Interfaces
{
    public interface IDiscountCodeService
    {
        Task<DiscountCode> Create(DiscountCode discountCode);
        void Update(DiscountCode discountCode);
        void Delete(DiscountCode discountCode);
        Task<DiscountCode> GetById(int id);
        Task<DiscountCode> GetByName(string name);
        Task<IEnumerable<DiscountCode>> Get();
    }
}
