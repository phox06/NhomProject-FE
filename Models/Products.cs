using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace NhomProject.Models
{
    public class Products
    {
        [Key]
        public int ProductId { get; set; } // Primary Key

        [Required]
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public decimal? OldPrice { get; set; } // Nullable
        public string ImageUrl { get; set; }

        // Properties for rating
        public decimal Rating { get; set; }
        public int ReviewCount { get; set; }

        // Foreign key for Category
        public int CategoryId { get; set; }
        public virtual Category Category { get; set; }
    }
}