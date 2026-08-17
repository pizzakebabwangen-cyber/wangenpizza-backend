using Microsoft.EntityFrameworkCore;
using WangenPizza.Context;
using WangenPizza.Interfaces;
using WangenPizza.Models;

namespace WangenPizza.Services
{
    public class DiscountCodeService: IDiscountCodeService
    {
        private readonly ApplicationDbContext context;
        public DiscountCodeService(ApplicationDbContext context)
        {
            this.context = context;
        }

        public async Task<DiscountCode> Create(DiscountCode DiscountCode)
        {
            await context.DiscountCode.AddAsync(DiscountCode);
            context.SaveChanges();
            return DiscountCode;
        }



        public void Delete(DiscountCode DiscountCode)
        {
            context.Entry(DiscountCode).State = EntityState.Deleted;

            context.SaveChanges();

        }



        public async Task<IEnumerable<DiscountCode>> Get()
        {
            return await context.DiscountCode.ToListAsync();
        }

        public async Task<DiscountCode> GetById(int id)
        {
            return await context.DiscountCode.SingleOrDefaultAsync(a => a.Id == id);
        }
        public async Task<DiscountCode> GetByName(string name)
        {
            return await context.DiscountCode.SingleOrDefaultAsync(a => a.Name == name);
        }



        public void Update(DiscountCode DiscountCode)
        {
            context.Entry(DiscountCode).State = EntityState.Modified;

            context.SaveChanges();

        }
    }
}
