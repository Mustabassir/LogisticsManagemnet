using LogiticsManagment.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LogiticsManagment.ViewModels
{
    public class OrderViewModel
    {
        public OrderViewModel()
        {
            // Initialize properties here
            Customer = new Customer();
            Order = new Order();
            Package = new Package();
            Payment = new Payment();
            Shipment = new Shipment();
        }

        public Customer Customer { get; set; }
        public Order Order { get; set; }
        public Package Package { get; set; }
        public Payment Payment { get; set; }
        public Shipment Shipment { get; set; }
    }
}
