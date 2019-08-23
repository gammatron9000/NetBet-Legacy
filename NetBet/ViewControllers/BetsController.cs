using System;
using System.Collections.Generic;
using System.Web.Mvc;

namespace NetBet.ViewControllers
{
    public class BetsController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }
    }
}