using WangenPizza.Models;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace WangenPizza.Interfaces
{
    public interface ICategoryService
    {
        Task<Category> Create(Category category);
        void Update(Category category);
        void Delete(Category category);
        Task<Category> GetById(int id);
        Task<IEnumerable<Category>> Get();
    }
}
