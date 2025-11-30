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
                // User is logged in: Load from DATABASE
                int userId = (int)HttpContext.Current.Session["UserId"];

                // Use NhomProject.Models.CartItem explicitly to avoid confusion
                List<NhomProject.Models.CartItem> dbCartItems = _db.CartItems.Where(c => c.UserId == userId).ToList();

                cart = new Cart();
                foreach (var item in dbCartItems)
                {
                    // Here we map Database Item -> ViewModel Item
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
                // User is guest: Load from SESSION
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
            cart.AddItem(product, quantity); // Updates the session/viewmodel cart

            // If logged in, ALSO update the database
            if (HttpContext.Current.Session["UserId"] != null)
            {
                int userId = (int)HttpContext.Current.Session["UserId"];

                // Check DB for existing item
                var dbItem = _db.CartItems.FirstOrDefault(c => c.UserId == userId && c.ProductId == product.ProductId);

                if (dbItem == null)
                {
                    // FIX: Use 'NhomProject.Models.CartItem' (The Database Entity)
                    // FIX: Use 'new CartItem' (Singular), not 'CartItems'
                    var newItem = new NhomProject.Models.CartItem
                    {
                        UserId = userId,
                        ProductId = product.ProductId,
                        ProductName = product.Name,
                        Price = product.Price,
                        Quantity = quantity,
                        ImageUrl = product.MainImageUrl,
                        OrderId = null // CHANGE THIS: Do not link to an Order yet!
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