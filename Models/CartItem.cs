using System.ComponentModel.DataAnnotations; // <-- ADD THIS
using System.ComponentModel.DataAnnotations.Schema; // <-- ADD THIS

namespace NhomProject.Models
{
    public class CartItem
    {
        [Key]
        public int Id { get; set; }

        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public string ImageUrl { get; set; }
        public decimal Price { get; set; }
        public int Quantity { get; set; }

        [NotMapped] // Not saved in database
        public decimal Total
        {
            get { return Price * Quantity; }
        }

        // --- THIS IS THE FIX ---
        // Foreign key to link this Item to an Order
        public int OrderId { get; set; }
        [ForeignKey("OrderId")]
        public virtual Order Order { get; set; }
        // -----------------------
    }
}
