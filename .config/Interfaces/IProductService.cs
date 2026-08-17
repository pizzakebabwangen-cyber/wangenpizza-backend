using WangenPizza.Models;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace WangenPizza.Interfaces
{
    public interface IProductService
    {
        Task<Product> Create(Product product);
        void Update(Product product);
        void Delete(int id);
        Task<Product> GetById(int id);
        Task<IEnumerable<Product>> Get();
        Task<IEnumerable<Product>> GetOffers();
        Task<IEnumerable<Product>> GetTodayBonus();
        Task<IEnumerable<Product>> GetProductsToHome();
        Task<IEnumerable<Product>> GetByCatgoryId(int categoryId);
        Task<IEnumerable<Product>> GetBySubCatgoryId(int subCategoryId);

    }
}
