using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Azure.Storage.Blobs;
using Sample.Exceptions;
using Sample.Storage;
using Sample.Storage.Azure.Settings;

namespace Sample.Storage.Azure
{
    // This is a simple implementation to write to azure storage.  It lacks
    // a lot of validation and has some race conditions.  It is only  meant to
    // show a simple example of wiring up your app to real storage access.
    // DO NOT USE for production services.
    public class AzureBlobStorage : IStorage
    {
        private readonly AzureStorageSettings settings;
        private readonly BlobContainerClient blobContainerClient;

        public AzureBlobStorage(AzureStorageSettings settings)
        {
            this.settings = Guard.ThrowIfNull(settings, nameof(settings));

            this.blobContainerClient = new BlobContainerClient(this.settings.ConnectionString, this.settings.ContainerName);
        }

        public async IAsyncEnumerable<string> GetIdentifiersAsync()
        {
            await this.blobContainerClient.CreateIfNotExistsAsync();

            await foreach (var blob in this.blobContainerClient.GetBlobsAsync())
            {
                yield return blob.Name;
            }
        }

        public async Task CreateAsync(string key, Stream value)
        {
            await this.blobContainerClient.CreateIfNotExistsAsync();

            await this.blobContainerClient.DeleteBlobIfExistsAsync(key);

            await this.blobContainerClient.UploadBlobAsync(key, value);
        }

        public async Task<Stream> GetAsync(string key)
        {
            await this.blobContainerClient.CreateIfNotExistsAsync();

            var client = this.blobContainerClient.GetBlobClient(key);

            if (!(await client.ExistsAsync()))
            {
                throw new KeyNotFoundException("key");
            }

            var blob = await client.DownloadAsync();

            return blob.Value.Content;
        }

        public async Task RemoveAsync(string key)
        {
            await this.blobContainerClient.CreateIfNotExistsAsync();

            var client = this.blobContainerClient.GetBlobClient(key);

            await client.DeleteIfExistsAsync();
        }
    }
}