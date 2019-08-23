using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace NetBet.Models
{
    public class BetResult
    {
        public int SeasonID { get; set; }
        public int EventID { get; set; }
        public String PlayerName { get; set; }
        public decimal WinLoseAmount { get; set; }
    }
}