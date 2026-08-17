using WangenPizza.Interfaces;
using WangenPizza.Models;

namespace WangenPizza.Services
{
    public class TempReservationService: ITempReservationService
    {
        private readonly Dictionary<string, Reservation> _reservations = new Dictionary<string, Reservation>();

        public Task StoreReservationAsync(string token, Reservation reservation)
        {
            _reservations[token] = reservation;
            return Task.CompletedTask;
        }

        public Task<Reservation> GetReservationByTokenAsync(string token)
        {
            _reservations.TryGetValue(token, out var reservation);
            return Task.FromResult(reservation);
        }
    }
}
