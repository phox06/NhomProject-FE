using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace NhomProject.Models
{
    public class HomeViewModel
    {
        public IEnumerable<Products> FlashSaleProducts { get; set; }
        public IEnumerable<Products> PhoneProducts { get; set; }
        public IEnumerable<Products> LaptopProducts { get; set; }
    }
}