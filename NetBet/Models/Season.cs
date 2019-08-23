using System;
using System.Collections.Generic;

namespace NetBet.Models
{
    public class Season
    {
        public int Id { get; set; }
        public String SeasonName { get; set; }
        public String Description { get; set; }
        public decimal StartingCash { get; set; }
        public decimal MinimumCash { get; set; }
        public DateTime EndTime { get; set; }
        public List<Player> Players { get; set; }
        public List<Event> Events { get; set; }

        public Season()
        {
            Players = new List<Player>();
            Events = new List<Event>();
        }
    }
}