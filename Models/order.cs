using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace NhomProject.Models
{
    public class Order
    {
        public string CustomerName { get; set; }
        public string Address { get; set; }
        public string Phone { get; set; }

        // public string ID { get; set; } // <-- DELETE THIS LINE

        [Key]
        public int Id { get; internal set; }

        public decimal TotalAmount { get; set; }
        public string Status { get; set; }

        public List<CartItem> Items { get; set; } = new List<CartItem>();
        public DateTime Date { get; internal set; }
        public string PaymentMethod { get; internal set; }
        public decimal Total { get; internal set; }
    }
}