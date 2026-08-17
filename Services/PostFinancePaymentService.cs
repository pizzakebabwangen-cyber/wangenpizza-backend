using System;
using System.Collections.Generic;
using System.Linq;
using PostFinanceCheckout.Model;
using PostFinanceCheckout.Service;
using PostFinanceCheckout.Client;
using WangenPizza.Dtos;
using WangenPizza.Interfaces;
using WangenPizza.Models;
using Microsoft.CodeAnalysis.Differencing;
using Microsoft.EntityFrameworkCore;
using Stripe.Climate;
using WangenPizza.Context;
using Order = WangenPizza.Models.Order;
using System.Diagnostics;
using System.Drawing;

namespace WangenPizza.Services
{
    public class PostFinancePaymentService
    {

        private readonly long spaceId;
        private readonly string applicationUserID;
        private readonly string authenticationKey;
        private readonly Configuration configuration;
        private readonly TransactionService transactionService;
        private readonly IConfiguration appconfiguration;
        private readonly ICartService cartService;
        private readonly ApplicationDbContext context;

        public PostFinancePaymentService(IConfiguration Appconfiguration,ICartService cartService,ApplicationDbContext context)
        {
            appconfiguration = Appconfiguration;
            this.cartService = cartService;
            this.context = context;
            this.spaceId = long.Parse(appconfiguration.GetSection("PostFinancePayment")["SpaceId"]);
            this.authenticationKey = appconfiguration.GetSection("PostFinancePayment")["AuthenticationKey"];
            this.applicationUserID = appconfiguration.GetSection("PostFinancePayment")["ApplicationUserID"];
            this.configuration = new Configuration(this.applicationUserID, this.authenticationKey);
            this.transactionService = new TransactionService(configuration);
        }


        public Transaction CreateTransaction(Order order, string? lineItemDisplayName = null, string? invoiceReferencePrefix = null)
        {
            var billingAddress = new AddressCreate
            {
                Salutation = order.Salute,
                GivenName = order.Name,
                Gender = Gender.FEMALE,
                Country = "CH",
                City = order.City,
                DateOfBirth = new DateTime(1988, 4, 19),
                OrganizationName = "Wangen Pizza Kebab GmbH",
                MobilePhoneNumber = order.Mobile,
                EmailAddress = order.Email
            };

            var itemName = string.IsNullOrWhiteSpace(lineItemDisplayName) ? "Order Total" : lineItemDisplayName.Trim();
            var sku = string.IsNullOrWhiteSpace(lineItemDisplayName) ? "order-total" : "wertgutschein";

            var lineItem = new LineItemCreate(
                name: itemName,
                uniqueId: Guid.NewGuid().ToString(),
                type: LineItemType.PRODUCT,
                quantity: 1,
                amountIncludingTax: order.FinalTotalNumber
            )
            {
                Sku = sku,
                ShippingRequired = true
            };

            var invPrefix = string.IsNullOrWhiteSpace(invoiceReferencePrefix) ? "order" : invoiceReferencePrefix.Trim();

            var transactionCreate = new TransactionCreate(new List<LineItemCreate> { lineItem })
            {
                BillingAddress = billingAddress,
                ShippingAddress = billingAddress,
                CustomerEmailAddress = billingAddress.EmailAddress,
                CustomerId = order.UserId ?? "guest",
                MerchantReference = Guid.NewGuid().ToString(),
                InvoiceMerchantReference = $"{invPrefix}-{order.Id}",
                SuccessUrl = $"{appconfiguration["AppUrl"]}/api/Payment/success?orderId={order.Id}",
                FailedUrl = $"{appconfiguration["AppUrl"]}/api/Payment/failed?orderId={order.Id}",
                ShippingMethod = "Live Shipping",
                ChargeRetryEnabled = false,
                AllowedPaymentMethodConfigurations = new List<long?> {/*Twint*/284882L, /*Credit / Debit Card*/284881L,  /*PostFinance Pay*/ 284884L, /*PostFinance E-finance Pay*/  },
               // AllowedPaymentMethodConfigurations = new List<long?> {/*Twint*/284882L, /*Credit / Debit Card*/284881L, /*Cryptocurrency*/ 285729L, /*PostFinance Card*/ 284883L, /*PostFinance Pay*/ 284884L, /*PostFinance E-finance Pay*/ 286584L },
                Language = "de-DE",
                Currency = "CHF"
            };

            try
            {
                var transaction = transactionService.CreateWithHttpInfo(spaceId, transactionCreate);

                // ✅ خزن الترانزكشن مع الطلب
                order.TransactionId = transaction.Data.Id;
                context.Orders.Update(order);
                context.SaveChanges();

                // ✅ رجّع الترانزكشن كاملة (فيها Id) عشان الفرونت يعرف يستعملها
                return transaction.Data;
            }
            catch (ApiException e)
            {
                throw new Exception($"Failed to create transaction. Reason: {e.Message}", e);
            }
        }

        public string GetPaymentPageUrl(long? transactionId)
        {
            var transactionPaymentPageService = new TransactionPaymentPageService(configuration);
            try
            {
                return transactionPaymentPageService.PaymentPageUrl(spaceId, transactionId);
            }
            catch (ApiException e)
            {
                throw new Exception($"Failed to get payment page URL. Reason: {e.Message}", e);
            }
        }

        /// <summary>
        /// Liest den echten Transaktions-Status direkt bei PostFinance (autoritativ).
        /// Wird vom Webhook genutzt, damit eine Bestellung auch dann als bezahlt gilt,
        /// wenn der Kunde nach TWINT nicht auf die SuccessUrl zurückgeleitet wurde.
        /// </summary>
        public TransactionState? GetTransactionState(long transactionId)
        {
            try
            {
                var transaction = transactionService.Read(spaceId, transactionId);
                return transaction.State;
            }
            catch (ApiException e)
            {
                throw new Exception($"Failed to read transaction {transactionId}. Reason: {e.Message}", e);
            }
        }

        /// <summary>
        /// Findet die Bestellung anhand der bei <see cref="CreateTransaction"/> gespeicherten TransactionId.
        /// </summary>
        public int? GetOrderIdByTransactionId(long transactionId)
        {
            return context.Orders
                .Where(o => o.TransactionId == transactionId)
                .Select(o => (int?)o.Id)
                .FirstOrDefault();
        }


    }
}
