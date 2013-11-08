using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Magic.Models;
using Magic.Models.DataContext;

namespace Magic.Controllers
{
    public class ChatLogController : Controller
    {
        private MagicDBContext db = new MagicDBContext();

        // GET: /Default1/
        public ActionResult Index()
        {
            return View(db.ChatLogs.ToList());
        }

        // GET: /Default1/Details/5
        public ActionResult Details(DateTime id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ChatLog chatlog = db.ChatLogs.Find(id);
            if (chatlog == null)
            {
                return HttpNotFound();
            }
            return View(chatlog);
        }

        // GET: /Default1/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: /Default1/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "DateCreated")] ChatLog chatlog)
        {
            if (ModelState.IsValid)
            {
                db.ChatLogs.Add(chatlog);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(chatlog);
        }

        // GET: /Default1/Edit/5
        public ActionResult Edit(DateTime id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ChatLog chatlog = db.ChatLogs.Find(id);
            if (chatlog == null)
            {
                return HttpNotFound();
            }
            return View(chatlog);
        }

        // POST: /Default1/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "DateCreated")] ChatLog chatlog)
        {
            if (ModelState.IsValid)
            {
                db.Entry(chatlog).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(chatlog);
        }

        // GET: /Default1/Delete/5
        public ActionResult Delete(DateTime id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ChatLog chatlog = db.ChatLogs.Find(id);
            if (chatlog == null)
            {
                return HttpNotFound();
            }
            return View(chatlog);
        }

        // POST: /Default1/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(DateTime id)
        {
            ChatLog chatlog = db.ChatLogs.Find(id);
            db.ChatLogs.Remove(chatlog);
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
    }
}
