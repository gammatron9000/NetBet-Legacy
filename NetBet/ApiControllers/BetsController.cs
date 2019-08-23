using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using NetBet.Models;
using NetBet.Services;

namespace NetBet.ApiControllers
{
    public class BetsController : ApiController
    {
        [HttpPost]
        [Route("api/placeBet")]
        public String PlaceBet(BetDTO bet)
        {
            try
            {
                BetService.PlaceBet(bet);
                return "Success";
            }
            catch (Exception ex)
            {
                return "ERROR: " + ex.Message;
            }
        }
        
        [HttpGet]
        [Route("api/getBetsForEvent/{eventID}")]
        public List<Bet> GetBetsForEvent(int eventID)
        {
            return BetService.GetBetsForEvent(eventID);
        }

        [HttpPost]
        [Route("api/getUsersBetsForEvent/")]
        public List<Bet> GetUsersBetsForEvent(UserBetsForEventDTO dto)
        {
            return BetService.GetUsersBetsForEvent(dto.EventID, dto.PlayerName);
        }

        [HttpPost]
        [Route("api/cancelExistingBet/")]
        public List<Bet> CancelExistingBet(CancelExistingBetDTO dto)
        {
            return BetService.CancelExistingBet(dto.EventID, dto.Bet, dto.Username);
        }
    }

    public class UserBetsForEventDTO
    {
        public int EventID { get; set; }
        public String PlayerName { get; set; }
    }

    public class CancelExistingBetDTO
    {
        public int EventID { get; set; }
        public Bet Bet { get; set; }
        public string Username { get; set; }
    }

}
