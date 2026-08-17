using WangenPizza.Dtos;

namespace WangenPizza.Interfaces
{
    public interface IpaymentService
    {
        Task<string> CreatePaymentIntentAsync(int amountInCents, string currency);
        Task<bool> ConfirmPaymentAsync(string paymentIntentId, PaymentRequest cardPaymentRequest);


    }
}
