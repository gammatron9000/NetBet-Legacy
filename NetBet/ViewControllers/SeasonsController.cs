using NetBet.Models;
using System;
using System.Collections.Generic;
using System.Web.Mvc;
using System.Linq;

namespace NetBet.Controllers
{
    public class SeasonsController : Controller
    {
        public ActionResult Index(int id)
        {
            return View(id);
        }

        public ActionResult Create()
        {
            return View();
        }
        
    }
}