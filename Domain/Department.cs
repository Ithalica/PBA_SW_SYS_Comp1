using System.Collections.Generic;

namespace Domain
{
    public class Department
    {

        public int Id { get; set; }
        public string Country { get; set; }
        public List<Product> Products { get; set; } = new List<Product>();

        public Department(int id, string country, List<Product> products)
        {
            Id = id;
            Country = country;
            Products = products;

        }
    }
}
