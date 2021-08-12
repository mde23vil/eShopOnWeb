using System;
using System.Threading.Tasks;
using Azure.Storage.Blobs;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage;

namespace OrderItemsReserver3
{
    public static class StoreOrderFromQueue
    {
        private const string containerName = "orders-queue";
        [FunctionName("StoreOrderFromQueue")]
        public async static Task Run([ServiceBusTrigger("orders-to-reserve", Connection = "ServiceBusConnectionString")]string myQueueItem, ILogger log)
        {
            string connectionString = Environment.GetEnvironmentVariable("storageConnectionString");

            var blobServiceClient = new BlobServiceClient(connectionString);

            var isContainerExist = false;
            var containers = blobServiceClient.GetBlobContainers();
            foreach (var item in containers)
            {
                if (item.Name == containerName)
                    isContainerExist = true;
            }

            if (!isContainerExist)
                blobServiceClient.CreateBlobContainer(containerName);

            var storageAccount = CloudStorageAccount.Parse(connectionString);
            var blobClient = storageAccount.CreateCloudBlobClient();
            var container = blobClient.GetContainerReference(containerName);
            var blockBlob = container.GetBlockBlobReference($"{Guid.NewGuid()}.json");

            await blockBlob.UploadTextAsync(myQueueItem);
        }
    }
}
