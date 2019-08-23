using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using NetBet.Models;
using Raven.Client;

namespace NetBet.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            var seasons = new List<Season>();
            return View(seasons);
        }
    }
}
