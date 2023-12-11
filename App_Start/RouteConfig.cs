using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace Attractions
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "Account", action = "Loggin", id = UrlParameter.Optional }
            );
            routes.MapRoute(
                name: "AjaxPage",
                url: "Home/AjaxPage",
                defaults: new { controller = "Home", action = "AjaxPage" }
            );
           

        }
    }
}
