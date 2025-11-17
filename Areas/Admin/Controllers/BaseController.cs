using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace NhomProject.Areas.Admin.Controllers
{
    public class BaseController : Controller
    {
        // GET: Admin/Base
        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            // Check if the action has the [AllowAnonymous] attribute
            bool skipAuthorization = filterContext.ActionDescriptor.IsDefined(typeof(AllowAnonymousAttribute), true);

            // If it DOES have the attribute (like Login), skip the security check.
            if (skipAuthorization)
            {
                base.OnActionExecuting(filterContext);
                return;
            }

            // --- This is your original security check ---
            // If the attribute is NOT present, run the check as normal.
            var session = filterContext.HttpContext.Session;

            if (session["UserRole"] == null || session["UserRole"].ToString() != "Admin")
            {
                filterContext.Result = new RedirectToRouteResult(
                    new RouteValueDictionary(new
                    {
                        controller = "Home",
                        action = "Login",
                        area = "Admin"
                    }));
            }

            base.OnActionExecuting(filterContext);
        }
    }
}