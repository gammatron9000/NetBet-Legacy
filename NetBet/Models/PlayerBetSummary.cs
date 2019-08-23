using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace NetBet.Models
{
    /// <summary>
    ///  DTO Object for the eventBreakdown summary on the season main page
    /// </summary>
    public class PlayerBetSummary
    {
        public int EventID { get; set; }
        public String PlayerName { get; set; }
        public int TotalBets { get; set; }
        public int BetsWon { get; set; }
        public decimal CashResult { get; set; }
    }
}