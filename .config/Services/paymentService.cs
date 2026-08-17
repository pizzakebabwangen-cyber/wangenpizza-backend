using Microsoft.AspNetCore.Mvc;
using Stripe;
using WangenPizza.Dtos;
using WangenPizza.Interfaces;

namespace WangenPizza.Services
{
    public class paymentService : IpaymentService
    {
        private readonly IConfiguration configuration;

        public paymentService(IConfiguration configuration)
        {
            this.configuration = configuration;
        }

        public async Task<string> CreatePaymentIntentAsync(int amountInCents, string currency)
        {
            StripeConfiguration.ApiKey = configuration["StripeSettings:SecretKey"]; ;

            var options = new PaymentIntentCreateOptions
            {
                Amount = amountInCents,
                Currency = currency,
                PaymentMethodTypes = new List<string> { "card" }
            };

            var service = new PaymentIntentService();
            var paymentIntent = await service.CreateAsync(options);

            return paymentIntent.Id;
        }


        public async Task<bool> ConfirmPaymentAsync(string paymentIntentId, PaymentRequest cardPaymentRequest)
        {
            StripeConfiguration.ApiKey = configuration["StripeSettings:SecretKey"];

            // Create PaymentMethod using the provided Visa card details
            var paymentMethodOptions = new PaymentMethodCreateOptions
            {
                Type = "card",
                Card = new PaymentMethodCardOptions
                {
                    Number = cardPaymentRequest.CardNumber,
                    ExpMonth = cardPaymentRequest.ExpMonth,
                    ExpYear = cardPaymentRequest.ExpYear,
                    Cvc = cardPaymentRequest.Cvc,
                },
            };

            var paymentMethodService = new PaymentMethodService();
            var paymentMethod = await paymentMethodService.CreateAsync(paymentMethodOptions);

            // Confirm PaymentIntent with the created PaymentMethod
            var confirmOptions = new PaymentIntentConfirmOptions
            {
                PaymentMethod = paymentMethod.Id
            };

            var confirmService = new PaymentIntentService();

            try
            {
                var paymentIntent = await confirmService.ConfirmAsync(paymentIntentId, confirmOptions);
                return paymentIntent.Status == "succeeded";
            }
            catch (StripeException ex)
            {
                // Log or handle the exception appropriately
                Console.WriteLine($"Error confirming payment: {ex.Message}");
                return false;
            }
        }


    }
}
 