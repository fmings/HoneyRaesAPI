using System.Security.Cryptography.X509Certificates;

namespace HoneyRaesAPI.Models
{
    public class Customer
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }
        public List<ServiceTicket> ServiceTickets { get; set; }
    }
}
