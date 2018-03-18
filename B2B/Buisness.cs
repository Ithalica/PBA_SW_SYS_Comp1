using Domain;
using EasyNetQ;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace B2B
{
    public class Buisness
    {
        private readonly IBus _bus = RabbitHutch.CreateBus("host=localhost");
        private readonly List<Order> _outstandingOrders = new List<Order>();
        private readonly Dictionary<int, List<OrderReply>> _broadcastOrders = new Dictionary<int, List<OrderReply>>();
        public void Initialize()
        {
            using (_bus)
            {
                _bus.Subscribe<BaseOrder>("CustomerRequests",
                                           ProcessRequest);
                _bus.Subscribe<OrderReply>("DepartmentReplies",
                                           ProcessReply, x => x.WithQueueName(nameof(Order)));

                //bus.Subscribe<OrderReply>("DepartmentBroadcastReplies",
                //                           ProcessBroadCastReply, x => x.WithQueueName(nameof(BroadCastOrder)));
                _bus.Subscribe<OrderReply>("DepartmentBroadcastReplies",
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
            _outstandingOrders.Add(order);
            _bus.Publish(order, request.Customer.Country);
            Console.WriteLine("Published Request");
        }

           private void ProcessBroadCastReply(OrderReply message)
        {
                Console.WriteLine("Proccesing Broadcast reply");
                if(!_broadcastOrders.Any(x => x.Key == message.OrderId))
            {
                _broadcastOrders.Add(message.OrderId, new List<OrderReply>() { message });

            }
            var dictItem = _broadcastOrders[message.OrderId];
            if (dictItem != null && !dictItem.Contains(message))
            {

                dictItem.Add(message);
               
            }
           
            var order = _outstandingOrders.FirstOrDefault(x => x.Id == message.OrderId);
            if(order != null) { 
            if (dictItem.Any(x => x.DeliveryType == DeliveryEnum.Full))
            {
                Console.WriteLine("Full order delivery possible");
                _bus.Publish(dictItem.FirstOrDefault(x => x.DeliveryType == DeliveryEnum.Full), order.Customer.Id);
                    _outstandingOrders.Remove(order);
                return;
            }else if (dictItem.Where(x => x.Product != null).Sum(x => x.Product.Amount) > order.Product.Amount)
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
                _bus.Publish(replyMessage, order.Customer.Id);
                    _outstandingOrders.Remove(order);
                    return;

            }
            else if (dictItem.Count == 3)
            {
                Console.WriteLine("No delivery possible");
                _bus.Publish(new OrderReply(-1, message.OrderId, order.Product, DeliveryEnum.None, null), order.Customer.Id);
                    _outstandingOrders.Remove(order);
                }
            }
        }
   
        private void ProcessReply(OrderReply message)
        {
                Console.WriteLine("Proccesing reply");
                var order = _outstandingOrders.FirstOrDefault(x => x.Id == message.OrderId);
            if (order != null)
            {
                if (message.DeliveryType == DeliveryEnum.Full)
                {
                    _bus.Publish(message, order.Customer.Id);
                    Console.WriteLine("Sent reply");
                    _outstandingOrders.Remove(order);
                }
                else
                {
                    _bus.Publish(new BroadCastOrder(order.Id, order.Product, order.Customer)
                    {
                        ReplyTo = nameof(BroadCastOrder)
                    });
                    Console.WriteLine("Broadcasted request");
                }
            }
        }
    }
}
