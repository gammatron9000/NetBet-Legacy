using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace NetBet.Models
{
    public class BetLine
    {
        public String FighterName { get; set; }
        public decimal Odds { get; set; }
        public int MatchNumber { get; set; }
        public Result Result { get; set; }
    }
}