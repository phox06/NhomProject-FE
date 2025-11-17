using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using NhomProject.Models;
using NhomProject.Models.ViewModel;

namespace NhomProject.Models.ViewModel
{
    public class CategoryStatisticVM
    {
        public string CategoryName { get; set; }
        public int ProductCount { get; set; }
        public decimal HighestPrice { get; set; }
        public decimal LowestPrice { get; set; }
        public decimal AveragePrice { get; set; }
    }
}