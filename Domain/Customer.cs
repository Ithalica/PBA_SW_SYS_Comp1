namespace Domain
{
    public class Customer
    {
        public Customer(string id, string country)
        {
            Id = id;
            Country = country;
        }
        public string Id { get; set; }
        public string Country { get; set; }
    }
}
