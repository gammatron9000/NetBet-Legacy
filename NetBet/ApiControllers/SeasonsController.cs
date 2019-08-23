using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using NetBet.Models;
using NetBet.Services;

namespace NetBet.ApiControllers
{
    public class SeasonsController : ApiController
    {

        [Route("api/getCurrentSeasons")]
        [HttpGet]
        public List<Season> GetCurrentSeasons()
        {
            var seasons = new List<Season>();
            using (var session = RavenDocStore.Store.OpenSession())
            {
                seasons = session.Query<Season>()
                    .Customize(x => x.Include<Season, Event>(s => s.Id))
                    .OrderByDescending(x => x.EndTime)
                    //.Where(x => x.EndTime >= DateTime.Now)
                    .ToList();

                foreach (var season in seasons)
                {
                    season.Events = session.Query<Event>()
                        .Where(x => x.SeasonID == season.Id)
                        .ToList();
                }
            }
            return seasons;
        }


        [Route("api/getSeasonDetails/{id}")]
        [HttpGet]
        public Season GetSeasonDetails(int id)
        {
            var season = new Season();

            using (var session = RavenDocStore.Store.OpenSession())
            {
                season = session
                    .Include<Season>(x => x.Events)
                    .Load<Season>(id);

                season.Events = session.Query<Event>()
                    .Where(x => x.SeasonID == season.Id)
                    .ToList();
            }

            return season;
        }


        [Route("api/createSeason")]
        [HttpPost]
        public String CreateSeason(Season season)
        {
            // save
            using (var session = RavenDocStore.Store.OpenSession())
            {
                session.Store(season);
                session.SaveChanges();
            }
            return "success";
        }

        [HttpGet]
        [Route("api/GetPlayersForSeason/{seasonID}")]
        public List<String> GetPlayersForSeason(int seasonID)
        {
            var players = new List<String>();
            using (var session = RavenDocStore.Store.OpenSession())
            {
                var season = session.Load<Season>(seasonID);
                if (season != null)
                {
                    players = season.Players
                        .Select(x => x.PlayerName)
                        .ToList();
                }
            }
            return players;
        }


        [HttpGet]
        [Route("api/GetPlayerBetSummariesForEvents/{seasonID}")]
        public List<PlayerBetSummary> GetPlayerBetSummariesForEvents(int seasonID)
        {
            return SeasonService.GetPlayerBetSummariesForEvents(seasonID);
        }

        [HttpGet]
        [Route("api/getPlayerStandingsForSeason/{seasonID}")]
        public List<PlayerBetSummary> GetPlayerStandingsForSeason(int seasonID)
        {
            return SeasonService.GetPlayerStandingsForSeason(seasonID);
        }

        [HttpPost]
        [Route("api/getCurrentPlayerStats")]
        public PlayerBetSummary GetCurrentPlayerStats(PlayerNameAndSeason dto)
        {
            return SeasonService.GetCurrentPlayerStats(dto.SeasonID, dto.PlayerName);
        }


        [HttpGet]
        [Route("api/ResetSeason/{seasonID}")]
        public String ResetSeason(int seasonID)
        {
            SeasonService.ResetSeason(seasonID);
            return "Success";
        }



        public class PlayerNameAndSeason
        {
            public int SeasonID { get; set; }
            public string PlayerName { get; set; }
        }
    }
}
