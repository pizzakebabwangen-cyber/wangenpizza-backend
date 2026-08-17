using Stripe;
using WangenPizza.Context;
using WangenPizza.Dtos;
using WangenPizza.Interfaces;
using WangenPizza.Models;

namespace WangenPizza.Services
{
	public class StripeService
	{
		private readonly string _stripeSecretKey;
        private readonly IConfiguration configuration;
        private readonly ApplicationDbContext context;
        private readonly ICartService cartService;

        public StripeService(IConfiguration configuration , ApplicationDbContext context , ICartService cartService)
		{
			_stripeSecretKey = configuration.GetSection("StripeSettings")["SecretKey"];
			StripeConfiguration.ApiKey = _stripeSecretKey;
            this.configuration = configuration;
            this.context = context;
            this.cartService = cartService;
        }


        public async Task<string> CreateStripeCustomer(ApplicationUser user)
        {
            StripeConfiguration.ApiKey = configuration["StripeSettings:SecretKey"];
            // Call the Stripe API to create a customer
            var customerOptions = new CustomerCreateOptions
            {
                Email = user.Email,
                // Other options as needed
            };
            var customerService = new CustomerService();
            var stripeCustomer = await customerService.CreateAsync(customerOptions);

            // Store the Stripe customer ID in the ApplicationUser object
         //   user.StripeCustomerId = stripeCustomer.Id;

            // Save changes to the database
            await context.SaveChangesAsync();

            // Return the Stripe customer ID
            return stripeCustomer.Id;
        }

        public async Task<string> CreatePaymentIntent(ClientSecretDto dto, string currency = "CHF")
		{
            var order = await cartService.GetOrderById(dto.OrderId);
            decimal amount = order.FinalTotalNumber;
            var customerOptions = new CustomerCreateOptions
            {
                Name = order.Name,
               
                // Other options as needed
            };
            var customerService = new CustomerService();
            var stripeCustomer = await customerService.CreateAsync(customerOptions);
           

            var options = new PaymentIntentCreateOptions
			{
				Amount = (long)(amount * 100), // amount in cents
				Currency = currency,
				PaymentMethodTypes = new List<string> { "card" },
                Customer = stripeCustomer.Id, // Assign the Stripe customer ID to the PaymentIntent

               
            };

			var service = new PaymentIntentService();
			var intent = await service.CreateAsync(options);

			return intent.ClientSecret;
		}
	}
}
