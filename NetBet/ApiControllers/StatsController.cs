using NetBet.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace NetBet.ApiControllers
{
    public class StatsController : ApiController
    {

        [Route("api/stats/{eventID}")]
        public EventStats GetEventStats(int eventID)
        {
            var result = new EventStats();
            var evnt = new Event();
            var allLines = new List<BetLine>();
            using (var session = RavenDocStore.Store.OpenSession())
            { evnt = session.Load<Event>(eventID); }
            using (var session = RavenDocStore.Store.OpenSession())
            {
                // raven doesnt support selectmany >:[
                allLines = session.Query<Event>()
                    .Take(1000)
                    .ToList()
                    .SelectMany(x => x.BetLines)
                    .ToList();
            }

            foreach (var line in evnt.BetLines)
            {
                var myStats = new FighterStat();
                var myLines = allLines
                    .Where(x => x.FighterName.ToLower().Replace(".", "").Replace(" ", "") == 
                                line.FighterName.ToLower().Replace(".", "").Replace(" ", ""))
                    .ToList();
                var favorites = myLines.Where(x => x.Odds < 2.00M);
                var underdogs = myLines.Where(x => x.Odds >= 2.00M);
                var favoriteWins = favorites.Where(x => x.Result == Result.Win);
                var favoriteLosses = favorites.Where(x => x.Result == Result.Lose);
                var underdogWins = underdogs.Where(x => x.Result == Result.Win);
                var underdogLosses = underdogs.Where(x => x.Result == Result.Lose);
                myStats.FighterName = line.FighterName;
                myStats.WinsAsFavorite = favoriteWins.Count();
                myStats.LossesAsFavorite = favoriteLosses.Count();
                myStats.WinsAsUnderdog = underdogWins.Count();
                myStats.LossesAsUnderdog = underdogLosses.Count();

                var allWins = favoriteWins.Concat(underdogWins);
                var allLosses = favoriteLosses.Concat(underdogLosses);
                var winPayouts = allWins.Sum(x => x.Odds) - allWins.Count();
                var totalLoss = (decimal)allLosses.Count();
                myStats.TotalOdds = winPayouts - totalLoss;

                result.FighterStats.Add(myStats);
            }

            return result;
        }


    }
}
