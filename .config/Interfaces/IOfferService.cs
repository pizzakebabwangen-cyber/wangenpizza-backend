using WangenPizza.Dtos;
using WangenPizza.Models;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace WangenPizza.Interfaces
{
    public interface IOfferService
    {
        Task<Product> Create(Product Product);
        void Update(Product Product);
        void Delete(Product Product);
        Task<Product> GetById(int id);
        Task<IEnumerable<Product>> Get();


    }
}
