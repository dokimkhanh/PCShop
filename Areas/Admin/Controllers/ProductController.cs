using PagedList;
using PCShop.Models;
using PCShop.Models.Common;
using PCShop.Models.EF;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.UI;

namespace PCShop.Areas.Admin.Controllers
{
    [Authorize(Roles = "Admin,Nhân viên")]


    public class ProductController : Controller
    {
        private ApplicationDbContext dbContext = new ApplicationDbContext();
        // GET: Admin/Product
        public ActionResult Index(string txtSearch, int? page)
        {
            int pageSize = 5;
            if (page == null)
            {
                page = 1;
            }

            IEnumerable<Product> productList = dbContext.products.OrderByDescending(x => x.id);
            if (!string.IsNullOrEmpty(txtSearch))
            {
                productList = productList.Where(x => x.title.ToLower().Contains(txtSearch));
            }

            int pageIndex = page.HasValue ? Convert.ToInt32(page) : 1;
            productList = productList.ToPagedList(pageIndex, pageSize);
            ViewBag._pageSize = pageSize;
            ViewBag._page = page;

            return View(productList);
        }

        public ActionResult Add()
        {
            ViewBag.productCategoryID = new SelectList(dbContext.categories.ToList(), "id", "title");
            return View();
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Add(Product product, List<string> listImage, List<int> rdDefault)
        {
            if (ModelState.IsValid)
            {
                if (listImage.Count > 0 && listImage != null)
                {
                    int i = 1;
                    foreach (var item in listImage)
                    {
                        bool checkDefaul = (i == rdDefault[0]) ? true : false;
                        product.productImages.Add(new ProductImage
                        {
                            ProductId = product.id,
                            image = item,
                            isDefault = checkDefaul
                        });
                        i++;
                    }
                }
                product.alias = CommonHelper.FilterChar(product.title);
                product.createdDate = DateTime.Now;
                product.createdBy = "Khánh tạo";
                product.modifiedDate = DateTime.Now;

                dbContext.products.Add(product);
                dbContext.SaveChanges();

                return RedirectToAction("Index");
            }
            ViewBag.productCategoryID = new SelectList(dbContext.categories.ToList(), "id", "title");
            return View(product);
        }

        public ActionResult Edit(int id)
        {
            ViewBag.productCategoryID = new SelectList(dbContext.categories.ToList(), "id", "title");
            var item = dbContext.products.Find(id);
            return View(item);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Product product)
        {
            if (ModelState.IsValid)
            {
                product.modifiedBy = "Khánh sửa";

                Product n = dbContext.products.Where(i => i.id == product.id).FirstOrDefault();
                n.title = product.title;
                n.description = product.description;
                n.detail = product.detail;
                n.modifiedDate = DateTime.Now;
                n.modifiedBy = product.modifiedBy;
                n.alias = CommonHelper.FilterChar(product.title);
                n.price = product.price;
                n.priceSale = product.priceSale;
                n.quantity = product.quantity;
                n.isNew = product.isNew;
                n.isSale = product.isSale;
                n.categoryID = product.categoryID;
                dbContext.SaveChanges();
                return RedirectToAction("Index");

            }
            return View(product);
        }

        [HttpPost]
        public ActionResult Delete(int id)
        {
            var item = dbContext.products.Find(id);
            if (item != null)
            {
                dbContext.products.Remove(item);
                dbContext.productImages.RemoveRange(dbContext.productImages.Where(x => x.ProductId == id));
                dbContext.SaveChanges();
                return Json(new { success = true });
            }
            return Json(new { success = false });

        }
    }
}