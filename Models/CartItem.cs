using System.ComponentModel.DataAnnotations; // <-- ADD THIS
using System.ComponentModel.DataAnnotations.Schema; // <-- ADD THIS

namespace NhomProject.Models
{
    public class CartItem
    {
        [Key] // <-- ADD THIS. This is the new primary key.
        public int Id { get; set; }

        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public string ImageUrl { get; set; }
        public decimal Price { get; set; }
        public int Quantity { get; set; }

        // Read-only property to calculate the total for this item
        [NotMapped] // <-- ADD THIS. Tells EF not to save this in the database.
        public decimal Total
        {
            get { return Price * Quantity; }
        }
    }
}