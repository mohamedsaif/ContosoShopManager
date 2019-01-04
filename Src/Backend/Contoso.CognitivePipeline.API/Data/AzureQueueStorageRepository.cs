using Contoso.SB.API.Abstractions;
using Contoso.SB.API.Settings;
using Microsoft.Extensions.Configuration;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Queue;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Contoso.SB.API.Data
{
    public class AzureQueueStorageRepository : IQueueRepository
    {
        // TODO: Separate queue storage account from the blob storage account for security (docs may include secure information that need to be kept separately)
        CloudStorageAccount storageAccount;
        CloudQueueClient queueClient;
        CloudQueue newReqQueue;
        CloudQueue callbackReqQueue;

        public AzureQueueStorageRepository(IConfiguration config)
        {
            var storageConfig = config.GetSection("StorageSettings").Get<StorageSettings>();

            storageAccount = new CloudStorageAccount(
                                    new Microsoft.WindowsAzure.Storage.Auth.StorageCredentials(
                                    storageConfig.StorageName,
                                    storageConfig.StorageKey), true);

            //Preparing the storage queues 
            // Create the CloudQueueClient object for the storage account.
            queueClient = storageAccount.CreateCloudQueueClient();

            // Get a reference to the CloudQueue
            newReqQueue = queueClient.GetQueueReference(storageConfig.StorageNewRequestQueue);
            callbackReqQueue = queueClient.GetQueueReference(storageConfig.StorageCallbackRequestQueue);

            // Create the CloudQueue if it does not exist.
            newReqQueue.CreateIfNotExistsAsync().Wait();
            callbackReqQueue.CreateIfNotExistsAsync().Wait();
        }

        public async Task<bool> DeleteMessage(CloudQueueMessage message, QueueType queueType)
        {
            // Then delete the message from the relevant queue
            var queue = GetQueue(queueType);
            await queue.DeleteMessageAsync(message);
            return true;
        }

        public async Task<CloudQueueMessage> GetMessage(QueueType queueType)
        {
            var queue = GetQueue(queueType);
            // Get the next message in the queue.
            CloudQueueMessage retrievedMessage = await queue.GetMessageAsync();
            return retrievedMessage;
        }

        public async Task<int> GetQueueLength(QueueType queueType)
        {
            var queue = GetQueue(queueType);
            // Fetch the queue attributes.
            await queue.FetchAttributesAsync();

            // Retrieve the cached approximate message count.
            int? cachedMessageCount = queue.ApproximateMessageCount;

            if (cachedMessageCount == null)
                return 0;
            return cachedMessageCount.Value;
        }

        public async Task<bool> QueueMessage(string message, QueueType queueType)
        {
            var queue = GetQueue(queueType);
            // Create a message and add it to the queue.
            CloudQueueMessage queueMessage = new CloudQueueMessage(message);
            await queue.AddMessageAsync(queueMessage);
            return true;
        }

        public CloudQueue GetQueue(QueueType queueType)
        {
            switch (queueType)
            {
                case QueueType.NewRequestQueue:
                    return newReqQueue;
                    break;
                case QueueType.CallbackRequestQueue:
                    return callbackReqQueue;
                    break;
                default:
                    return null;
                    break;
            }
        }
    }
}
