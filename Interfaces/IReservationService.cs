using WangenPizza.Models;

namespace WangenPizza.Interfaces
{
    public interface IReservationService
    {
        Task<Reservation> Create(Reservation Reservation);
        void Update(Reservation Reservation);
        void Delete(Reservation Reservation);
        Task<Reservation> GetById(int id);
        Task<IEnumerable<Reservation>> Get();
    }
}
