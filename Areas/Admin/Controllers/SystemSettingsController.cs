using PCShop.Models;
using PCShop.Models.EF;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace PCShop.Areas.Admin.Controllers
{
    public class SystemSettingsController : Controller
    {
        private ApplicationDbContext _dbContext = new ApplicationDbContext();
        // GET: Admin/SystemSettings
        public ActionResult Index()
        {
            var settings = _dbContext.systemSettings.ToList();
            return View(settings);
        }

        public ActionResult Add()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Add(SystemSettings systemSettings)
        {
            if (ModelState.IsValid)
            {
                if (systemSettings != null)
                {
                    _dbContext.systemSettings.Add(new SystemSettings
                    {
                        settingKey = systemSettings.settingKey,
                        settingValue = systemSettings.settingValue,
                        settingDescription = systemSettings.settingDescription
                    });
                    _dbContext.SaveChanges();
                    return RedirectToAction("Index");
                }
            }
            return View(systemSettings);
        }

        public ActionResult Edit(int id)
        {
            var item = _dbContext.systemSettings.Find(id);
            return View(item);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(SystemSettings model)
        {
            if (ModelState.IsValid)
            {
                if (model != null)
                {
                    var n = _dbContext.systemSettings.Where(i => i.id == model.id).FirstOrDefault();
                    //n.settingKey = model.settingKey;
                    n.settingValue = model.settingValue;
                    n.settingDescription = model.settingDescription;
                    _dbContext.SaveChanges();
                    return RedirectToAction("Index");
                }
            }
            return View(model);
        }

        [HttpPost]
        public ActionResult Delete(int id)
        {
            var item = _dbContext.systemSettings.Find(id);
            if (item != null)
            {
                var delete = _dbContext.systemSettings.Remove(item);
                _dbContext.SaveChanges();
                return Json(new { success = true });
            }
            return Json(new { success = false });
        }
    }
}