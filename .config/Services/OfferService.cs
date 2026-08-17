using Microsoft.EntityFrameworkCore;
using WangenPizza.Context;
using WangenPizza.Interfaces;
using WangenPizza.Models;

namespace WangenPizza.Services
{
    public class OfferService : IOfferService
    {
        private readonly ApplicationDbContext context;
        public OfferService(ApplicationDbContext context)
        {
            this.context = context;
        }

        public async Task<Product> Create(Product Product)
        {
            await context.Product.AddAsync(Product);
            context.SaveChanges();
            return Product;
        }



        public void Delete(Product Product)
        {
            context.Entry(Product).State = EntityState.Deleted;

            context.SaveChanges();

        }

        public async Task<IEnumerable<Product>> Get()
        {
            return await context.Product.ToListAsync();
        }
      

        public async Task<Product> GetById(int id)
        {
            return await context.Product.SingleOrDefaultAsync(a => a.Id == id);
        }



        public void Update(Product Product)
        {
            context.Entry(Product).State = EntityState.Modified;


            context.SaveChanges();

        }
    }
}