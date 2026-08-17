using WangenPizza.Models;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace WangenPizza.Interfaces
{
    public interface ISubCategoryService
    {
        Task<SubCategory> Create(SubCategory subCategory);
        void Update(SubCategory subCategory);
        void Delete(SubCategory subCategory);
        Task<SubCategory> GetById(int id);
        Task<IEnumerable<SubCategory>> Get();
        Task<IEnumerable<SubCategory>> GetByCategoryId(int id);

    }
}
