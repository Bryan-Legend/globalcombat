using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using LT;

namespace WebGame
{
    // Note: For instructions on enabling IIS6 or IIS7 classic mode, 
    // visit http://go.microsoft.com/?LinkId=9394801

    public class MvcApplication : System.Web.HttpApplication
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
        }

        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute("DefaultHome", "", new { controller = "Home", action = "Index" });
            routes.MapRoute("Game-Manual", "Game-Manual", new { controller = "Home", action = "GameManual" });
            routes.MapRoute("IpAddresses", "IpAddresses", new { controller = "Home", action = "IpAddresses" });
            routes.MapRoute("Messages", "Messages", new { controller = "Home", action = "Messages" });
            routes.MapRoute("Send-Message", "Send-Message", new { controller = "Home", action = "SendMessage" });
            routes.MapRoute("Chat", "Chat", new { controller = "Home", action = "Chat" });
            routes.MapRoute("Opt-Out", "Opt-Out", new { controller = "Home", action = "OptOut" });

            routes.MapRoute("Create-Game", "Create-Game", new { controller = "Game", action = "Create" });
            routes.MapRoute("Game", "Game-{id}/{action}", new { controller = "Game", action = "Index" });

            routes.MapRoute("Create-Tournament", "Create-Tournament", new { controller = "Tourney", action = "Create" });
            routes.MapRoute("Tournament", "Tournament-{id}/{action}", new { controller = "Tourney", action = "Index" });

            routes.MapRoute("Player-Info", "Player-Info-{id}", new { controller = "Home", action = "PlayerInfo" });

            routes.MapRoute(
                "Default", // Route name
                "{controller}/{action}/{id}", // URL with parameters
                new { controller = "Home", action = "Index", id = UrlParameter.Optional } // Parameter defaults
            );
        }

        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();

            RegisterGlobalFilters(GlobalFilters.Filters);
            RegisterRoutes(RouteTable.Routes);
        }

        protected void Application_End()
        {
        }

        protected void Session_Start()
        {
            if (User.Identity.IsAuthenticated)
            {
                using (var db = new DBConnection())
                {
                    var accountRow = db.EvaluateRow("select * from account where name = '{0}' or email = '{0}'", DBConnection.AddSlashes(User.Identity.Name));
                    if (accountRow != null)
                        AccountController.SetSession(accountRow);
                }
            }
        }

        protected void Session_End()
        {
            var account = Session["Account"] as Account;
            if (account != null)
            {
                account = GameServer.GetOnlineAccount(account.Id);
                if (account != null)
                {
                    lock (GameServer.OnlineAccounts)
                    {
                        GameServer.OnlineAccounts.Remove(account);
                    }
                }
            }
        }
    }
}