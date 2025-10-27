using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace NhomProject.Models
{
    public class Order
    {
        public string CustomerName { get; set; }
        public string Address { get; set; }
        public string Phone { get; set; }

        [Key]
        public int Id { get; set; }

        public decimal TotalAmount { get; set; }
        public string Status { get; set; }
        public List<CartItem> Items { get; set; } = new List<CartItem>();
        public DateTime Date { get; set; }
        public string PaymentMethod { get; set; }
        public decimal Total { get; set; }
        public int? UserId { get; set; } 
        [ForeignKey("UserId")]
        public virtual User User { get; set; }
       
      
    }
}