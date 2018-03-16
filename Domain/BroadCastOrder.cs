using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain
{
    public class BroadCastOrder : Order
    {
        public BroadCastOrder(int id, Product product, Customer customer) : base(id, product, customer)
        {
        }
    }
}
