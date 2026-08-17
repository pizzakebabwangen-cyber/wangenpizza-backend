using WangenPizza.Models;

namespace WangenPizza.Interfaces
{
    public interface IEmailTextService
    {
        Task<EmailText> Create(EmailText EmailText);
        void Update(EmailText EmailText);
        void Delete(EmailText EmailText);
        Task<EmailText> GetById(int id);
        Task<IEnumerable<EmailText>> Get();

    }
}
