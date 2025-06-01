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

        public async Task<FileUploadResult> UploadAsync(string containerName, IFormFile file)
        {
            var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
            await containerClient.CreateIfNotExistsAsync(PublicAccessType.None);

            var sanitizedFileName = Regex.Replace(file.FileName.Replace(" ", "_"), @"\([^)]*\)+", "_");
            var blobName = $"{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}{sanitizedFileName}";
            var blobClient = containerClient.GetBlobClient(blobName);

            using var stream = file.OpenReadStream();
            await blobClient.UploadAsync(stream, new BlobHttpHeaders { ContentType = file.ContentType });

            return new FileUploadResult
            {
                Success = true,
                Data = new ProjectImage
                {
                    FileName = file.FileName,
                    BlobName = blobName,
                    FullUrl = $"https://{_accountName}.blob.core.windows.net/{containerName}/{blobName}"
                }
            };
        }

        public async Task<DeleteResult> DeleteAsync(string containerName, string blobName)
        {
            var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
            var blobClient = containerClient.GetBlobClient(blobName);

            var exists = await blobClient.ExistsAsync();
            if (!exists)
            {
                return new DeleteResult
                {
                    Success = false,
                    Error = "Blob does not exist"
                };
            }

            await blobClient.DeleteAsync();
            return new DeleteResult
            {
                Success = true,
                Deleted = true
            };
        }

        public async Task<List<ProjectImage>> FindAsync(string containerName)
        {
            var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
            var results = new List<ProjectImage>();

            await foreach (var blob in containerClient.GetBlobsAsync())
            {
                results.Add(new ProjectImage
                {
                    FileName = Path.GetFileNameWithoutExtension(blob.Name).Replace("-", " "),
                    BlobName = blob.Name,
                    FullUrl = $"https://{_accountName}.blob.core.windows.net/{containerName}/{blob.Name}"
                });
            }

            return results;
        }

    }

    public class DeleteResult
    {
        public bool Success { get; set; }
        public bool Deleted { get; set; }
        public string? Error { get; set; }
    }

    public class FileUploadResult
    {
        public bool Success { get; set; }
        public ProjectImage Data { get; set; }
    }


}
