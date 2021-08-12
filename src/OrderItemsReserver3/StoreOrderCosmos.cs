using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Microsoft.Azure.Cosmos;
using System.Net;
using OrderItemsReserver3.Model;

namespace OrderItemsReserver3
{
    public static class StoreOrderCosmos
    {
        private static string EndpointUrl;
        private static string AuthorizationKey;
        private const string DatabaseId = "OrdersDatabase";
        private const string ContainerId = "OrdersContainer";

        [FunctionName("StoreOrderCosmos")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");
            try
            {
                EndpointUrl = Environment.GetEnvironmentVariable("cosmosDbOrdersEndpointUrl");
                AuthorizationKey = Environment.GetEnvironmentVariable("cosmosDbOrdersAuthorizationKey");

                var content = await req.ReadAsStringAsync();
                log.LogInformation(content);
                var jsonSerializerSettings = new JsonSerializerSettings();
                var extendedOrder = JsonConvert.DeserializeObject<ExtendedOrder>(content);
                var cosmosClient = new CosmosClient(EndpointUrl, AuthorizationKey);
                await CreateDatabaseAsync(cosmosClient);
                await CreateContainerAsync(cosmosClient);
                await AddItemsToContainerAsync(cosmosClient, extendedOrder);

                return new OkResult();
            }
            catch (Exception ex)
            {
                log.LogError(ex, "Storing order to Cosmos DB failed.");
                throw;
            }
        }

        /// <summary>
        /// Create the database if it does not exist
        /// </summary>
        private static async Task CreateDatabaseAsync(CosmosClient cosmosClient)
        {
            // Create a new database
            var database = await cosmosClient.CreateDatabaseIfNotExistsAsync(DatabaseId);
        }

        /// <summary>
        /// Create the container if it does not exist. 
        /// Specify "/LastName" as the partition key since we're storing family information, to ensure good distribution of requests and storage.
        /// </summary>
        /// <returns></returns>
        private static async Task CreateContainerAsync(CosmosClient cosmosClient)
        {
            // Create a new container
            var containerResponse = await cosmosClient.GetDatabase(DatabaseId).CreateContainerIfNotExistsAsync(ContainerId, "/BuyerId");
            var container = containerResponse.Container;
            Console.WriteLine("Created Container: {0}\n", container.Id);
        }

        /// <summary>
        /// Add Family items to the container
        /// </summary>
        private static async Task AddItemsToContainerAsync(CosmosClient cosmosClient, ExtendedOrder order)
        {
            var container = cosmosClient.GetContainer(DatabaseId, ContainerId);
            try
            {
                // Read the item to see if it exists.  
                ItemResponse<ExtendedOrder> orderResponse = await container.ReadItemAsync<ExtendedOrder>(order.id, new PartitionKey(order.BuyerId));
                Console.WriteLine("Item in database with id: {0} already exists\n", orderResponse.Resource.Id);
            }
            catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
            {
                // Create an item in the container representing the Andersen family. Note we provide the value of the partition key for this item, which is "Andersen"
                ItemResponse<ExtendedOrder> orderResponse = await container.CreateItemAsync(order, new PartitionKey(order.BuyerId));

                // Note that after creating the item, we can access the body of the item with the Resource property off the ItemResponse.
                Console.WriteLine("Created item in database with id: {0}\n", orderResponse.Resource.Id);
            }
        }
    }
}
