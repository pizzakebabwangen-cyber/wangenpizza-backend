using WangenPizza.Models;

namespace WangenPizza.Interfaces
{
    public class OrderPaymentCompletionResult
    {
        public bool NotFound { get; set; }
        public bool AlreadyProcessed { get; set; }
        public Order? Order { get; set; }
    }

    public interface IOrderPaymentCompletionService
    {
        /// <summary>
        /// Markiert Zahlung als erfolgreich, SignalR, E-Mail nur ans Restaurant (Kunden-Mail erst nach POS «Akzeptieren»).
        /// </summary>
        Task<OrderPaymentCompletionResult> CompleteSuccessfulPaymentAsync(int orderId);
    }
}
