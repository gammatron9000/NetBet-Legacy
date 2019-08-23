using System;
using System.Collections.Generic;
using System.Web.Mvc;

namespace NetBet.Controllers
{
    public class EventsController : Controller
    {
        public ActionResult Create(int id)
        {
            // the event and its id dont exist yet.
            // this ID is the season ID the event will be associated with
            return View(id);
        }

        public ActionResult Edit(int id)
        {
            // id is the eventID to look up
            return View(id);
        }

        public ActionResult Bet(int id)
        {
            // id is eventID
            return View(id);
        }

        public ActionResult Live(int id)
        {
            return View(id);
        }

    }
}