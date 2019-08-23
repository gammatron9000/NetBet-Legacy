using System;
using System.Collections.Generic;

namespace NetBet.Models
{
    public class Event
    {
        public int Id { get; set; }
        public int SeasonID { get; set; }
        public String EventName { get; set; }
        public List<BetLine> BetLines { get; set; }
        public DateTime StartTime { get; set; }
    }
}