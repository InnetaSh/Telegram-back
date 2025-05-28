using Azure.Storage.Blobs.Models;
using Azure.Storage.Blobs;

namespace Telegram_back.Service
{
    public class BlobService
    {
        private readonly BlobContainerClient _containerClient;

        public BlobService(IConfiguration config)
        {
            var connection = config["AzureBlob:ConnectionString"];
            var containerName = config["AzureBlob:ContainerName"];
            var blobServiceClient = new BlobServiceClient(connection);
            _containerClient = blobServiceClient.GetBlobContainerClient(containerName);
            _containerClient.CreateIfNotExists(PublicAccessType.Blob);
        }

        public async Task<string> UploadFileAsync(IFormFile file)
        {
            var fileName = Guid.NewGuid() + Path.GetExtension(file.FileName);
            var blobClient = _containerClient.GetBlobClient(fileName);
            await using var stream = file.OpenReadStream();
            await blobClient.UploadAsync(stream, true);
            return blobClient.Uri.ToString();
        }

        public async Task DeleteFileAsync(string blobUrl)
        {
            var blobUri = new Uri(blobUrl);
            string blobName = Path.GetFileName(blobUri.LocalPath);

            var blobClient = _containerClient.GetBlobClient(blobName);
            await blobClient.DeleteIfExistsAsync();
        }
    }
}
