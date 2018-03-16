using Domain;
using EasyNetQ;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace B2B
{
    public class Buisness
    {
        private IBus bus = RabbitHutch.CreateBus("host=localhost");
        private List<Order> OutstandingOrders = new List<Order>();
        private Dictionary<int, List<OrderReply>> broadcastOrders = new Dictionary<int, List<OrderReply>>();
        public void Initialize()
        {
            using (bus)
            {
                bus.Subscribe<BaseOrder>("CustomerRequests",
                                           ProcessRequest);
                bus.Subscribe<OrderReply>("DepartmentReplies",
                                           ProcessReply, x => x.WithQueueName(nameof(Order)));

                //bus.Subscribe<OrderReply>("DepartmentBroadcastReplies",
                //                           ProcessBroadCastReply, x => x.WithQueueName(nameof(BroadCastOrder)));
                bus.Subscribe<OrderReply>("DepartmentBroadcastReplies",
                                           ProcessBroadCastReply, x => x.WithQueueName(nameof(BroadCastOrder)));
                
                Console.WriteLine("Subscribed");
                lock (this)
                {
                    Monitor.Wait(this);
                }
            }
        }
        private void ProcessRequest(BaseOrder request)
        {
            Console.WriteLine("Recived request");
            var order = new Order(request.Id, request.Product, request.Customer)
            {
                ReplyTo = nameof(Order)
            };
            OutstandingOrders.Add(order);
            bus.Publish(order, request.Customer.Country);
            Console.WriteLine("Published Request");
        }

           private void ProcessBroadCastReply(OrderReply message)
        {
                Console.WriteLine("Proccesing Broadcast reply");
                if(!broadcastOrders.Any(x => x.Key == message.OrderId))
            {
                broadcastOrders.Add(message.OrderId, new List<OrderReply>() { message });

            }
            var dictItem = broadcastOrders[message.OrderId];
            if (dictItem != null && !dictItem.Contains(message))
            {

                dictItem.Add(message);
               
            }
           
            var order = OutstandingOrders.FirstOrDefault(x => x.Id == message.OrderId);
            if(order != null) { 
            if (dictItem.Any(x => x.DeliveryType == DeliveryEnum.Full))
            {
                Console.WriteLine("Full order delivery possible");
                bus.Publish(dictItem.FirstOrDefault(x => x.DeliveryType == DeliveryEnum.Full), order.Customer.Id);
                    OutstandingOrders.Remove(order);
                return;
            }else if (dictItem.Sum(x => x.Product.Amount) > order.Product.Amount)
            {
                Console.WriteLine("Multiple partial delivery possible");

                var currentOrders = new List<OrderReply>();
                while(currentOrders.Sum(x => x.Product.Amount) < order.Product.Amount){
                    var maxStock = dictItem.Max(x => x.Product.Amount);
                    currentOrders.AddRange(dictItem.Where(x => x.Product.Amount == maxStock));
                }
                var replyMessage = new OrderReply(-2, order.Id, order.Product, DeliveryEnum.Full, null)
                {
                    DeliveryTime = currentOrders.Max(x => x.DeliveryTime),
                    ShippingFee = currentOrders.Sum(x => x.ShippingFee)
                };
                bus.Publish(replyMessage, order.Customer.Id);
                    OutstandingOrders.Remove(order);
                    return;

            }
            else if (dictItem.Count == 3)
            {
                Console.WriteLine("No delivery possible");
                bus.Publish(new OrderReply(-1, message.OrderId, order.Product, DeliveryEnum.None, null), order.Customer.Id);
                    OutstandingOrders.Remove(order);
                }
            }
        }
   
        private void ProcessReply(OrderReply message)
        {
                Console.WriteLine("Proccesing reply");
                var order = OutstandingOrders.FirstOrDefault(x => x.Id == message.OrderId);
            if (order != null)
            {
                if (message.DeliveryType == DeliveryEnum.Full)
                {
                    bus.Publish(message, order.Customer.Id);
                    Console.WriteLine("Sent reply");
                    OutstandingOrders.Remove(order);
                }
                else
                {
                    bus.Publish(new BroadCastOrder(order.Id, order.Product, order.Customer)
                    {
                        ReplyTo = nameof(BroadCastOrder)
                    });
                    Console.WriteLine("Broadcasted request");
                }
            }
        }
    }
}
