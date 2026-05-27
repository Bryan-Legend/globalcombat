using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc;

using System.Collections;
using GlobalCombat.Core;
using Microsoft.AspNetCore.Http;

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
