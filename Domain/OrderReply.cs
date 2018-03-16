using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain
{
    public class OrderReply
    {
        public OrderReply(int warehouseId, int orderId, Product product, DeliveryEnum deliveryType, string orderType)
        {
            WarehouseId = warehouseId;
            OrderId = orderId;
            Product = product;
            DeliveryType = deliveryType;
            OrderType = orderType;
        }
        public string OrderType { get; set; }
        public int WarehouseId { get; set; }
        public int OrderId { get; set; }
        public Decimal? ShippingFee {get; set;}
        public int? DeliveryTime { get; set; }
        public DeliveryEnum DeliveryType { get; set; }
        public Product Product { get; set; }
    }
}
