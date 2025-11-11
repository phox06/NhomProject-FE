using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace NhomProject.Models
{
    public class HomeViewModel
    {
        public IEnumerable<Product> FlashSaleProducts { get; set; }
        public IEnumerable<Product> PhoneProducts { get; set; }
        public IEnumerable<Product> LaptopProducts { get; set; }
    }
}