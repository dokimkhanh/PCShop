using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace PCShop.Areas.Admin.Controllers
{
    [Authorize(Roles = "Admin,Nhân viên")]

    public class AdvController : Controller
    {
        // GET: Admin/Adv
        public ActionResult Index()
        {
            return View();
        }
    }
}