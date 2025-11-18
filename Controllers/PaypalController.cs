using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using NhomProject.Models;
using PayPal.Api;
using NhomProject.App_Start;
using System.Globalization;

namespace NhomProject.Controllers
{
    public class PaypalController : Controller
    {
        private MyProjectDatabaseEntities _db = new MyProjectDatabaseEntities();

        public ActionResult CreatePayment()
        {
            var cart = Session["Cart"] as Cart;
            var shippingDetails = Session["OrderModel"] as NhomProject.Models.Order;
            var userId = Session["UserId"] as int?;

            if (cart == null || shippingDetails == null || userId == null)
            {
                return RedirectToAction("Checkout", "Home");
            }

            var apiContext = PaypalConfiguration.GetAPIContext();
            string payerId = Request.Params["PayerID"];

            try
            {
                var total = cart.GetTotal();

                if (total <= 0)
                {
                    TempData["Error"] = "Cannot checkout with a total of 0. Please add items to your cart.";
                    return RedirectToAction("PaymentCancel");
                }

                var itemList = new ItemList() { items = new List<Item>() };
                foreach (var item in cart.Items)
                {
                    itemList.items.Add(new Item()
                    {
                        name = item.ProductName,
                        currency = "USD",
                        price = item.Price.ToString("F2", CultureInfo.InvariantCulture),
                        quantity = item.Quantity.ToString(),
                        sku = item.ProductId.ToString()
                    });
                }

                
                var details = new Details()
                {
                    
                    subtotal = total.ToString("F2", CultureInfo.InvariantCulture)
                };

                
                var amount = new Amount()
                {
                    currency = "USD",
                    
                    total = total.ToString("F2", CultureInfo.InvariantCulture),
                    details = details
                };

                
                var transactionList = new List<Transaction>();
                transactionList.Add(new Transaction()
                {
                    description = "Test order description.",
                    invoice_number = Convert.ToString(new Random().Next(100000)), 
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
                    TempData["Error"] = "Payment information is missing. Session may have expired.";
                    return RedirectToAction("PaymentCancel");
                }

                
                var paymentExecution = new PaymentExecution() { payer_id = payerId };
                var payment = new Payment() { id = paymentId };

                var executedPayment = payment.Execute(apiContext, paymentExecution);

                if (executedPayment.state.Equals("approved", StringComparison.OrdinalIgnoreCase))
                {
                    var cart = Session["Cart"] as Cart;
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

                    foreach (var item in cart.Items)
                    {
                        order.CartItems.Add(new CartItem
                        {
                            ProductId = item.ProductId,
                            ProductName = item.ProductName,
                            ImageUrl = item.ImageUrl,
                            Price = item.Price,
                            Quantity = item.Quantity
                        });
                    }

                    _db.Orders.Add(order);
                    _db.SaveChanges();

                    Session["Cart"] = null;
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
            if (disposing)
            {
                _db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}