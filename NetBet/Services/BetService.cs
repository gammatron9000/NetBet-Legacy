using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using NetBet.Models;

namespace NetBet.Services
{
    public static class BetService
    {
        /// <summary>
        /// returns all users' bets for a specific event
        /// </summary>
        public static List<Bet> GetBetsForEvent(int eventID)
        {
            var bets = new List<Bet>();
            using (var session = RavenDocStore.Store.OpenSession())
            {
                bets = session.Query<Bet>()
                    .Where(x => x.EventID == eventID)
                    .ToList();
            }
            return bets;
        }


        public static List<Bet> GetUsersBetsForEvent(int eventID, String username)
        {
            var bets = new List<Bet>();
            using (var session = RavenDocStore.Store.OpenSession())
            {
                bets = session.Query<Bet>()
                    .Customize(x => x.WaitForNonStaleResultsAsOfLastWrite())
                    .Take(1000)
                    .Where(x => x.EventID == eventID)
                    .Where(x => x.PlayerName == username)
                    .ToList();
            }
            return bets;
        }


        public static List<Bet> GetAllBetsForSeason(int seasonID)
        {
            var bets = new List<Bet>();
            using (var session = RavenDocStore.Store.OpenSession())
            {
                var betCount = session.Query<Bet>()
                    .Customize(x => x.WaitForNonStaleResultsAsOfLastWrite())
                    .Where(x => x.SeasonID == seasonID)
                    .Count();

                // TODO : make this paginate and return all results
                //if (betCount > 1000)
                //{ }
                //else
                //{
                    bets = session.Query<Bet>()
                        .Where(x => x.SeasonID == seasonID)
                        .Take(1000)
                        .ToList();
                //} 
            }
            return bets;
        }


        public static void PlaceBet(BetDTO dto)
        {
            var fullEvent = EventService.GetFullEvent(dto.EventID);
            if (dto.isParlay)
            {
                // get odds
                var fighterNames = dto.Wagers.Select(x => x.FighterName).ToList();
                var allOdds = fullEvent.BetLines
                    .Where(x => fighterNames.Contains(x.FighterName))
                    .Select(x => x.Odds)
                    .ToList();
                var parlayOdds = CalculateParlayOdds(allOdds);
                var individualBets = dto.Wagers
                    .Select(x => { return new SingleBet
                    {
                        FighterName = x.FighterName,
                        Stake = x.Stake,
                        Result = Result.TBD,
                        Odds = 0
                    }; }).ToList();
                var bet = new Bet
                {
                    EventID = dto.EventID,
                    SeasonID = dto.SeasonID,
                    TimePlaced = DateTime.Now,
                    PlayerName = dto.PlayerName,
                    Result = Result.TBD,
                    Stake = dto.ParlayStake,
                    Odds = parlayOdds,
                    IndividualBets = individualBets
                };
                var bets = new List<Bet> { bet };
                PlaceBets(bets);
            }
            else // bet is not parlay, it is many separate single bets
            {
                var bets = new List<Bet>();
                foreach (var wager in dto.Wagers)
                {
                    var line = fullEvent.BetLines
                        .FirstOrDefault(x => x.FighterName == wager.FighterName);
                    if (line == null) { throw new Exception("error getting odds for " + wager.FighterName); }
                    var odds = line.Odds;
                    bets.Add(new Bet {
                        EventID = dto.EventID,
                        SeasonID = dto.SeasonID,
                        PlayerName = dto.PlayerName,
                        TimePlaced = DateTime.Now,
                        Stake = wager.Stake,
                        Result = Result.TBD,
                        Odds = odds,
                        IndividualBets = new List<SingleBet>
                        {
                            new SingleBet
                            {
                                FighterName = wager.FighterName,
                                Odds = odds,
                                Stake = wager.Stake,
                                Result = Result.TBD
                            }
                        }
                    });
                }
                PlaceBets(bets);
            }
        }

        private static void PlaceBets(List<Bet> bets)
        {
            var totalStake = bets.Sum(x => x.Stake);
            // error if total is more than person currently has
            if (SeasonService.PlayerHasEnoughMoney(bets[0].SeasonID, bets[0].PlayerName, totalStake))
            {
                using (var session = RavenDocStore.Store.OpenSession())
                {
                    foreach (var bet in bets)
                    {
                        session.Store(bet);
                    }
                    session.SaveChanges();
                }
            }
            else throw new Exception("You do not have enough cash");
        }
        
        /// <summary>
        /// Multiplies all odds together for a parlay bet
        /// </summary>
        private static decimal CalculateParlayOdds(List<decimal> odds)
        {
            decimal value = 1.0M;
            foreach (var odd in odds)
            {
                value *= odd;
            }
            return Math.Round(value, 2);
        }
        
        
        public static void ResolveAllBets(int eventID, String winnerName, String loserName)
        {
            using (var session = RavenDocStore.Store.OpenSession())
            {
                var eventBets = session.Query<Bet>()
                    .Where(x => x.EventID == eventID)
                    .ToList();
                var winBets = eventBets
                    .Where(x => x.IndividualBets.Select(b => b.FighterName).Contains(winnerName))
                    .ToList();
                var loseBets = eventBets
                    .Where(x => x.IndividualBets.Select(b => b.FighterName).Contains(loserName))
                    .ToList();

                foreach (var win in winBets)
                {
                    var bet = win.IndividualBets.FirstOrDefault(x => x.FighterName == winnerName);
                    bet.Result = Result.Win;
                }
                foreach(var lose in loseBets)
                {
                    var bet = lose.IndividualBets.FirstOrDefault(x => x.FighterName == loserName);
                    bet.Result = Result.Lose;
                }
                session.SaveChanges();   
            }

            BetService.ResolveAllFullBets(eventID);
        }

        public static void ResolveAllBetsAsDraw(int eventID, String fighter1, String fighter2)
        {
            using (var session = RavenDocStore.Store.OpenSession())
            {
                var eventBets = session.Query<Bet>()
                    .Where(x => x.EventID == eventID)
                    .ToList();
                var bets = eventBets
                    .Where(x => x.IndividualBets.Select(b => b.FighterName).Contains(fighter1)
                             || x.IndividualBets.Select(b => b.FighterName).Contains(fighter2))
                    .ToList();
                foreach (var bet in bets)
                {
                    var indv1 = bet.IndividualBets.FirstOrDefault(x => x.FighterName == fighter1);
                    var indv2 = bet.IndividualBets.FirstOrDefault(x => x.FighterName == fighter2);
                    if (indv1 != null) { indv1.Result = Result.Lose; }
                    if (indv2 != null) { indv2.Result = Result.Lose; }
                }
                session.SaveChanges();
            }
            BetService.ResolveAllFullBets(eventID);
        }

        private static void ResolveAllFullBets(int eventID)
        {
            var wins = new List<BetResult>();
            using (var session = RavenDocStore.Store.OpenSession())
            {
                var eventBets = session.Query<Bet>()
                    .Customize(x => x.WaitForNonStaleResultsAsOfLastWrite())
                    .Where(x => x.EventID == eventID)
                    .Where(x => x.Result == Result.TBD)
                    .ToList();
                
                var winners = eventBets
                    .Where(x => x.IndividualBets.All(b => b.Result == Result.Win))
                    .ToList();

                var losers = eventBets
                    .Where(x => x.IndividualBets.All(b => b.Result != Result.TBD))
                    .Where(x => x.IndividualBets.Any(b => b.Result == Result.Lose))
                    .ToList();

                foreach (var winner in winners)
                {
                    winner.Result = Result.Win;
                    wins.Add(new BetResult
                    {
                        EventID = winner.EventID,
                        SeasonID = winner.SeasonID,
                        PlayerName = winner.PlayerName,
                        WinLoseAmount = winner.Odds * winner.Stake
                    });
                }

                foreach (var loser in losers)
                {
                    loser.Result = Result.Lose;
                }

                session.SaveChanges();
            }
        }

        public static void IssueRefund(int eventID, string playerName, decimal amount)
        {
            var fullEvent = new Event();
            using (var session = RavenDocStore.Store.OpenSession())
            {
                fullEvent = EventService.GetFullEvent(eventID);
            }

            var singleBet = new SingleBet()
            {
                Stake = amount,
                Odds = 2.0M,
                FighterName = "REFUND",
                Result = Result.Win,
                MatchNumber = 999
            };
            var singleBets = new List<SingleBet>();
            singleBets.Add(singleBet);

            var refund = new Bet()
            {
                EventID = eventID,
                SeasonID = fullEvent.SeasonID,
                PlayerName = playerName,
                TimePlaced = DateTime.Now,
                Stake = amount,
                Odds = 2.0M,
                Result = Result.Win,
                IndividualBets = singleBets
            };

            using (var session = RavenDocStore.Store.OpenSession())
            {
                session.Store(refund);
                session.SaveChanges();
            }
        }


        public static List<Bet> CancelExistingBet(int eventID, Bet bet, string username)
        {
            using (var session = RavenDocStore.Store.OpenSession())
            {
                var dbBet = session.Query<Bet>()
                    .Where(x => x.EventID == eventID)
                    .Where(x => x.Id == bet.Id)
                    .FirstOrDefault();
                var allResults = dbBet.IndividualBets.Select(x => x.Result);
                
                if (dbBet != null &&
                    dbBet.PlayerName == username &&
                    dbBet.Result == Result.TBD &&
                    allResults.All(x => x == Result.TBD))
                {
                    session.Delete<Bet>(dbBet);
                    session.SaveChanges();
                }
            }

            return GetUsersBetsForEvent(eventID, username);
        }

    }
}