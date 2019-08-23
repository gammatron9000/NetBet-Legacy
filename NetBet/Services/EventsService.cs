using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using NetBet.Models;

namespace NetBet.Services
{
    public static class EventService
    {

        public static void SaveEvent(Event evnt)
        {
            using (var session = RavenDocStore.Store.OpenSession())
            {
                // save/create event
                var existing = session.Load<Event>(evnt.Id);
                if (existing == null)
                {
                    session.Store(evnt);
                }
                else
                {
                    existing.EventName = evnt.EventName;
                    existing.StartTime = evnt.StartTime;
                    existing.BetLines = evnt.BetLines;
                }
                
                session.SaveChanges();
            }
        }

        
        public static void DeleteEvent(int eventID)
        {
            using (var session = RavenDocStore.Store.OpenSession())
            {
                var evnt = session.Load<Event>(eventID);
                session.Delete<Event>(evnt);
                session.SaveChanges();
            }
        }

        public static Event GetFullEvent(int eventID)
        {
            Event result = null;
            using (var session = RavenDocStore.Store.OpenSession())
            {
                result = session.Load<Event>(eventID);
            }
            return result;
        }


        public static Event ResolveMatch(int eventID, String winnerName)
        {
            var loser = new BetLine();
            using (var session = RavenDocStore.Store.OpenSession())
            {
                var fullEvent = session.Load<Event>(eventID);
                if (fullEvent == null) { throw new Exception("event not found. id: " + eventID); }

                var winner = fullEvent.BetLines.FirstOrDefault(x => x.FighterName == winnerName);
                if (winner == null) { throw new Exception("bet line not found for event: " + eventID + ", winner: " + winnerName); }
                loser = fullEvent.BetLines
                    .Where(x => x.MatchNumber == winner.MatchNumber)
                    .FirstOrDefault(x => x.FighterName != winnerName);
                if (loser == null) { throw new Exception("loser not found for " + winnerName + ". Match Number: " + winner.MatchNumber); }
                winner.Result = Result.Win;
                loser.Result = Result.Lose;
                session.SaveChanges();
            }
            EventService.CheckForZeroOutRefunds(eventID);
            BetService.ResolveAllBets(eventID, winnerName, loser.FighterName);
            return EventService.GetFullEvent(eventID);
        }

        public static Event ResolveMatchDraw(int eventID, string oneOfTheFighters)
        {
            var otherFighter = new BetLine();
            using (var session = RavenDocStore.Store.OpenSession())
            {
                var fullEvent = session.Load<Event>(eventID);
                if (fullEvent == null) { throw new Exception("event not found. id: " + eventID); }

                var fighter = fullEvent.BetLines.FirstOrDefault(x => x.FighterName == oneOfTheFighters);
                if (fighter == null) { throw new Exception("bet line not found for event: " + eventID + ", name: " + oneOfTheFighters); }
                otherFighter = fullEvent.BetLines
                    .Where(x => x.MatchNumber == fighter.MatchNumber)
                    .FirstOrDefault(x => x.FighterName != oneOfTheFighters);
                if (otherFighter == null) { throw new Exception("other fighter not found for " + oneOfTheFighters + ". Match Number: " + fighter.MatchNumber); }
                fighter.Result = Result.Lose;
                otherFighter.Result = Result.Lose;
                session.SaveChanges();
            }
            EventService.CheckForZeroOutRefunds(eventID);
            BetService.ResolveAllBetsAsDraw(eventID, oneOfTheFighters, otherFighter.FighterName);
            return EventService.GetFullEvent(eventID);
        }


        public static void CheckForZeroOutRefunds(int eventID)
        {
            var fullEvent = new Event();
            using (var session = RavenDocStore.Store.OpenSession())
            {
                fullEvent = session.Load<Event>(eventID);
            }
            if (fullEvent == null) { throw new Exception("event not found for id: " + eventID); }
            // only zero-out if all the bets on this event have been resolved
            if (fullEvent.BetLines.Any(x => x.Result == Result.TBD)) { return; }

            var playerBets = BetService.GetBetsForEvent(eventID).GroupBy(x => x.PlayerName);
            foreach (var player in playerBets)
            {
                var stats = SeasonService.GetCurrentPlayerStats(fullEvent.SeasonID, player.Key);
                if (stats.CashResult < 0)
                {
                    var amount = 0 - stats.CashResult;
                    BetService.IssueRefund(eventID, player.Key, amount);
                }
            }
        }

        public static Event DeleteSingleLine(int eventID, BetLine line1, BetLine line2)
        {
            if (line1.MatchNumber != line2.MatchNumber) { return null; }
            
            using (var session = RavenDocStore.Store.OpenSession())
            {
                var myEvent = session.Query<Event>()
                    .Where(x => x.Id == eventID)
                    .FirstOrDefault();
                if (myEvent == null)
                { return null; }

                // update the event to remove that specific line
                myEvent.BetLines.RemoveAll(x => x.MatchNumber == line1.MatchNumber);

                // delete all bets associated with the cancelled line
                var betsToDelete = session.Query<Bet>()
                    .Where(x => x.EventID == eventID)
                    .Where(x => x.IndividualBets.Any(b => b.MatchNumber == line1.MatchNumber))
                    .ToArray();

                foreach (var bet in betsToDelete)
                { session.Delete<Bet>(bet); }

                session.SaveChanges();
            }

            return GetFullEvent(eventID);
        }



    }
}