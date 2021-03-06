﻿using System.Web.Mvc;

namespace FlightJournal.Web.Controllers
{
    [Authorize]
    public class AboutController : Controller
    {
        [AllowAnonymous]
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Administration()
        {
            return View();
        }
        
        public ActionResult License()
        {
            return View();
        }

        [AllowAnonymous]
        public ActionResult UHB530()
        {
            return View();
        }

        [AllowAnonymous]
        public PartialViewResult GoogleAnalyticsPartial()
        {
            return this.PartialView();
        }

    }
}
