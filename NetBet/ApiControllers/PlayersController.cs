using NetBet.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace NetBet.ApiControllers
{
    public class PlayersController : ApiController
    {
        [Route("api/players")]
        [HttpGet]
        public List<Player> GetAllPlayers()
        {
            var result = new List<Player>();
            using (var session = RavenDocStore.Store.OpenSession())
            {
                result = session.Query<Player>().ToList();
            }
            return result;
        }

        [Route("api/players/{id}")]
        [HttpDelete]
        public List<Player> DeletePlayer(int id)
        {
            var result = new List<Player>();
            using (var session = RavenDocStore.Store.OpenSession())
            {
                var entity = session.Load<Player>(id);
                session.Delete(entity);
                session.SaveChanges();

                result = session.Query<Player>()
                    .Customize(x => x.WaitForNonStaleResultsAsOfLastWrite())
                    .ToList();
            }
            return result;
        }

        [Route("api/createPlayer")]
        [HttpPost]
        public List<Player> CreatePlayers(Player player)
        {
            var result = new List<Player>();
            using (var session = RavenDocStore.Store.OpenSession())
            {
                session.Store(player);
                session.SaveChanges();

                result = session.Query<Player>()
                    .Customize(x => x.WaitForNonStaleResultsAsOfLastWrite())
                    .ToList();
            }

            return result;
        }

        [Route("api/players/{id}")]
        [HttpGet]
        public Player GetPlayer(int id)
        {
            var result = new Player();
            using (var session = RavenDocStore.Store.OpenSession())
            {
                result = session.Load<Player>(id);
            }
            return result;
        }

        [HttpPost]
        [Route("api/players/getPlayerColors")]
        public List<PlayerAndColor> GetPlayerColors (List<String> playerNames)
        {
            var result = new List<PlayerAndColor>();
            foreach (var player in playerNames)
            {
                using (var session = RavenDocStore.Store.OpenSession())
                {
                    var yes = session.Query<Player>()
                        .FirstOrDefault(x => x.PlayerName == player);
                    if (yes != null)
                    {
                        result.Add(new PlayerAndColor
                        {
                            PlayerName = player,
                            Color = yes.PlayerColor
                        });
                    }
                }
            }
            return result;
        }
        
        public class PlayerAndColor
        {
            public String PlayerName { get; set; }
            public String Color { get; set; }
        } 

    }
}
