﻿using System.Linq;
using System.Web.Mvc;
using DevDay.Models;

namespace DevDay.Controllers
{
    public class InscricoesController : Controller
    {
        private readonly DevdayEntities _db = new DevdayEntities();

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Stats()
        {
            ViewBag.UsersCount = _db.Users.Count();
            ViewBag.UsersCompetitorCount = _db.Users.Count(t => t.IsCompetitor);
            ViewBag.SubmissionsCount = _db.Submissions.Count();
            ViewBag.ActiveUsersCount = _db.Users.Count(t => t.LastLoggedOn != null);
                
            return View();
        }

        protected override void Dispose(bool disposing)
        {
            _db.Dispose();
            base.Dispose(disposing);
        }
    }
}
