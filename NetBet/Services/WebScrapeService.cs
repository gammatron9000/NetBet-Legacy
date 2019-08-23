using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Text.RegularExpressions;

namespace NetBet.Services
{
    public static class WebScrapeService
    {
        
        public static void GetFightOdds()
        {
            var wc = new WebClient();
            var s = wc.DownloadString("http://www.bestfightodds.com/");
            Console.Write(s);
        }

    }
}