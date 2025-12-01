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

           
            protected void Application_BeginRequest(object sender, EventArgs e)
            {
                CultureInfo newCulture = new CultureInfo("vi-VN");

                NumberFormatInfo nfi = (NumberFormatInfo)newCulture.NumberFormat.Clone();

                nfi.CurrencySymbol = "₫";
                nfi.CurrencyGroupSeparator = ".";
                nfi.CurrencyDecimalSeparator = ",";

                newCulture.NumberFormat = nfi;

                Thread.CurrentThread.CurrentCulture = newCulture;
                Thread.CurrentThread.CurrentUICulture = newCulture;
            }
        }
    }