using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using System.Globalization; 
using System.Threading;

namespace NhomProject
{
    public class MvcApplication : System.Web.HttpApplication
    {
            protected void Application_Start()
            {
                AreaRegistration.RegisterAllAreas();
                RouteConfig.RegisterRoutes(RouteTable.Routes);
            }

            // --- REPLACE THE OLD METHOD WITH THIS NEW ONE ---
            // This code runs at the beginning of every request
            protected void Application_BeginRequest(object sender, EventArgs e)
            {
                // 1. Create a new, specific CultureInfo object for Vietnamese (Vietnam)
                CultureInfo newCulture = new CultureInfo("vi-VN");

                // 2. Clone its NumberFormat so we can modify it
                NumberFormatInfo nfi = (NumberFormatInfo)newCulture.NumberFormat.Clone();

                // 3. Set the number format to use dots for thousands and the "₫" symbol
                nfi.CurrencySymbol = "₫";
                nfi.CurrencyGroupSeparator = ".";
                nfi.CurrencyDecimalSeparator = ",";

                // 4. Apply the modified format back to our new culture
                newCulture.NumberFormat = nfi;

                // 5. Apply this culture to the current request
                Thread.CurrentThread.CurrentCulture = newCulture;
                Thread.CurrentThread.CurrentUICulture = newCulture;
            }
        }
    }