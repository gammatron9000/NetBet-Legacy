using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace NetBet.Models
{
    public class EventStats
    {
        public List<FighterStat> FighterStats { get; set; }

        public EventStats()
        { FighterStats = new List<FighterStat>(); }
    }

    public class FighterStat
    {
        public string FighterName { get; set; }
        public int WinsAsFavorite { get; set; }
        public int LossesAsFavorite { get; set; }
        public int WinsAsUnderdog { get; set; }
        public int LossesAsUnderdog { get; set; }
        public decimal TotalOdds { get; set; }
    }
}