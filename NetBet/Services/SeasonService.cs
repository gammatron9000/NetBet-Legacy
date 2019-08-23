using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using NetBet.Models;
using NetBet.Services;

namespace NetBet.Services
{
    public static class SeasonService
    {
        public static Season GetSeason(int seasonID)
        {
            Season season = null;
            using (var session = RavenDocStore.Store.OpenSession())
            {
                season = session.Load<Season>(seasonID);
            }
            return season;
        }
       

        public static bool PlayerHasEnoughMoney(int seasonID, String playerName, decimal stake)
        {
            var currentStandings = SeasonService.GetPlayerStandingsForSeason(seasonID);
            var playerSummary = currentStandings.FirstOrDefault(x => x.PlayerName == playerName);
            var season = GetSeason(seasonID);
            var total = season.StartingCash; // default total is season starting amount
            if (playerSummary != null) { total = playerSummary.CashResult; } // if there are bets, calculate how much player has left
            if (total - stake > (0 - season.MinimumCash))
            { return true; }
            else return false;
        }
        
        public static List<PlayerBetSummary> GetPlayerBetSummariesForEvents(int seasonID)
        {
            var summaries = new List<PlayerBetSummary>();
            // get all bets for the given season
            var seasonBets = BetService.GetAllBetsForSeason(seasonID);

            // group bets by eventid
            var eventBets = seasonBets.GroupBy(x => x.EventID);

            foreach (var evt in eventBets)
            {
                // group by playername
                var groupedBets = evt.GroupBy(x => x.PlayerName);
                // assemble summaries
                foreach (var group in groupedBets)
                {
                    var summary = new PlayerBetSummary();
                    var wins = group.Where(x => x.Result == Result.Win);
                    var notWins = group.Where(x => x.Result != Result.Win);
                    var currentLoss = notWins.Sum(x => x.Stake);
                    var currentProfit = wins.Sum(x => (x.Stake * x.Odds) - x.Stake);
                    summary.PlayerName = group.Key;
                    summary.TotalBets = group.Count();
                    summary.BetsWon = wins.Count();
                    summary.CashResult = currentProfit - currentLoss; 
                    summary.EventID = evt.Key;
                    summaries.Add(summary);
                }
            }

            return summaries;
        }

        public static List<PlayerBetSummary> GetPlayerStandingsForSeason(int seasonID)
        {
            var season = SeasonService.GetSeason(seasonID);
            var summaries = new List<PlayerBetSummary>();
            if (season == null) { return summaries; }
            var seasonBets = BetService.GetAllBetsForSeason(seasonID);
            var playerGroups = seasonBets.GroupBy(x => x.PlayerName);
            foreach (var group in playerGroups)
            {
                var summary = new PlayerBetSummary();
                var wins = group.Where(x => x.Result == Result.Win);
                var notWins = group.Where(x => x.Result != Result.Win);
                var currentLoss = notWins.Sum(x => x.Stake);
                var currentProfit = wins.Sum(x => (x.Stake * x.Odds) - x.Stake);
                summary.PlayerName = group.Key;
                summary.TotalBets = group.Count();
                summary.BetsWon = wins.Count();
                summary.CashResult = GetCurrentCash(season.StartingCash, season.MinimumCash, currentProfit, currentLoss);
                summaries.Add(summary);
            }
            return summaries;
        }

        public static PlayerBetSummary GetCurrentPlayerStats(int seasonID, string playerName)
        {
            var summary = new PlayerBetSummary();
            var season = SeasonService.GetSeason(seasonID);
            var myBets = BetService.GetAllBetsForSeason(seasonID)
                .Where(x => x.PlayerName == playerName);
            var wins = myBets.Where(x => x.Result == Result.Win);
            var notWins = myBets.Where(x => x.Result != Result.Win);
            var currentLoss = notWins.Sum(x => x.Stake);
            var currentProfit = wins.Sum(x => (x.Stake * x.Odds) - x.Stake);
            summary.PlayerName = playerName;
            summary.TotalBets = myBets.Count();
            summary.BetsWon = wins.Count();
            summary.CashResult = GetCurrentCash(season.StartingCash, season.MinimumCash, currentProfit, currentLoss);
            return summary;
        }

        private static decimal GetCurrentCash(decimal startingCash, decimal minimumCash, decimal profit, decimal loss)
        {
            var result = startingCash + (profit - loss);
            return result;
        }

        public static Bet GetBiggestWin(int seasonID)
        {
            var result = new Bet();
            using (var session = RavenDocStore.Store.OpenSession())
            {
                result = session.Query<Bet>()
                    .Where(x => x.SeasonID == seasonID)
                    .Where(x => x.Result == Result.Win)
                    .OrderByDescending(x => x.Stake * x.Odds)
                    .FirstOrDefault();
            }
            return result;
        }

        public static Bet GetBiggestLoss(int seasonID)
        {
            var result = new Bet();
            using (var session = RavenDocStore.Store.OpenSession())
            {
                result = session.Query<Bet>()
                    .Where(x => x.SeasonID == seasonID)
                    .Where(x => x.Result == Result.Lose)
                    .OrderByDescending(x => x.Stake)
                    .FirstOrDefault();
            }
            return result;
        }

        public static void ResetSeason(int seasonID)
        {
            using (var session = RavenDocStore.Store.OpenSession())
            {
                var season = session.Load<Season>(seasonID);
                if (season == null) { throw new Exception("season not found"); }
                var events = session.Query<Event>()
                    .Where(x => x.SeasonID == seasonID)
                    .ToList();

                // set all events' betlines back to 'tbd' result
                foreach (var evt in events)
                {
                    foreach (var line in evt.BetLines)
                    {
                        line.Result = Result.TBD;
                    }
                }

                // delete all player bets
                var seasonBets = session.Query<Bet>()
                    .Where(x => x.SeasonID == seasonID)
                    .ToList();
                foreach (var bet in seasonBets)
                {
                    session.Delete(bet);
                }
                
                session.SaveChanges();
            }
        }


    }
}