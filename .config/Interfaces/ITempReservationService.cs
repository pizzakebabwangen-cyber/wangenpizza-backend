using WangenPizza.Models;

namespace WangenPizza.Interfaces
{
    public interface ITempReservationService
    {
        Task StoreReservationAsync(string token, Reservation reservation);
        Task<Reservation> GetReservationByTokenAsync(string token);
    }
}
