using Microsoft.EntityFrameworkCore;
using WangenPizza.Context;
using WangenPizza.Interfaces;
using WangenPizza.Models;

namespace WangenPizza.Services
{
    public class ReservationService: IReservationService
    {
        private readonly ApplicationDbContext context;
        public ReservationService(ApplicationDbContext context)
        {
            this.context = context;
        }

        public async Task<Reservation> Create(Reservation Reservation)
        {
            await context.Reservation.AddAsync(Reservation);
            context.SaveChanges();
            return Reservation;
        }



        public void Delete(Reservation Reservation)
        {
            context.Entry(Reservation).State = EntityState.Deleted;
            context.SaveChanges();

        }



        public async Task<IEnumerable<Reservation>> Get()
        {
            return await context.Reservation.OrderByDescending(o => o.Id).ToListAsync();
        }

        public async Task<Reservation> GetById(int id)
        {
            return await context.Reservation.SingleOrDefaultAsync(a => a.Id == id);
        }



        public void Update(Reservation Reservation)
        {
            context.Entry(Reservation).State = EntityState.Modified;

            context.SaveChanges();

        }
    }
}
