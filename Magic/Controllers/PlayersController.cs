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
    public class PlayersController : Controller
    {
        private MagicDbContext context = new MagicDbContext();

        [HttpGet]
        public ViewResult Index()
        {
            //return View(context.AllPlayers.Include(actionItem => actionItem.CardsInHand).ToList());
            return View();
        }

		#region CREATE
		[HttpGet]
        public ActionResult Create()
        {
            return View();
        } 

        [HttpPost]
        public ActionResult Create(Player actionItem)
        {
            if (ModelState.IsValid)
            {
                context.Entry(actionItem).State = EntityState.Added;
                context.SaveChanges();
                return RedirectToAction("Index");  
            }

            return View(actionItem);
        }
		#endregion

		#region EDIT/UPDATE
		[HttpGet]
        public ActionResult Edit(Player actionItem)
        {
            return View(actionItem);
        }

        [HttpPost]
        public ActionResult PostEdit(Player actionItem)
        {
            if (ModelState.IsValid)
            {
			try
                {
                    //Player item = context.AllPlayers.First(e => e.Id == actionItem.Id);
                    
					// TODO: UPDATE FIELDS ON DB ENTITY
					//context.Entry(item).State = EntityState.Modified;
                    //context.SaveChanges();
                }
                catch (InvalidOperationException)
                {
                    TempData["Message"] = "Your changes could not be saved... The item has probably been deleted in the meanwhile.";
                }
                return RedirectToAction("Index");
            }
            return View(actionItem);
        }
		#endregion

		#region DELETE
        public ActionResult Delete(Player actionItem)
        {
            context.Entry(actionItem).State = EntityState.Deleted;
            context.SaveChanges();
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