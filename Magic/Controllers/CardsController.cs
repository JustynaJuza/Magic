using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Magic.Models;
using Magic.Models.DataContext;

namespace Magic.Controllers
{   
    public class CardsController : Controller
    {
        private MagicDBContext context = new MagicDBContext();

        [HttpGet]
        public ViewResult Index()
        {
            return View(context.AllCards.ToList());
        }

		#region CREATE
		[HttpGet]
        public ActionResult Create()
        {
            return View();
        } 

        [HttpPost]
        public ActionResult Create(Card actionItem)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    context.Entry(actionItem).State = EntityState.Added;
                    context.SaveChanges();
                }
                catch (Exception ex)
                {
                    TempData["Error"] = "Something went wrong here... That's quite unusual, maybe try again."
                    + "\n (Or, if you are PRO, open the console and send us the error log ;)";
                    ViewBag.ErrorLog = ex.ToFullString();
                    ViewBag.ErrorLog2 = ex.ToString();
                }

                return RedirectToAction("Index");  
            }
            else
            {
                ModelState.AddModelError("", "Please enter correct values to proceed.");
            }

            return View(actionItem);
        }
		#endregion

		#region EDIT/UPDATE
		[HttpGet]
        public ActionResult Edit(Card actionItem)
        {
            var foundItem = context.AllCards.FirstOrDefault(i => i.Id == actionItem.Id);
            if (foundItem == null)
            {
                TempData["Error"] = "This card seems to no longer be there... It has probably been deleted in the meanwhile.";
                return RedirectToAction("Index");
                //return HttpNotFound();
            }

            return View(foundItem);
        }

        [HttpPost]
        public ActionResult PostEdit(Card actionItem)
        {
            if (ModelState.IsValid)
            {
                var foundItem = context.Entry(actionItem);
                if (foundItem == null)
                {
                    TempData["Error"] = "Your changes could not be saved... The card has probably been deleted in the meanwhile.";
                    return RedirectToAction("Index");
                }

                try
                {
                    context.Entry(actionItem).State = EntityState.Modified;
                    context.SaveChanges();
                }
                catch (Exception ex)
                {
                    TempData["Error"] = "Something went wrong here... That's quite unusual, maybe try again.";
                    ViewBag.ErrorLog = ex.ToFullString();
                    ViewBag.ErrorLog2 = ex.ToString();
                }
                //catch (System.Data.Entity.Infrastructure.DbUpdateConcurrencyException dbcex) { }
                return RedirectToAction("Index");
            }
            else
            {
                ModelState.AddModelError("", "Please enter correct values to proceed.");
            }

            return View(actionItem);
        }
		#endregion

		#region DELETE
        public ActionResult Delete(Card actionItem)
        {
            var foundItem = context.AllCards.FirstOrDefault(i => i.Id == actionItem.Id);
            if (foundItem == null)
            {
                TempData["Error"] = "This card seems to no longer be there... It has probably been deleted in the meanwhile.";
                return RedirectToAction("Index");
                //return HttpNotFound();
            }

            try
            {
                context.Entry(foundItem).State = EntityState.Deleted;
                context.SaveChanges();
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Something went wrong here... That's quite unusual, maybe try again."
                + "\n (Or, if you are PRO, open the console and send us the error log ;)";
                ViewBag.ErrorLog = ex.ToFullString();
                ViewBag.ErrorLog2 = ex.ToString();
            }

            return RedirectToAction("Index");
        }
		#endregion

        #region DISPOSE
        protected override void Dispose(bool disposing)
        {
            if (disposing) {
                context.Dispose();
            }
            base.Dispose(disposing);
        }
        #endregion
    }
}