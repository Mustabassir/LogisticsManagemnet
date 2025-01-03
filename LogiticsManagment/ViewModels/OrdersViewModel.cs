using LogiticsManagment.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LogiticsManagment.ViewModels
{
    public class OrdersViewModel
    {
        public int OrderID { get; set; }
        public DateTime? OrderDate { get; set; } // Allow for nullable DateTime
        public int? CustomerID { get; set; } // Allow for nullable int
        public string Status { get; set; }
        public decimal? DeliveryCharges { get; set; } // Allow for nullable decimal
    }

}