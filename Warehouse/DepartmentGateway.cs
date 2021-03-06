﻿using Domain;
using EasyNetQ;
using System;
using System.Linq;
using System.Threading;

namespace Warehouse
{
    public class DepartmentGateway
    {
        private const int InternalShippingCharge = 5;
        private const int ExternalShippingCharge = 10;
        private const int InternalShippingDays = 2;
        private const int ExternalShippingDays = 10;
        private readonly IBus _bus = RabbitHutch.CreateBus("host=localhost");

        public void DoStuff(Department department)
        {
            using (_bus)
            {
                _bus.Subscribe<Order>("department" + department.Id,
                                           x => CheckOrder(x, department), x => x.WithTopic(department.Country));

                _bus.Subscribe<BroadCastOrder>("department" + department.Id,
                   x => CheckOrder(x, department));
                Console.WriteLine("Subscribed");
                Console.ReadLine();

                lock (this)
                {
                    Monitor.Wait(this);
                }
            }
        }

        public void CheckOrder(Order order, Department department)
        {
            string orderType;
            if (order is BroadCastOrder broadcastORder)
            {
                Console.WriteLine("Broadcast order recieved at department " +
                     department.Id);
                orderType = nameof(BroadCastOrder);
            }
            else
            {
                Console.WriteLine("Order placed at department" + department.Id);
                orderType = nameof(Order);
            }
            var product = department.Products.FirstOrDefault(x => x.Id == order.Product.Id);
            if (product != null)
            {
                //Product found, determaining if full delivery is possible.
                var amount = product.Amount;
                DeliveryEnum type;
                if (amount > order.Product.Amount)
                {
                    type = DeliveryEnum.Full;
                }
                else if (amount > 0)
                {
                    type = DeliveryEnum.Partial;
                }
                else
                {
                    type = DeliveryEnum.None;
                }

                var reply = new OrderReply(department.Id, order.Id, product, type, orderType);
                if (type != DeliveryEnum.None)
                {
                    bool isInternal = department.Country.Equals(order.Customer.Country, StringComparison.InvariantCultureIgnoreCase);
                    reply.DeliveryTime = isInternal ? InternalShippingDays : ExternalShippingDays;
                    reply.ShippingFee = isInternal ? InternalShippingCharge : ExternalShippingCharge;
                }
                _bus.Send(order.ReplyTo, reply);
                Console.WriteLine("Order processed with deliverytype" +
                    type +
                    " at department" + department.Id);
            }
            else
            {
                //No products of that Id
                _bus.Send(order.ReplyTo, new OrderReply(department.Id, order.Id, null, DeliveryEnum.None, orderType));

                Console.WriteLine("No products found at department" + department.Id);
            }

        }
    }
}
