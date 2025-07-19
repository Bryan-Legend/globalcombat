using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Web.Mvc;
using System.Web.Security;
using System.Collections;
using GlobalCombat.Core;

namespace WebGame
{
    public class PlayerInfoModel
    {
        public Account Account;
        public List<Hashtable> IpAddresses;
        public List<Game> Games;
    }
}
