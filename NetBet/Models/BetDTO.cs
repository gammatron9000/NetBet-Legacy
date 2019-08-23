using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace NetBet.Models
{
    public class BetDTO
    {
        public String PlayerName { get; set; }
        public int EventID { get; set; }
        public int SeasonID { get; set; }
        public bool isParlay { get; set; }
        public decimal ParlayStake { get; set; }
        public List<SimpleWager> Wagers { get; set; }

        public BetDTO()
        {
            Wagers = new List<SimpleWager>();
        }
    }

    public class SimpleWager
    {
        public String FighterName { get; set; }
        public decimal Stake { get; set; }
    }
}