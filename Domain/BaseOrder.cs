using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain
{
    public class BaseOrder
    {
        public BaseOrder(int id, Product product, Customer customer)
        {
            Id = id;
            Product = product;
            Customer = customer;
        }
    public Product Product { get; set; }
    public int Id { get; set; }
    public Customer Customer { get; set; }
}
}
