using Microsoft.EntityFrameworkCore;
using WangenPizza.Context;
using WangenPizza.Interfaces;
using WangenPizza.Models;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace WangenPizza.Services
{
    public class CompanyService:ICompanyService
    {
        private readonly ApplicationDbContext context;

        public CompanyService(ApplicationDbContext context)
        {
            this.context = context;
        }


        public async Task<CompanyData> Create(CompanyData CompanyData)
        {
            await context.CompanyData.AddAsync(CompanyData);
            context.SaveChanges();
            return CompanyData;
        }
        public async Task UpdateWithdays(CompanyData companyData)
        {
            context.Entry(companyData).State = EntityState.Modified;
            await context.SaveChangesAsync();
        }


        public void Delete(CompanyData CompanyData)
        {
            context.Entry(CompanyData).State = EntityState.Deleted;
            context.SaveChanges();

        }

      

        public async Task<IEnumerable<CompanyData>> Get()
        {
            return await context.CompanyData.ToListAsync();
        }

        public async Task<CompanyData> GetById(int id)
        {
            return await context.CompanyData
                  
                .SingleOrDefaultAsync(a => a.Id == id);
        }

      

        public void Update(CompanyData CompanyData)
        {
            context.Entry(CompanyData).State = EntityState.Modified;
            context.SaveChanges();

        }
      



    }
}
