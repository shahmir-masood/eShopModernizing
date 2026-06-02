using System;
using System.IO;
using eShopModernizedWebForms.Models;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;

namespace eShopModernizedWebForms.Services
{
    /// <summary>
    /// Blob-storage backed image service. Migrated from the legacy
    /// WindowsAzure.Storage SDK to the modern Azure.Storage.Blobs SDK.
    /// </summary>
    public class ImageAzureStorage : IImageService
    {
        private const string ContainerName = "pics";

        private readonly BlobServiceClient _serviceClient;
        private readonly BlobContainerClient _container;
        private readonly IWebHostEnvironment _environment;

        public ImageAzureStorage(IWebHostEnvironment environment)
        {
            _environment = environment;
            _serviceClient = new BlobServiceClient(CatalogConfiguration.StorageConnectionString);
            _container = _serviceClient.GetBlobContainerClient(ContainerName);
        }

        public string BaseUrl()
        {
            return _serviceClient.Uri.ToString();
        }

        public string BuildUrlImage(CatalogItem item)
        {
            if (string.IsNullOrEmpty(item.PictureFileName))
                return UrlDefaultImage();

            return $"{_serviceClient.Uri}{ContainerName}/{item.Id}/{item.PictureFileName}";
        }

        public void Dispose()
        {
        }

        public void InitializeCatalogImages()
        {
            _container.CreateIfNotExists(PublicAccessType.Blob);

            foreach (var blob in _container.GetBlobs())
            {
                _container.DeleteBlob(blob.Name);
            }

            var webRoot = Path.Combine(_environment.WebRootPath, "Pics");

            for (int i = 1; i <= 12; i++)
            {
                var path = Path.Combine(webRoot, i + ".png");
                var blobName = i + "/" + i + ".png";
                UploadImageFromFile(blobName, path, "image/png");
            }

            var defaultImagePath = Path.Combine(webRoot, "default.png");
            UploadImageFromFile("temp/default.png", defaultImagePath, "image/png");
        }

        public void UpdateImage(CatalogItem item)
        {
            var folder = item.TempImageName.Replace("/pics/", string.Empty);
            var tempBlob = _container.GetBlobClient(folder);

            foreach (var blob in _container.GetBlobs(prefix: item.Id + "/"))
            {
                _container.DeleteBlob(blob.Name);
            }

            var fileName = Path.GetFileName(item.TempImageName);
            var imageBlob = _container.GetBlobClient(item.Id + "/" + fileName);

            imageBlob.StartCopyFromUri(tempBlob.Uri);
            tempBlob.DeleteIfExists();
        }

        public string UploadTempImage(IFormFile file, int? catalogItemId)
        {
            string path = catalogItemId.HasValue
                ? catalogItemId + "/temp/"
                : "temp/" + Guid.NewGuid() + "/";

            var blob = _container.GetBlobClient(path + file.FileName.ToLowerInvariant());

            using (var stream = file.OpenReadStream())
            {
                blob.Upload(stream, new BlobHttpHeaders { ContentType = file.ContentType });
            }

            return blob.Uri.ToString();
        }

        public string UrlDefaultImage()
        {
            return $"{_serviceClient.Uri}{ContainerName}/temp/default.png";
        }

        private void UploadImageFromFile(string blobName, string filePath, string contentType)
        {
            using (var fileStream = File.OpenRead(filePath))
            {
                var blob = _container.GetBlobClient(blobName);
                blob.Upload(fileStream, new BlobHttpHeaders { ContentType = contentType });
            }
        }
    }
}
