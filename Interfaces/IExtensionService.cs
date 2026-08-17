using WangenPizza.Models;

namespace WangenPizza.Interfaces
{
    public interface IExtensionService
    {
        Task<Extension> Create(Extension Extension);
        void Update(Extension Extension);
        void Delete(Extension Extension);
        Task<Extension> GetById(int id);
        Task<Extension> GetByName(string name);
        Task<IEnumerable<Extension>> Get();
        Task<IEnumerable<Extension>> GetByCategoryId(int categoryId);
    }
}
