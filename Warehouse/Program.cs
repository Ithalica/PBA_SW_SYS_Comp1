using Domain;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Warehouse
{
    class Program
    {
        static void Main(string[] args)
        {
            List<Product> dkProducts = new List<Product>{
                new Product { Id = 1, Amount = 10 },
                new Product { Id = 2, Amount = 2 }
            };

            List<Product> frProducts = new List<Product>{
                new Product { Id = 1, Amount = 2 },
                new Product { Id = 2, Amount = 0 }
            };
            List<Product> usProducts = new List<Product>{
                new Product { Id = 1, Amount = 0 },
                new Product { Id = 2, Amount = 2 },
                new Product { Id = 3, Amount = 5 }
            };

            var gateway = new DepartmentGateway();
            Task.Factory.StartNew(() => gateway.DoStuff(new Department(1, "DK", dkProducts)));
            Task.Factory.StartNew(() => gateway.DoStuff(new Department(2, "FR", frProducts)));
            Task.Factory.StartNew(() => gateway.DoStuff(new Department(3, "US", usProducts)));

            Console.ReadLine();
        }
    }
}
