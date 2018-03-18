using Domain;
using EasyNetQ;
using System;
using System.Threading;

namespace Customer
{
    public class CustomerMessageSystem
    {
        readonly IBus _bus = RabbitHutch.CreateBus("host=localhost");
        OrderReply _reply = null;
        int _count = 0;
        public void CreateOrder(Domain.Customer customer, Product product)
        {
            using (_bus)
            {
                _bus.Subscribe<OrderReply>("Customer" + customer.Id, ProcessOrder, x => x.WithTopic(customer.Id));
                Console.WriteLine("Subscribed");
                _bus.Publish<BaseOrder>(new BaseOrder(_count++, product, customer));

                lock (this)
                {
                    Monitor.Wait(this);
                }
            }
        }
        private void ProcessOrder(OrderReply message)
        {
            _reply = message;
            Console.WriteLine("Proccesed order" + _reply.OrderId + " Delivery type: " + message.DeliveryType + " Shippingfee: " + message.ShippingFee + " Delivery time" + message.DeliveryTime);
        }
    }
}
