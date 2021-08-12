using Microsoft.Azure.ServiceBus;
using Microsoft.eShopWeb.ApplicationCore.Entities.OrderAggregate;
using Microsoft.eShopWeb.ApplicationCore.Interfaces;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.eShopWeb.Web.Services
{
    public class OrderReserverQueueService : IOrderReserverService
    {
        private readonly QueueClient _queueClient;
        private readonly string _functionUrl;
        private readonly IHttpClientFactory _clientFactory;

        public OrderReserverQueueService(IConfiguration configuration, IHttpClientFactory httpClientFactory)
        {
            var queueConnectionString = configuration["baseUrls:queuConnectionString"];
            _queueClient = new QueueClient(queueConnectionString, "orders-to-reserve");
            _clientFactory = httpClientFactory;
            _functionUrl = configuration["baseUrls:functionUrl"];
        }

        public async Task Reserve(Order order)
        {
            string message = JsonConvert.SerializeObject(order);
            var encodedMessage = new Message(Encoding.UTF8.GetBytes(message));
            await _queueClient.SendAsync(encodedMessage);



            var request = new HttpRequestMessage(HttpMethod.Post,
                _functionUrl);
            request.Content = new StringContent(order.ToJson());
            var client = _clientFactory.CreateClient();
            var response = await client.SendAsync(request);
        }
    }
}
