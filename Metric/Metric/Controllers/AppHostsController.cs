using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Metric.DAL;
using Metric.Models;

namespace Metric.Controllers
{
    public class AppHostsController : Controller
    {
        private LogMessageContext db = new LogMessageContext();

        // GET: AppHosts
        public ActionResult Index()
        {
            return View(db.AppHosts.ToList());
        }

        // GET: AppHosts/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            db.AppHosts.Include("LogMessage");
            AppHosts appHosts = db.AppHosts.Find(id);

            if (appHosts == null)
            {
                return HttpNotFound();
            }
            return View(appHosts);
        }

        // GET: AppHosts/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: AppHosts/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "AppHostsID,Name")] AppHosts appHosts)
        {
            if (ModelState.IsValid)
            {
                db.AppHosts.Add(appHosts);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(appHosts);
        }

        // GET: AppHosts/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            AppHosts appHosts = db.AppHosts.Find(id);
            if (appHosts == null)
            {
                return HttpNotFound();
            }
            return View(appHosts);
        }

        // POST: AppHosts/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "AppHostsID,Name")] AppHosts appHosts)
        {
            if (ModelState.IsValid)
            {
                db.Entry(appHosts).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(appHosts);
        }

        // GET: AppHosts/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            AppHosts appHosts = db.AppHosts.Find(id);
            if (appHosts == null)
            {
                return HttpNotFound();
            }
            return View(appHosts);
        }

        // POST: AppHosts/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            AppHosts appHosts = db.AppHosts.Find(id);
            db.AppHosts.Remove(appHosts);
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
