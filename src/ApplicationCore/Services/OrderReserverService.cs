using Microsoft.eShopWeb.ApplicationCore.Entities.OrderAggregate;
using Microsoft.eShopWeb.ApplicationCore.Interfaces;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.eShopWeb.ApplicationCore.Services
{
    public class OrderReserverService : IOrderReserverService
    {
        private readonly string _functionUrl;
        private readonly IHttpClientFactory _clientFactory;

        public OrderReserverService(IConfiguration configuration, IHttpClientFactory httpClientFactory)
        {
            _clientFactory = httpClientFactory;
            _functionUrl = configuration["baseUrls:functionUrl"];
        }

        public async Task Reserve(Order order)
        {
            var request = new HttpRequestMessage(HttpMethod.Post,
                _functionUrl);
            request.Content = new StringContent(order.ToJson());

            var client = _clientFactory.CreateClient();

            var response = await client.SendAsync(request);
        }
    }
}
