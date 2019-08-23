using System;
using System.Collections.Generic;

namespace NetBet.Models
{
    public class Bet
    {
        public int Id { get; set; }
        public String PlayerName { get; set; }
        public int EventID { get; set; }
        public int SeasonID { get; set; }
        public DateTime TimePlaced { get; set; }
        public decimal Odds { get; set; }
        public decimal Stake { get; set; }
        public Result Result { get; set; }
        public List<SingleBet> IndividualBets { get; set; }
    }

    public class SingleBet
    {
        public String FighterName { get; set; }
        public decimal Odds { get; set; }
        public decimal Stake { get; set; }
        public Result Result { get; set; }
        public int MatchNumber { get; set; } // for sorting bets on the live screen
    }

    
}