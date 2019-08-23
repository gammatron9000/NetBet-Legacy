using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using NetBet.Services;

namespace NetBet.ApiControllers
{
    public class WebScrapeController : ApiController
    {

        [HttpGet]
        [Route("api/getLatestOdds")]
        public String GetLatestOdds()
        {
            WebScrapeService.GetFightOdds();
            return "";
        }

    }
}
