using Microsoft.EntityFrameworkCore;
using WangenPizza.Context;
using WangenPizza.Interfaces;
using WangenPizza.Models;

namespace WangenPizza.Services
{
    public class DeliveryService: IDeliveryService
    {
        private readonly ApplicationDbContext context;

        public DeliveryService(ApplicationDbContext context)
        {
            this.context = context;
        }


        public async Task<Delivery> Create(Delivery Delivery)
        {
            await context.Delivery.AddAsync(Delivery);
            context.SaveChanges();
            return Delivery;
        }



        public void Delete(Delivery Delivery)
        {
            context.Entry(Delivery).State = EntityState.Deleted;
            context.SaveChanges();

        }



        public async Task<IEnumerable<Delivery>> Get()
        {
            return await context.Delivery.ToListAsync();
        }

        public async Task<Delivery> GetById(int id)
        {
            return await context.Delivery.SingleOrDefaultAsync(a => a.Id == id);
        }

        public async Task<Delivery> GetByPostBox(string postbox)
        {
            return await context.Delivery.FirstOrDefaultAsync(a => a.PostBox == postbox);
        }

        public void Update(Delivery Delivery)
        {
            context.Entry(Delivery).State = EntityState.Modified;
            context.SaveChanges();

        }
    }
}
