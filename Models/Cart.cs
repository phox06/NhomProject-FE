using System;
using System.Collections.Generic;
using System.Linq;

namespace NhomProject.Models
{
    public class Cart
    {
        public List<CartItem> Items { get; private set; }

        public Cart()
        {
            Items = new List<CartItem>();
        }

        public void AddItem(Product product, int quantity)
        {
            
            var item = Items.FirstOrDefault(p => p.ProductId == product.ProductId);

            if (item == null)
            {
               
                Items.Add(new CartItem
                {
                    ProductId = product.ProductId,
                    ProductName = product.Name,
                    ImageUrl = product.MainImageUrl,
                    Price = product.Price,
                    Quantity = quantity
                });
            }
            else
            {
                
                item.Quantity += quantity;
            }
        }

        public void RemoveItem(int productId)
        {
            Items.RemoveAll(p => p.ProductId == productId);
        }

        public decimal GetTotal()
        {
            return Items.Sum(p => p.Quantity * p.Price);
        }
        public void UpdateQuantity(int productId, int quantity)
        {
            var item = Items.FirstOrDefault(p => p.ProductId == productId);
            if (item != null)
            {
                if (quantity > 0)
                {
                    item.Quantity = quantity;
                }
                else
                {
                    
                    Items.Remove(item);
                }
            }
        }
    }
}