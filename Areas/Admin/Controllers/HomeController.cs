using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using PCShop.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;

namespace PCShop.Areas.Admin.Controllers
{
    [Authorize(Roles = "Admin,Nhân viên")]


    public class HomeController : Controller
    {
        private ApplicationDbContext dbContext = new ApplicationDbContext();
        // GET: Admin/Home
        public ActionResult Index()
        {
            var orders = dbContext.orders.Where(x => x.createdDate == DateTime.Today).Count();
            ViewBag.Orders = orders > 0 ? orders : 0;

            var total_income_today = dbContext.orders.Where(x => x.createdDate == DateTime.Today).Count();
            ViewBag.IncomeToday = total_income_today > 0 ? total_income_today : 0;

            //Get admin role
            var roleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(dbContext));
            var cusRole = roleManager.FindByName("Khách hàng");
            var customers = dbContext.Users.Where(x => x.Roles.Any(role => role.RoleId == cusRole.Id)).Count();
            ViewBag.Customers = customers > 0 ? customers : 0;

            var products = dbContext.products.Count();
            ViewBag.Products = products > 0 ? products : 0;

            return View();
        }

        public ActionResult Abc()
        {
            return View();
        }
    }
}