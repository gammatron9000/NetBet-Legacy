using System;
using System.Configuration;

namespace NetBet
{
    public static class Settings
    {
        public static String RavenUrl { get; private set; }
        public static String RavenDatabase { get; private set; }

        static Settings()
        {
            RavenUrl = ConfigurationManager.AppSettings["RavenUrl"] ?? "http://localhost:8080/";
            RavenDatabase = ConfigurationManager.AppSettings["RavenDatabase"] ?? "FantasyBet";
        }
    }
}