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
            // Get Cart and Shipping Details from Session
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

                // NEW CHECK: PayPal will fail if the total is 0
                if (total <= 0)
                {
                    TempData["Error"] = "Cannot checkout with a total of 0. Please add items to your cart.";
                    return RedirectToAction("PaymentCancel");
                }

                // Create a list of PayPal items
                var itemList = new ItemList() { items = new List<Item>() };
                foreach (var item in cart.Items)
                {
                    itemList.items.Add(new Item()
                    {
                        name = item.ProductName,
                        currency = "USD",
                        // THE FIX: Force dot decimal separator
                        price = item.Price.ToString("F2", CultureInfo.InvariantCulture),
                        quantity = item.Quantity.ToString(),
                        sku = item.ProductId.ToString()
                    });
                }

                // Create payment details
                var details = new Details()
                {
                    // THE FIX: Force dot decimal separator
                    subtotal = total.ToString("F2", CultureInfo.InvariantCulture)
                };

                // Create amount
                var amount = new Amount()
                {
                    currency = "USD",
                    // THE FIX: Force dot decimal separator
                    total = total.ToString("F2", CultureInfo.InvariantCulture),
                    details = details
                };

                // Create transaction
                var transactionList = new List<Transaction>();
                transactionList.Add(new Transaction()
                {
                    description = "Test order description.",
                    invoice_number = Convert.ToString(new Random().Next(100000)), // A unique invoice number
                    amount = amount,
                    item_list = itemList
                });

                // Create payment
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

                // Create the payment and get the approval URL
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

                // Execute the payment
                var paymentExecution = new PaymentExecution() { payer_id = payerId };
                var payment = new Payment() { id = paymentId };

                var executedPayment = payment.Execute(apiContext, paymentExecution);

                if (executedPayment.state.Equals("approved", StringComparison.OrdinalIgnoreCase))
                {
                    // PAYMENT SUCCEEDED - Create the order in your database
                    var cart = Session["Cart"] as Cart;
                    var shippingDetails = Session["OrderModel"] as NhomProject.Models.Order;
                    var userId = (int)Session["UserId"];

                    // Create the final order object
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

                    // Add cart items to the order
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

                    // Clear session data
                    Session["Cart"] = null;
                    Session["OrderModel"] = null;
                    Session["paypalPaymentId"] = null;

                    // Redirect to confirmation page
                    return RedirectToAction("OrderConfirmation", "Home", new { id = order.Id });
                }
                else
                {
                    TempData["Error"] = "Payment was not approved by PayPal.";
                    return RedirectToAction("PaymentCancel");
                }
            }
            // NEW CATCH BLOCK
            catch (PayPal.PaymentsException ex)
            {
                TempData["Error"] = ex.Response;
                return RedirectToAction("PaymentCancel"); // <-- SET BREAKPOINT HERE
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error executing payment: " + ex.Message;
                return RedirectToAction("PaymentCancel");
            }
        }

        public ActionResult PaymentCancel()
        {
            // Clear any lingering PayPal session
            Session["paypalPaymentId"] = null;

            // Show an error on the cart page
            if (TempData["Error"] != null)
            {
                ViewBag.Error = TempData["Error"];
            }
            else
            {
                ViewBag.Error = "Payment was canceled.";
            }

            // Redirect back to the cart to try again
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