using Newtonsoft.Json;
using System.Net.Http.Headers;
using System.Text;

namespace WangenPizza.Services
{
    public class TwintService
    {
        private readonly IConfiguration _configuration;
        private readonly HttpClient _httpClient;

        public TwintService(IConfiguration configuration)
        {
            _configuration = configuration;
            _httpClient = new HttpClient();
        }

        public async Task<string> CreatePayment(decimal amount, int orderId)
        {
            var apiUrl = _configuration["Twint:ApiUrl"];
            var apiKey = _configuration["Twint:ApiKey"];
            var apiSecret = _configuration["Twint:ApiSecret"];

            var request = new
            {
                amount = (int)(amount * 100),
                currency = "CHF",
                orderId,
                successUrl = "https://yourdomain.com/api/payment/accept",
                failUrl = "https://yourdomain.com/api/payment/decline",
                cancelUrl = "https://yourdomain.com/api/payment/cancel"
            };

            var json = JsonConvert.SerializeObject(request);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic",
                Convert.ToBase64String(Encoding.ASCII.GetBytes($"{apiKey}:{apiSecret}")));

            var response = await _httpClient.PostAsync($"{apiUrl}/payment/initiate", content);
            response.EnsureSuccessStatusCode();

            var responseString = await response.Content.ReadAsStringAsync();
            var responseObject = JsonConvert.DeserializeObject<dynamic>(responseString);

            return responseObject.paymentPageUrl;
        }
    }
}
