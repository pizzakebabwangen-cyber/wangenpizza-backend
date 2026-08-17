using WangenPizza.Models;

namespace WangenPizza.Interfaces
{
    public interface IContactService
    {
        Task<Contact> Create(Contact Contact);
        void Update(Contact Contact);
        void Delete(Contact Contact);
        Task<Contact> GetById(int id);
        Task<Contact> GetByEmail(string email);
        Task<IEnumerable<Contact>> Get();
    }
}
