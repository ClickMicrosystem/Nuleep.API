using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Blobs;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Http;

namespace Nuleep.Business.Services
{
    public class AzureFileService
    {
        private readonly string _accountName;
        private readonly string _accountKey;
        private readonly string _container;
        private readonly BlobServiceClient _blobServiceClient;

        public AzureFileService(IConfiguration configuration)
        {
            _accountName = configuration["AzureStorage:AccountName"];
            _accountKey = configuration["AzureStorage:AccountKey"];
            _container = configuration["AzureStorage:Container"];

            var blobUri = new Uri($"https://{_accountName}.blob.core.windows.net");
            var credential = new Azure.Storage.StorageSharedKeyCredential(_accountName, _accountKey);
            _blobServiceClient = new BlobServiceClient(blobUri, credential);
        }

        public async Task<(bool Success, string BlobName, string FullUrl)> UploadFileAsync(IFormFile file)
        {
            var containerClient = _blobServiceClient.GetBlobContainerClient(_container);
            var blobName = $"{DateTime.UtcNow.Ticks}_{file.FileName.Replace(" ", "_")}";
            var blobClient = containerClient.GetBlobClient(blobName);

            using var stream = file.OpenReadStream();
            await blobClient.UploadAsync(stream, new BlobHttpHeaders { ContentType = file.ContentType });

            return (true, blobName, blobClient.Uri.ToString());
        }

        public async Task<bool> DeleteFileAsync(string blobName)
        {
            var containerClient = _blobServiceClient.GetBlobContainerClient(_container);
            var blobClient = containerClient.GetBlobClient(blobName);

            var exists = await blobClient.ExistsAsync();
            if (!exists) return false;

            await blobClient.DeleteAsync();
            return true;
        }
    }
}
