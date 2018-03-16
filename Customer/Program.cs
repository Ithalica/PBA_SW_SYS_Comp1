using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Customer
{
    class Program
    {
        static void Main(string[] args)
        {
            var messageSystem = new CustomerMessageSystem();
            Task.Factory.StartNew(() => messageSystem.CreateOrder(new Domain.Customer("1", "DK"), new Domain.Product() { Id = 2, Amount = 1  }));            
            Task.Factory.StartNew(() => messageSystem.CreateOrder(new Domain.Customer("2", "FR"), new Domain.Product() { Id = 4, Amount = 22 }));            
            Task.Factory.StartNew(() => messageSystem.CreateOrder(new Domain.Customer("3", "US"), new Domain.Product() { Id = 1, Amount = 10 }));
            Console.ReadLine();
        }
    }
}
