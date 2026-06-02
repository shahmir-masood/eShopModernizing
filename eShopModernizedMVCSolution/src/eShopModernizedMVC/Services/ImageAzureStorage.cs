using eShopModernizedMVC.Models;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using System;
using System.IO;
using Microsoft.AspNetCore.Http;

namespace eShopModernizedMVC.Services
{
    public class ImageAzureStorage : IImageService
    {
        private const string ContainerName = "pics";

        private readonly BlobServiceClient _blobServiceClient;

        public ImageAzureStorage()
        {
            _blobServiceClient = new BlobServiceClient(CatalogConfiguration.StorageConnectionString);
        }

        public string BaseUrl()
        {
            return NormalizeUri(_blobServiceClient.Uri);
        }

        public string BuildUrlImage(CatalogItem item)
        {
            if (string.IsNullOrEmpty(item.PictureFileName))
                return UrlDefaultImage();

            return BaseUrl() + "pics/" + item.Id + "/" + item.PictureFileName;
        }

        public void Dispose()
        {
        }

        public void InitializeCatalogImages()
        {
            BlobContainerClient container = _blobServiceClient.GetBlobContainerClient(ContainerName);

            container.CreateIfNotExists(PublicAccessType.Blob);

            foreach (var blobItem in container.GetBlobs())
            {
                container.DeleteBlob(blobItem.Name);
            }

            var webRoot = Path.Combine(HostingConfiguration.WebRootPath, "Pics");

            for (int i = 1; i <= 12; i++)
            {
                var path = Path.Combine(webRoot, i + ".png");
                var blobName = i + "/" + i + ".png";
                UpLoadImageFromFile(container, blobName, path, "image/png");
            }
            var defaultImagePath = Path.Combine(webRoot, "default.png");
            UpLoadImageFromFile(container, "temp/default.png", defaultImagePath, "image/png");
        }

        public void UpdateImage(CatalogItem item)
        {
            BlobContainerClient container = _blobServiceClient.GetBlobContainerClient(ContainerName);

            var folder = item.TempImageName.Replace("/pics/", string.Empty);

            BlobClient tempBlob = container.GetBlobClient(folder);

            foreach (var blobItem in container.GetBlobs(prefix: item.Id + "/"))
            {
                container.DeleteBlob(blobItem.Name);
            }

            var fileName = Path.GetFileName(item.TempImageName);
            BlobClient imageBlob = container.GetBlobClient(item.Id + "/" + fileName);

            imageBlob.SyncCopyFromUri(tempBlob.Uri);
            tempBlob.DeleteIfExists();
        }

        public string UploadTempImage(IFormFile file, int? catalogItemId)
        {
            string path = catalogItemId.HasValue ? catalogItemId + "/temp/" : "temp/" + Guid.NewGuid().ToString() + "/";

            BlobContainerClient container = _blobServiceClient.GetBlobContainerClient(ContainerName);
            BlobClient blockBlob = container.GetBlobClient(path + file.FileName.ToLower());

            using (var stream = file.OpenReadStream())
            {
                blockBlob.Upload(stream, new BlobHttpHeaders { ContentType = file.ContentType });
            }

            return blockBlob.Uri.ToString();
        }

        public string UrlDefaultImage()
        {
            return BaseUrl() + "pics/temp/default.png";
        }

        private void UpLoadImageFromFile(BlobContainerClient container, string blobName, string filePath, string contentType)
        {
            using (var fileStream = File.OpenRead(filePath))
            {
                BlobClient blockBlob = container.GetBlobClient(blobName);
                blockBlob.Upload(fileStream, new BlobHttpHeaders { ContentType = contentType });
            }
        }

        private static string NormalizeUri(Uri uri)
        {
            var value = uri.ToString();
            return value.EndsWith("/") ? value : value + "/";
        }
    }
}
