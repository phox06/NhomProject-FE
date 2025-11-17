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
            
            bool skipAuthorization = filterContext.ActionDescriptor.IsDefined(typeof(AllowAnonymousAttribute), true);

           
            if (skipAuthorization)
            {
                base.OnActionExecuting(filterContext);
                return;
            }

            
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