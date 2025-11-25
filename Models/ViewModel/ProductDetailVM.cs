using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using NhomProject.Models;
using NhomProject.Models.ViewModel;
using PagedList;


namespace NhomProject.Models.ViewModel
{
    public class ProductDetailVM
    {
        public Product Product { get; set; }
        public int Quantity { get; set; } = 1;
        public decimal EstimatedValue { get; set; }

        
        public List<Product> RelatedProducts { get; set; }

       
        public List<Product> TopProducts { get; set; }
    }
}
