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

namespace Nuleep.Business.Services
{
    public interface IUdemyService
    {
        Task<object> SearchCourses(int page, int size, string search);
        Task<object> GetTrendingCourses(int page, int size);
    }

    public class UdemyService : IUdemyService
    {
        private readonly HttpClient _httpClient;
        private readonly string _authHeader;

        public UdemyService(IConfiguration config, HttpClient httpClient)
        {
            _httpClient = httpClient;

            var clientId = config["Udemy:ClientId"];
            var clientSecret = config["Udemy:ClientSecret"];
            var authBytes = Encoding.ASCII.GetBytes($"{clientId}:{clientSecret}");
            _authHeader = Convert.ToBase64String(authBytes);
        }

        public async Task<object> SearchCourses(int page, int size, string search)
        {
            string url = $"https://www.udemy.com/api-2.0/courses" +
                         $"?fields[course]=title,image_480x270,url,content_info_short,discount_price,price_detail,description,avg_rating" +
                         $"&page={page}&page_size={size}&search={search}";

            return await SendRequestAsync(url);
        }

        public async Task<object> GetTrendingCourses(int page, int size)
        {
            string url = $"https://www.udemy.com/api-2.0/courses" +
                         $"?fields[course]=@all&page={page}&page_size={size}";

            return await SendRequestAsync(url);
        }

        private async Task<object> SendRequestAsync(string url)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Authorization = new AuthenticationHeaderValue("Basic", _authHeader);

            var response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<object>(json);
        }
    }


}
