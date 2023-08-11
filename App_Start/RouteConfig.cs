using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace PCShop
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute(
                name: "Contact",
                url: "lien-he",
                defaults: new { controller = "Contact", action = "Index" },
                namespaces: new[] { "PCShop.Controllers" }
            );

            routes.MapRoute(
                name: "News",
                url: "tin-tuc",
                defaults: new { controller = "News", action = "Index", alias = UrlParameter.Optional },
                namespaces: new[] { "PCShop.Controllers" }

            );

            routes.MapRoute(
                name: "NewsDetail",
                url: "tin-tuc/{alias}-n{id}",
                defaults: new { controller = "News", action = "NewsDetail", alias = UrlParameter.Optional },
                namespaces: new[] { "PCShop.Controllers" }

            );

            routes.MapRoute(
                name: "ProductDetail",
                url: "san-pham/{alias}-p{id}",
                defaults: new { controller = "Product", action = "ProductDetail", alias = UrlParameter.Optional },
                namespaces: new[] { "PCShop.Controllers" }

            );

            routes.MapRoute(
                name: "Products",
                url: "san-pham",
                defaults: new { controller = "Product", action = "Index", alias = UrlParameter.Optional },
                namespaces: new[] { "PCShop.Controllers" }

            );

            routes.MapRoute(
               name: "ProductCategory",
               url: "danh-muc/{alias}/{id}",
               defaults: new { controller = "Product", action = "ProductCategory", id = UrlParameter.Optional },
               namespaces: new[] { "PCShop.Controllers" }

           );

            routes.MapRoute(
               name: "CheckOut",
               url: "thanh-toan",
               defaults: new { controller = "Cart", action = "CheckOut", id = UrlParameter.Optional },
               namespaces: new[] { "PCShop.Controllers" }

           );

            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional },
                namespaces: new[] { "PCShop.Controllers" }

            );


        }
    }
}
