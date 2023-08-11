using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using PCShop.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace PCShop.Areas.Admin.Controllers
{
    [Authorize(Roles = "Admin")]
    public class RoleController : Controller
    {
        private ApplicationDbContext dbContext = new ApplicationDbContext();
        // GET: Admin/Role
        public ActionResult Index()
        {
            var roles = dbContext.Roles.ToList();
            return View(roles);
        }

        public ActionResult Add()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Add(IdentityRole roleName)
        {
            if (ModelState.IsValid)
            {
                var role = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(dbContext));
                role.Create(roleName);
                return RedirectToAction("Index");
            }
            return View();
        }

        public ActionResult Edit(int id)
        {
            var roles = dbContext.Roles.Find(id);

            return View(roles);
        }

        [HttpPost]
        public ActionResult Edit(IdentityRole roleName)
        {
            if (ModelState.IsValid)
            {
                var role = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(dbContext));
                role.Update(roleName);
                return RedirectToAction("Index");
            }
            return View();
        }

        [HttpPost]
        public ActionResult Delete(IdentityRole roleName)
        {
            if (ModelState.IsValid)
            {
                var role = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(dbContext));
                role.Delete(roleName);
                return RedirectToAction("Index");
            }
            return View();
        }
    }
}