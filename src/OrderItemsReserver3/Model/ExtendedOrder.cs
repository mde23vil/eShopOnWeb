using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace OrderItemsReserver3.Model
{
    
    public class ExtendedOrder
    {
        public int Id { get; set; }
        public string BuyerId { get; set; }
        public Address ShipToAddress { get; set; }
        public List<OrderItem> OrderItems { get; set; }

        public string id
        {
            get
            {
                return Guid.NewGuid().ToString();
            }
        }

        public ExtendedOrder()
        {
        }
    }
}
