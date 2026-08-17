using WangenPizza.Models;

namespace WangenPizza.Interfaces
{
    public interface ICompanyService
    {
        Task<CompanyData> Create(CompanyData company);
        void Update(CompanyData company);
        Task UpdateWithdays(CompanyData company);

        void Delete(CompanyData company);
        Task<CompanyData> GetById(int id);
        Task<IEnumerable<CompanyData>> Get();
 


    }
}
