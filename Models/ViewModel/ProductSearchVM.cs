using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using PagedList;
using NhomProject.Models;

namespace NhomProject.Models.ViewModel
{
    public class ProductSearchVM
    {
        public string SearchTerm { get; set; }

        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }

        public string SortOrder { get; set; }

        public int PageNumber { get; set; } 
        public int PageSize { get; set; } = 10; 

        public IPagedList<Product> Products { get; set; }
    }
}

