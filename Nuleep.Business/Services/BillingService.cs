using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Blobs;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Http;
using Nuleep.Models;
using System.Text.RegularExpressions;
using Microsoft.Data.SqlClient;
using Azure.Core;
using DocumentFormat.OpenXml.Office2016.Excel;
using System.Data;
using Dapper;
using System.Net.Mail;
using SendGrid.Helpers.Mail;
using SendGrid;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using Stripe;
using Stripe.Checkout;
using Nuleep.Models.Request;

namespace Nuleep.Business.Services
{
    public interface IBillingService
    {
        Task<string> CreateBillingPortalSession(string customerId);
        Task<string> CreateCheckoutSession(CheckoutSessionRequest request);
    }

    public class BillingService : IBillingService
    {
        private readonly string _frontendUrl;

        public BillingService(IConfiguration config)
        {
            StripeConfiguration.ApiKey = config["Stripe:SecretKey"];
            _frontendUrl = config["Stripe:FrontendUrl"];
        }

        public async Task<string> CreateBillingPortalSession(string customerId)
        {
            var service = new Stripe.BillingPortal.SessionService();

            var options = new Stripe.BillingPortal.SessionCreateOptions
            {
                Customer = customerId,
                ReturnUrl = _frontendUrl
            };

            var session = await service.CreateAsync(options);
            return session.Url;
        }

        public async Task<string> CreateCheckoutSession(CheckoutSessionRequest request)
        {
            // Always create a new customer (like your Node.js version)
            var customerService = new CustomerService();
            var newCustomer = await customerService.CreateAsync(new CustomerCreateOptions
            {
                Email = request.Email
            });

            // Success URL
            string successUrl = request.Approve?.ToString() switch
            {
                "True" => $"{_frontendUrl}/company?success=true&session_id={{CHECKOUT_SESSION_ID}}",
                "SUB" => $"{_frontendUrl}/company?success=true&session_id={{CHECKOUT_SESSION_ID}}",
                _ => $"{_frontendUrl}/onboarding?success=true&session_id={{CHECKOUT_SESSION_ID}}"
            };

            // Cancel URL
            string cancelUrl = request.Approve?.ToString() switch
            {
                "True" => $"{_frontendUrl}/company?canceled=true",
                "SUB" => $"{_frontendUrl}/subscription?canceled=true",
                _ => $"{_frontendUrl}/onboarding?canceled=true"
            };

            var options = new SessionCreateOptions
            {
                BillingAddressCollection = "auto",
                Customer = newCustomer.Id,
                PaymentMethodTypes = new List<string> { "card" },
                LineItems = new List<SessionLineItemOptions>
                {
                    new SessionLineItemOptions
                    {
                        Price = request.ProductId, // Stripe Price ID
                        Quantity = 1
                    }
                },
                Mode = "subscription",
                SubscriptionData = new SessionSubscriptionDataOptions
                {
                    TrialPeriodDays = 15,
                    Metadata = new Dictionary<string, string>
                    {
                        { "email", request.Email }
                    }
                },
                AllowPromotionCodes = true,
                SuccessUrl = successUrl,
                CancelUrl = cancelUrl
            };

            var service = new SessionService();
            var session = await service.CreateAsync(options);

            return session.Url;
        }
    }
}
