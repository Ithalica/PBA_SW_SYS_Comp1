using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain
{
    public class Order : BaseOrder
    {
        public Order(int id, Product product, Customer customer) : base(id, product, customer)
        {
        }

        public string ReplyTo { get; set; }
    }
}
