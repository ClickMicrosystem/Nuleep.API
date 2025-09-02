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

namespace Nuleep.Business.Services
{
    public class EmailService
    {
        private readonly IConfiguration _config;

        public EmailService(IConfiguration config)
        {
            _config = config;
        }

        public async Task<bool> SendResetEmail(string toEmail, string resetUrl)
        {
            var client = new SendGridClient(_config["SendGrid:ApiKey"]);
            var from = new EmailAddress(_config["SendGrid:FromEmail"], "Nuleep");
            var subject = "Nuleep Password Reset Token";

            var plainText = $"Reset your password using this link: {resetUrl}";
            var htmlContent = $"<p>You are receiving this email because you (or someone else) has requested the reset of a password.</p>" +
                              $"<a href='{resetUrl}' style='font-size: 20px'>click here</a>";

            var to = new EmailAddress(toEmail);
            var msg = MailHelper.CreateSingleEmail(from, to, subject, plainText, htmlContent);
            var response = await client.SendEmailAsync(msg);

            return response.IsSuccessStatusCode;
        }

        public async Task<bool> SendEmail(string toEmail, string subject, string message)
        {
            var client = new SendGridClient(_config["SendGrid:ApiKey"]);
            var from = new EmailAddress(_config["SendGrid:FromEmail"], "Nuleep");
            var plainText = $"Claim ownership of your company at Nuleep";
            var to = new EmailAddress(toEmail);
            var msg = MailHelper.CreateSingleEmail(from, to, subject, plainText, message);
            var response = await client.SendEmailAsync(msg);

            return response.IsSuccessStatusCode;
        }
    }



}
