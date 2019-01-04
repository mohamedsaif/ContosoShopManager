using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Contoso.SB.API.Abstractions;
using Contoso.SB.API.Settings;
using Microsoft.Extensions.Configuration;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.Queue;

namespace Contoso.SB.API.Data
{
    public class AzureBlobStorageRepository : IStorageRepository
    {
        CloudStorageAccount storageAccount;
        CloudBlobClient cloudBlob;
        CloudBlobContainer blobContainer;
        CloudQueueClient queueClient;
        CloudQueue newReqQueue;
        CloudQueue callbackReqQueue;

        public AzureBlobStorageRepository(IConfiguration config)
        {
            var storageConfig = config.GetSection("StorageSettings").Get<StorageSettings>();

            storageAccount = new CloudStorageAccount(
                                    new Microsoft.WindowsAzure.Storage.Auth.StorageCredentials(
                                    storageConfig.StorageName,
                                    storageConfig.StorageKey), true);

            //Preparing the storage container for blobs
            cloudBlob = storageAccount.CreateCloudBlobClient();
            blobContainer = cloudBlob.GetContainerReference(storageConfig.StorageContainer);
            blobContainer.CreateIfNotExistsAsync().Wait();
        }

        public async Task<string> CreateFile(string name, Stream stream)
        {
            CloudBlockBlob blockBlob = blobContainer.GetBlockBlobReference(name);

            // Create or overwrite the file name blob with the contents of the provided stream
            await blockBlob.UploadFromStreamAsync(stream);

            return blockBlob.Uri.AbsoluteUri;
        }
    }
}
