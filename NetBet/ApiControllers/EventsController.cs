using System;
using System.Collections.Generic;
using System.Web.Http;
using NetBet.Models;
using System.Linq;
using NetBet.Services;

namespace NetBet.ApiControllers
{
    public class EventsController : ApiController
    {

        [Route("api/getCurrentEvents")]
        [HttpGet]
        public List<Event> GetCurrentSeasons(int seasonID)
        {
            var result = new List<Event>();
            using (var session = RavenDocStore.Store.OpenSession())
            {
                var season = session.Load<Season>(seasonID);
                var yesterday = DateTime.Now.AddDays(-1.0);

                result = session.Query<Event>()
                    .Where(x => x.SeasonID == seasonID)
                    .Where(x => x.StartTime <= season.EndTime && x.StartTime > yesterday)
                    .ToList();
            }
            return result;
        }

        [Route("api/GetFullEvent/{eventID}")]
        [HttpGet]
        public Event GetFullEvent(int eventID)
        {
            var result = new Event();
            using (var session = RavenDocStore.Store.OpenSession())
            {
                result = session.Load<Event>(eventID);
            }
            return result;
        }


        [Route("api/saveEvent")]
        [HttpPost]
        public String SaveEvent(Event evnt)
        {
            EventService.SaveEvent(evnt);
            
            return "Success";
        }

        [Route("api/resolveMatch")]
        [HttpPost]
        public String ResolveMatch(ResolveMatchDTO dto)
        {
            EventService.ResolveMatch(dto.EventID, dto.FighterName);
            return "Success";
        }

        [Route("api/resolveDraw")]
        [HttpPost]
        public String ResolveMatchDraw(ResolveMatchDTO dto)
        {
            EventService.ResolveMatchDraw(dto.EventID, dto.FighterName);
            return "Success";
        }

        [Route("api/deleteSingleLine")]
        [HttpPost]
        public Event DeleteSingleLine(DeleteSingleLineDTO dto)
        {
            return EventService.DeleteSingleLine(dto.EventID, dto.Line1, dto.Line2);
        }

        public class ResolveMatchDTO
        {
            public int EventID { get; set; }
            public String FighterName { get; set; }
        }

        public class DeleteSingleLineDTO
        {
            public int EventID { get; set; }
            public BetLine Line1 { get; set; }
            public BetLine Line2 { get; set; }
        }
           
    }
}
