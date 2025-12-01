using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using NhomProject.Models;
using NhomProject.Models.ViewModel;

namespace NhomProject.Models.ViewModel
{
    public class CartService
    {
        private readonly MyProjectDatabaseEntities _db = new MyProjectDatabaseEntities();

        public Cart GetCart()
        {
            Cart cart = null;

            if (HttpContext.Current.Session["UserId"] != null)
            {
                int userId = (int)HttpContext.Current.Session["UserId"];

                List<NhomProject.Models.CartItem> dbCartItems = _db.CartItems.Where(c => c.UserId == userId).ToList();

                cart = new Cart();
                foreach (var item in dbCartItems)
                {
                    cart.Items.Add(new CartItem
                    {
                        ProductId = item.ProductId,
                        ProductName = item.ProductName,
                        ImageUrl = item.ImageUrl,
                        Price = item.Price,
                        Quantity = item.Quantity
                    });
                }
            }
            else
            {
                cart = HttpContext.Current.Session["Cart"] as Cart;
            }

            if (cart == null)
            {
                cart = new Cart();
                HttpContext.Current.Session["Cart"] = cart;
            }

            return cart;
        }

        public void AddToCart(Product product, int quantity)
        {
            Cart cart = GetCart();
            cart.AddItem(product, quantity); 

            if (HttpContext.Current.Session["UserId"] != null)
            {
                int userId = (int)HttpContext.Current.Session["UserId"];

                var dbItem = _db.CartItems.FirstOrDefault(c => c.UserId == userId && c.ProductId == product.ProductId);

                if (dbItem == null)
                {
                   
                    var newItem = new NhomProject.Models.CartItem
                    {
                        UserId = userId,
                        ProductId = product.ProductId,
                        ProductName = product.Name,
                        Price = product.Price,
                        Quantity = quantity,
                        ImageUrl = product.MainImageUrl,
                        OrderId = null 
                    };
                    _db.CartItems.Add(newItem);
                }
                else
                {
                    dbItem.Quantity += quantity;
                }
                _db.SaveChanges();
            }
            else
            {
                HttpContext.Current.Session["Cart"] = cart;
            }
        }

        public void RemoveFromCart(int productId)
        {
            Cart cart = GetCart();
            cart.RemoveItem(productId);

            if (HttpContext.Current.Session["UserId"] != null)
            {
                int userId = (int)HttpContext.Current.Session["UserId"];
                var dbItem = _db.CartItems.FirstOrDefault(c => c.UserId == userId && c.ProductId == productId);
                if (dbItem != null)
                {
                    _db.CartItems.Remove(dbItem);
                    _db.SaveChanges();
                }
            }
            else
            {
                HttpContext.Current.Session["Cart"] = cart;
            }
        }

        public void ClearCart()
        {
            Cart cart = GetCart();
            cart.Clear();

            if (HttpContext.Current.Session["UserId"] != null)
            {
                int userId = (int)HttpContext.Current.Session["UserId"];
                
                var dbItems = _db.CartItems.Where(c => c.UserId == userId);
                _db.CartItems.RemoveRange(dbItems);
                _db.SaveChanges();
            }
            else
            {
                HttpContext.Current.Session["Cart"] = null;
            }
        }
    }
}