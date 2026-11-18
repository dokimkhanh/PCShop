using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Newtonsoft.Json;
using PCShop.Models;
using PCShop.Models.Common;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;

namespace PCShop.Areas.Admin.Controllers
{
    [RequirePermission("Dashboard.View")]
    public class HomeController : Controller
    {
        private ApplicationDbContext dbContext = new ApplicationDbContext();

        public ActionResult Index()
        {
            var today = DateTime.Today;
            var tomorrow = today.AddDays(1);
            var sevenDaysAgo = today.AddDays(-6);

            var ordersToday = dbContext.orders
                .Count(x => x.createdDate >= today && x.createdDate < tomorrow);
            ViewBag.Orders = ordersToday;

            var incomeToday = dbContext.orders
                .Where(x => x.createdDate >= today && x.createdDate < tomorrow && x.paymentStatus == 1)
                .Select(x => (int?)x.totalAmount)
                .Sum() ?? 0;
            ViewBag.IncomeToday = incomeToday;

            var roleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(dbContext));
            var cusRole = roleManager.FindByName("Customer");
            var customers = 0;
            if (cusRole != null && !string.IsNullOrEmpty(cusRole.Id))
            {
                customers = dbContext.Users.Count(x => x.Roles.Any(role => role.RoleId == cusRole.Id));
            }
            ViewBag.Customers = customers;

            var products = dbContext.products.Count();
            ViewBag.Products = products;

            var pendingOrdersCount = dbContext.orders.Count(x => x.paymentStatus == 0);
            ViewBag.PendingOrdersCount = pendingOrdersCount;

            var lowStockCount = dbContext.products.Count(x => x.quantity <= 5);
            ViewBag.LowStockCount = lowStockCount;

            var paidOrders7Days = dbContext.orders
                .AsNoTracking()
                .Where(x => x.createdDate >= sevenDaysAgo && x.createdDate < tomorrow && x.paymentStatus == 1)
                .Select(x => new { x.createdDate, x.totalAmount })
                .ToList();

            var labels = new List<string>();
            var values = new List<int>();
            var total7Days = 0;
            for (var offset = 0; offset < 7; offset++)
            {
                var date = sevenDaysAgo.AddDays(offset);
                var nextDate = date.AddDays(1);
                var daySum = paidOrders7Days
                    .Where(x => x.createdDate >= date && x.createdDate < nextDate)
                    .Sum(x => x.totalAmount);
                labels.Add(date.ToString("dd/MM"));
                values.Add(daySum);
                total7Days += daySum;
            }

            ViewBag.RevenueLabelsJson = JsonConvert.SerializeObject(labels);
            ViewBag.RevenueValuesJson = JsonConvert.SerializeObject(values);
            ViewBag.Revenue7DaysTotal = total7Days;

            ViewBag.PendingOrders = dbContext.orders
                .AsNoTracking()
                .Where(x => x.paymentStatus == 0)
                .OrderByDescending(x => x.createdDate)
                .Take(5)
                .ToList();

            ViewBag.LowStockProducts = dbContext.products
                .AsNoTracking()
                .Where(x => x.quantity <= 5)
                .OrderBy(x => x.quantity)
                .Take(5)
                .ToList();

            return View();
        }

        public ActionResult Abc()
        {
            return View();
        }
    }
}
