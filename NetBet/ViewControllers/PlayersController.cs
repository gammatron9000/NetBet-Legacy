using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace NetBet.ViewControllers
{
    public class PlayersController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }
    }
}