using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using LT;
using System.Collections;
using GlobalCombat.Core;
using System.Text;

namespace WebGame
{
    public class HomeController : BaseController
    {
        public ActionResult Index()
        {
            var model = new HomeIndexModel();

            using (var db = new DBConnection())
            {
                model.NewGames = GameServer.GetNewGames();

                var tourneyList = new StringBuilder();
                foreach (var row in db.EvaluateTable("select * from tourney where status='New' order by id"))
                {
                    tourneyList.AppendFormat("<a href=\"/Tournament-{0}/\" >{1}</a> - {2}<br />", row["id"], row["name"], row["status"]);
                }
                model.TourneyList = new HtmlString(tourneyList.ToString());

                if (LoggedIn)
                {
                    // refresh account record
                    Account = Account.Load(db.EvaluateRow("select * from account where id = {0}", Account.Id));

                    //model.MessageCount = (long)db.Evaluate("select Count(*) from message where to_id = " + Account.Id);
                    model.PlayerGames = GameServer.GetPlayerGames(Account.Id);
                    model.InvitedGames = GameServer.GetPlayerGames(Account.Id, false, true);

                    // Your Tourneys
                    var yourTourneyList = new StringBuilder();
                    var tourneys = db.EvaluateTable("select * from tourneyplayer, tourney where account_id = {0} and id = tourney_id order by tourney_id desc limit 20", Account.Id);
                    if (tourneys.Count > 0)
                    {
                        yourTourneyList.Append("<h3>Your Recent Tourneys</h3>");
                        foreach (var tourney in tourneys)
                            yourTourneyList.AppendFormat("<a href=\"/Tournament-{0}/\">{1}</a> - {2}<br>", tourney["tourney_id"], tourney["name"], tourney["status"]);
                        yourTourneyList.Append("<br />");
                    }
                    model.YourTourneyList = yourTourneyList.ToString();
                }
            }

            return View(model);
        }

        public ActionResult GameManual()
        {
            return View();
        }

        public ActionResult IpAddresses(string ipAddress = null)
        {
            var model = new PlayerInfoModel();
            if (ipAddress != null)
            {
                using (var db = new DBConnection())
                {
                    model.IpAddresses = db.EvaluateTable("select * from account_login where ipaddress = '{0}' order by datetime desc limit 100", DBConnection.AddSlashes(ipAddress));
                }
            }
            return View(model);
        }

        public ActionResult PlayerInfo(int id)
        {
            if (id <= 0)
                return HttpNotFound();

            var model = new PlayerInfoModel();

            using (var db = new DBConnection())
            {
                model.Account = Account.Load(db.EvaluateRow("select * from account where id = {0}", id));

                if (model.Account == null)
                    return HttpNotFound();

                if (LoggedIn && Account.Id == id) // use freshest account record
                    Account = model.Account;

                if (IsSet("ShowLoginHistory"))
                    model.IpAddresses = db.EvaluateTable("select * from account_login where account_id = " + id + " order by datetime desc limit 100");

                if (IsSet("KillAccount") && Account.IsAdmin)
                {
                    db.Execute("update account set disabled_by = " + Account.Id + " where id=" + id);
                    ViewBag.ErrorMessage = "Account Disabled";
                }

                if (id > 1)
                    model.Games = GameServer.GetPlayerGames(id, IsSet("AllGames"));
                else
                    model.Games = new List<Game>();
            }

            return View("PlayerInfo", model);
        }

        [ValidateInput(false)]
        public ActionResult SendMessage(int accountId = -1, string message = null)
        {
            if (!LoggedIn || accountId == -1)
                return Redirect("/");

            using (var db = new DBConnection())
            {
                message = HttpUtility.HtmlEncode(message);
                ViewBag.ErrorMessage = GameServer.SendMessage(db, accountId, Account.Id, Account.Name, message);
            }

            return PlayerInfo(accountId);
        }

        [ValidateInput(false)]
        public string Chat(int targetId, string message)
        {
            if (!LoggedIn)
                return null;

            if (String.IsNullOrWhiteSpace(message))
                return null;

            using (var db = new DBConnection())
            {
                message = HttpUtility.HtmlEncode(message);
                ViewBag.ErrorMessage = GameServer.SendMessage(db, targetId, Account.Id, Account.Name, message);
            }

            return null;
        }

        public ActionResult LoadChatMessages(int targetId, string targetName)
        {
            if (!LoggedIn)
                return null;

            using (var db = new DBConnection())
            {
                var result = new ArrayList();

                foreach (var message in db.EvaluateTable("select to_id, from_id, text from message where (to_id = {0} and from_id = {1}) or (to_id = {1} and from_id = {0}) order by id desc limit 20", targetId, Account.Id))
                {
                    result.Add(new { name = (int)message["from_id"] == Account.Id ? "Me" : targetName, text = (string)message["text"] });
                }

                result.Reverse();

                AddChatWindow(targetId, targetName);

                return Json(result);
            }
        }

        public void CloseChatWindow(int targetId, string targetName)
        {
            if (!LoggedIn)
                return;

            var windowId = String.Format("{0}|{1}", targetId, targetName);
            OpenChatWindows.Remove(windowId);
        }

        public ActionResult Messages()
        {
            if (!LoggedIn)
                return Redirect("/");

            using (var db = new DBConnection())
            {
                //if (IsSet("DeleteMessage"))
                //    db.Execute("delete from message where id=" + GetInt("DeleteMessage") + " and to_id=" + Account.Id);
                //else if (IsSet("DeleteAll"))
                //    db.Execute("delete from message where to_id=" + Account.Id);

                return View(LoadMessages(db, Account.Id));
            }
        }

        List<Message> LoadMessages(DBConnection db, int toId)
        {
            var result = new List<Message>();

            using (var messages = db.OpenQuery(String.Format("select m.id as id, to_id, from_id, fromAccount.name fromName, toAccount.name toName, text, time from message as m join account as fromAccount on m.from_id = fromAccount.id join account as toAccount on m.to_id = toAccount.id where to_id = {0} or from_id = {0} order by id desc limit 150", toId)))
            {
                var count = 0;
                while (messages.Read())
                {
                    var destinationId = (int)messages["to_id"];
                    if (destinationId > 0)
                    {
                        var newMessage = new Message() { Id = (int)messages["id"], SourceId = (int)messages["from_id"], SourceName = (string)messages["fromName"], DestinationId = destinationId, DestinationName = (string)messages["toName"], Text = messages["text"].ToString(), Sent = Utility.FromUnixTimestamp((int)messages["time"]) };
                        result.Add(newMessage);

                        count++;
                        if (count >= 100)
                            break;
                    }
                }
            }
            return result;
        }

        public ActionResult Stats()
        {
            if (!LoggedIn || !Account.IsAdmin)
                return Redirect("/");


            using (var games_db = GameServer.CreateDB())
            {
                ViewBag.GameCount = games_db.Evaluate("select count(*) from game where status = 1");

                if (IsSet("ForceAll"))
                {
                    foreach (var gameId in games_db.EvaluateTable("select id from game where status = 1"))
                    {
                        var game = GameServer.GetGame((int)gameId["id"]);

                        if (game.ForceEndCheck())
                        {
                            game.ForceEnd();
                            GameServer.SaveGame(game);
                        }
                    }
                }
            }

            using (var db = new DBConnection())
            {
                ViewBag.TourneyCount = db.Evaluate("select count(*) from tourney where status = 'Running'");

                ViewBag.DailyActive = db.Evaluate("select count(*) from account where last_on > {0}", Utility.UnixTimestamp(DateTime.UtcNow.AddDays(-1)));
                ViewBag.MonthlyActive = db.Evaluate("select count(*) from account where last_on > {0}", Utility.UnixTimestamp(DateTime.UtcNow.AddMonths(-1)));

                ViewBag.Accounts = db.Evaluate("select count(*) from account");
                ViewBag.MonthAccounts = db.Evaluate("select count(*) from account where signed_up > {0}", Utility.UnixTimestamp(DateTime.UtcNow.AddMonths(-1)));

                // new accounts:
                // SELECT Date(FROM_UNIXTIME(signed_up)) Day, Count(*) AccountCount from Account group by Date(FROM_UNIXTIME(signed_up)) order by Day desc

                var loginCounts = new StringBuilder();
                foreach (var row in db.EvaluateTable("SELECT Date(FROM_UNIXTIME(datetime)) Day, Count(*) LoginCount from Account_login where dateTime > {0} group by Date(FROM_UNIXTIME(datetime)) order by Day desc", Utility.UnixTimestamp(DateTime.UtcNow.AddMonths(-1))))
                {
                    loginCounts.AppendFormat("['{0}', {1}],", ((DateTime)row["Day"]).ToShortDateString(), row["LoginCount"]);
                }
                ViewBag.LoginCounts = loginCounts.ToString();


                var newAccounts = new StringBuilder();
                foreach (var row in db.EvaluateTable("SELECT Date(FROM_UNIXTIME(signed_up)) Day, Count(*) AccountCount from Account where signed_up > {0} group by Date(FROM_UNIXTIME(signed_up)) order by Day desc", Utility.UnixTimestamp(DateTime.UtcNow.AddMonths(-1))))
                {
                    newAccounts.AppendFormat("['{0}', {1}],", ((DateTime)row["Day"]).ToShortDateString(), row["AccountCount"]);
                }
                ViewBag.NewAccounts = newAccounts.ToString();
            }

            return View();
        }

        public ActionResult OptOut()
        {
            if (!IsSet("Account") || !IsSet("Key"))
            {
                ViewBag.ErrorMessage = "Missing account or opt out key.";
                return View();
            }

            return View();
        }

        [HttpPost]
        public ActionResult OptOut(int account)
        {
            if (!IsSet("Account") || !IsSet("Key"))
            {
                ViewBag.ErrorMessage = "Missing account or opt out key.";
                return View();
            }

            using (var db = new DBConnection())
            {
                var accountRow = db.EvaluateRow("select * from Account where Id = {0}", account);
                if (accountRow == null)
                {
                    ViewBag.ErrorMessage = "Invalid account.";
                    return View();
                }
                else
                {
                    if ((int)accountRow["OptOutKey"] != GetInt("Key"))
                    {
                        ViewBag.ErrorMessage = "Incorrect opt out key.";
                        return View();
                    }
                }

                db.Execute("update Account set OptOut = 1 where Id = {0}", account);
                ViewBag.ErrorMessage = "You will no longer recieve emails about new features.";
            }

            return View();
        }
    }
}