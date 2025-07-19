using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Web.Mvc;
using System.Web.Security;
using System.Collections;
using GlobalCombat.Core;
using System.Web;

namespace WebGame
{
    public class HomeIndexModel
    {
        public List<Game> NewGames;
        public long MessageCount;
        public List<Game> PlayerGames;
        public List<Game> InvitedGames;
        public HtmlString TourneyList;
        public string YourTourneyList;
    }
}
