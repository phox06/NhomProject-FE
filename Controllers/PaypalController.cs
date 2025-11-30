using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using NhomProject.Models;
using NhomProject.Models.ViewModel; // Required for CartService
using PayPal.Api;
using NhomProject.App_Start;
using System.Globalization;

namespace NhomProject.Controllers
{
    public class PaypalController : Controller
    {
        private MyProjectDatabaseEntities _db = new MyProjectDatabaseEntities();

        // 1. Initialize CartService
        private CartService _cartService = new CartService();

        public ActionResult CreatePayment()
        {
            // 2. Use Service to get the correct Cart (Database or Session)
            var cart = _cartService.GetCart();

            // Get other details from Session
            var shippingDetails = Session["OrderModel"] as NhomProject.Models.Order;
            var userId = Session["UserId"] as int?;

            // Validation: Ensure cart has items
            if (cart == null || cart.Items.Count == 0 || shippingDetails == null || userId == null)
            {
                TempData["Error"] = "Cart is empty or session expired.";
                return RedirectToAction("Cart", "Home");
            }

            var apiContext = PaypalConfiguration.GetAPIContext();

            try
            {
                // 3. Get Total (This will now be correct because we loaded from DB)
                decimal totalVND = cart.GetTotal();

                // Exchange rate
                decimal exchangeRate = 25000m;

                if (totalVND <= 0)
                {
                    TempData["Error"] = "Cannot checkout with a total of 0.";
                    return RedirectToAction("PaymentCancel");
                }

                var itemList = new ItemList() { items = new List<Item>() };
                foreach (var item in cart.Items)
                {
                    decimal itemPriceUSD = item.Price / exchangeRate;

                    itemList.items.Add(new Item()
                    {
                        name = item.ProductName,
                        currency = "USD",
                        price = itemPriceUSD.ToString("F2", CultureInfo.InvariantCulture),
                        quantity = item.Quantity.ToString(),
                        sku = item.ProductId.ToString()
                    });
                }

                decimal subtotalUSD = itemList.items.Sum(i => decimal.Parse(i.price, CultureInfo.InvariantCulture) * int.Parse(i.quantity));

                var details = new Details()
                {
                    subtotal = subtotalUSD.ToString("F2", CultureInfo.InvariantCulture)
                };

                var amount = new Amount()
                {
                    currency = "USD",
                    total = subtotalUSD.ToString("F2", CultureInfo.InvariantCulture),
                    details = details
                };

                var transactionList = new List<Transaction>();
                transactionList.Add(new Transaction()
                {
                    description = "Order #" + Convert.ToString(new Random().Next(100000)),
                    invoice_number = Guid.NewGuid().ToString(),
                    amount = amount,
                    item_list = itemList
                });

                var payment = new Payment()
                {
                    intent = "sale",
                    payer = new Payer() { payment_method = "paypal" },
                    transactions = transactionList,
                    redirect_urls = new RedirectUrls()
                    {
                        return_url = Url.Action("PaymentSuccess", "Paypal", null, Request.Url.Scheme),
                        cancel_url = Url.Action("PaymentCancel", "Paypal", null, Request.Url.Scheme)
                    }
                };

                var createdPayment = payment.Create(apiContext);
                var approvalUrl = createdPayment.links.FirstOrDefault(
                    x => x.rel.Equals("approval_url", StringComparison.OrdinalIgnoreCase));

                Session["paypalPaymentId"] = createdPayment.id;

                return Redirect(approvalUrl.href);
            }
            catch (PayPal.PaymentsException ex)
            {
                TempData["Error"] = ex.Response;
                return RedirectToAction("PaymentCancel");
            }
            catch (Exception ex)
            {
                TempData["Error"] = "An error occurred: " + ex.Message;
                return RedirectToAction("PaymentCancel");
            }
        }

        public ActionResult PaymentSuccess()
        {
            var apiContext = PaypalConfiguration.GetAPIContext();

            try
            {
                string payerId = Request.Params["PayerID"];
                string paymentId = Session["paypalPaymentId"] as string;

                if (string.IsNullOrEmpty(payerId) || string.IsNullOrEmpty(paymentId))
                {
                    TempData["Error"] = "Payment information is missing.";
                    return RedirectToAction("PaymentCancel");
                }

                var paymentExecution = new PaymentExecution() { payer_id = payerId };
                var payment = new Payment() { id = paymentId };

                var executedPayment = payment.Execute(apiContext, paymentExecution);

                if (executedPayment.state.Equals("approved", StringComparison.OrdinalIgnoreCase))
                {
                    // 4. Use Service again to ensure we get the DB cart
                    var cart = _cartService.GetCart();
                    var shippingDetails = Session["OrderModel"] as NhomProject.Models.Order;
                    var userId = (int)Session["UserId"];

                    var order = new NhomProject.Models.Order
                    {
                        CustomerName = shippingDetails.CustomerName,
                        Address = shippingDetails.Address,
                        Phone = shippingDetails.Phone,
                        PaymentMethod = "PayPal",
                        Date = DateTime.Now,
                        Status = "Paid",
                        Total = cart.GetTotal(),
                        UserId = userId
                    };

                    // Initialize lists to avoid errors
                    if (order.CartItems == null) order.CartItems = new List<CartItem>();
                    if (order.OrderDetails == null) order.OrderDetails = new List<OrderDetail>();

                    foreach (var item in cart.Items)
                    {
                        // Save for User History
                        order.CartItems.Add(new CartItem
                        {
                            ProductId = item.ProductId,
                            ProductName = item.ProductName,
                            ImageUrl = item.ImageUrl,
                            Price = item.Price,
                            Quantity = item.Quantity
                        });

                        // 5. CRITICAL: Save for Admin Panel (OrderDetails)
                        order.OrderDetails.Add(new OrderDetail
                        {
                            ProductId = item.ProductId,
                            Quantity = item.Quantity,
                            UnitPrice = item.Price
                        });
                    }

                    _db.Orders.Add(order);
                    _db.SaveChanges();

                    // 6. Use Service to Clear Cart (clears DB too)
                    _cartService.ClearCart();

                    Session["OrderModel"] = null;
                    Session["paypalPaymentId"] = null;

                    return RedirectToAction("OrderConfirmation", "Home", new { id = order.Id });
                }
                else
                {
                    TempData["Error"] = "Payment was not approved by PayPal.";
                    return RedirectToAction("PaymentCancel");
                }
            }
            catch (PayPal.PaymentsException ex)
            {
                TempData["Error"] = ex.Response;
                return RedirectToAction("PaymentCancel");
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error executing payment: " + ex.Message;
                return RedirectToAction("PaymentCancel");
            }
        }

        public ActionResult PaymentCancel()
        {
            Session["paypalPaymentId"] = null;
            if (TempData["Error"] != null)
            {
                ViewBag.Error = TempData["Error"];
            }
            else
            {
                ViewBag.Error = "Payment was canceled.";
            }
            return RedirectToAction("Cart", "Home");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) _db.Dispose();
            base.Dispose(disposing);
        }
    }
}