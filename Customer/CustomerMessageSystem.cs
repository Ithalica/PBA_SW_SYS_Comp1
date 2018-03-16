using Domain;
using EasyNetQ;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Customer
{

   public class CustomerMessageSystem
    {
        IBus bus = RabbitHutch.CreateBus("host=localhost");
        OrderReply reply = null;
        int timeout = 10000;
        int count = 0;
        public void CreateOrder(Domain.Customer customer, Product product)
        {
            using (bus)
            {
                bus.Subscribe<OrderReply>("Customer" + customer.Id, ProcessOrder, x => x.WithTopic(customer.Id));
                Console.WriteLine("Subscribed");
                bus.Publish<BaseOrder>(new BaseOrder(count++, product, customer));

                lock (this)
                {
                    Monitor.Wait(this);
                }
            }
        }
        private void ProcessOrder(OrderReply message)
        {
            reply = message;
            Console.WriteLine("Proccesed order" + reply.OrderId + " Delivery type: " + message.DeliveryType + " Shippingfee: " + message.ShippingFee + " Delivery time" + message.DeliveryTime);
            
        }
    }
}
