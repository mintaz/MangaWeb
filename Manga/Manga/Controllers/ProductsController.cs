using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Manga.Filter;
using Manga.Models;

namespace Manga.Controllers
{
    [Authorize]
    public class ProductsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: Products
        [MVCAuthPermission("Product.Read")]
        public ActionResult Index()
        {
            return View(db.Product.ToList());
        }

        [MVCAuthPermission("Product.Read")]
        // GET: Products/Details/5
        public ActionResult Detail(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Product product = db.Product.Find(id);
            var catelogries = db.Product.Where(s => s.Id == id).SelectMany(c => c.Categories).ToList();
            if (product == null)
            {
                return HttpNotFound();
            }
            ViewBag.Categories = catelogries;
            ViewBag.CategoriesCount = catelogries.Count();
            return View(product);
        }

        // GET: Products/Create
        [MVCAuthPermission("Product.Create")]
        public ActionResult Create()
        {
            ViewBag.CategoriesId = new SelectList(db.Category.ToList(),"Id","Title");
            return View();
        }

        // POST: Products/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [MVCAuthPermission("Product.Create")]
        public ActionResult Create([Bind(Include = "Id,Name")] Product product, params int[] SelectedCategories)
        {
            if (ModelState.IsValid)
            {
                UpdateCategories(SelectedCategories, product);
                db.Product.Add(product);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(product);
        }

        // GET: Products/Edit/5
        [MVCAuthPermission("Product.Edit")]
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Product product = db.Product.Find(id);
            var catelogries = db.Product.Where(s => s.Id == id).SelectMany(c => c.Categories).ToList();
            var list = db.Category.AsEnumerable().Select(x => new SelectListItem
            {
                Selected = catelogries.Contains(x),
                Text = x.Title,
                Value = x.Id.ToString()
            });
            ViewBag.CatagoryList = list;
            return View(product);
        }

        // POST: Products/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [MVCAuthPermission("Product.Edit")]
        public ActionResult Edit([Bind(Include = "Id,Name")] Product product, int[] selectCategories)
        {
            if (ModelState.IsValid)
            {
                var productEdit = db.Product.Where(x => x.Id == product.Id).Include(c => c.Categories).SingleOrDefault();
                UpdateCategories(selectCategories, productEdit);
                productEdit.Name = product.Name;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(product);
        }

        // GET: Products/Delete/5
        [MVCAuthPermission("Product.Delete")]
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Product product = db.Product.Find(id);
            if (product == null)
            {
                return HttpNotFound();
            }
            return View(product);
        }

        [MVCAuthPermission("Product.Delete")]
        // POST: Products/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Product product = db.Product.Find(id);
            db.Product.Remove(product);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
        private void UpdateCategories(int[] selectedCategories, Product productToUpdate)
        {
            if (selectedCategories == null)
            {
                productToUpdate.Categories = new List<Category>();
                return;
            }

            var selectedCategoriesHS = new HashSet<int>(selectedCategories);
            var productCategories = new HashSet<int>(productToUpdate.Categories.Select(c =>c.Id));

            foreach (var item in db.Category)
            {
                if (selectedCategoriesHS.Contains(item.Id))
                {
                    if (!productCategories.Contains(item.Id))
                    {
                        productToUpdate.Categories.Add(item);
                    }
                }
                else
                {
                    if (productCategories.Contains(item.Id))
                    {
                        productToUpdate.Categories.Remove(item);
                    }
                }
            }

        }
    }
}
