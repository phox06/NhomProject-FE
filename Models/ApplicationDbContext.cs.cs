using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace NhomProject.Models
{
    public class ApplicationDbContext : DbContext
    {
        // The name "DefaultConnection" must match your connection string name in Web.config
        public ApplicationDbContext() : base("name=DefaultConnection")
        {
        }

        // --- Your Models as Database Tables ---
        // EF will create tables named "Categories", "Products", "Users", etc.

        public DbSet<Category> Categories { get; set; }
        public DbSet<Products> Products { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderDetail> OrderDetails { get; set; }
    }
}