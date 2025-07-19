using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Text;
using GlobalCombat.Core;
using LT;

namespace WebGame
{
    public class GameController : BaseController
    {
        Game game;
        Player player;

        bool IsPlaying { get { return player != null; } }
        bool IsHost { get { return (LoggedIn && Account.IsAdmin) || (IsPlaying && player.Number == 1); } }

        void Initalize(int id)
        {
            game = GameServer.GetGame(id);

            if (game == null)
            {
                Response.RedirectPermanent("/", true);
                Response.End();
                return;
            }

            if (LoggedIn)
            {
                player = game.GetPlayer(Account.Id);
                ViewBag.Player = player;
            }

            ViewBag.IsHost = IsHost;
        }

        void LoadMessages()
        {
            if (game == null)
                return;

            using (var db = new DBConnection())
            {
                ViewBag.Messages = new List<Message>();

                using (var messages = db.OpenQuery(String.Format("select m.id as id, to_id, from_id, fromAccount.name fromName, text, time from message as m join account as fromAccount on m.from_id = fromAccount.id where to_id = {0} order by id desc limit 150", -game.Id)))
                {
                    while (messages.Read())
                    {
                        var newMessage = new Message() { Id = (int)messages["id"], SourceId = (int)messages["from_id"], SourceName = (string)messages["fromName"], Text = messages["text"].ToString(), Sent = Utility.FromUnixTimestamp((int)messages["time"]) };
                        ViewBag.Messages.Add(newMessage);
                    }
                }
            }
        }

        public ActionResult Index(int id = -1)
        {
            if (id == -1)
                return HttpNotFound();

            if (!Request.Url.PathAndQuery.EndsWith("/"))
            {
                if (Request.IsLocal)
                    throw new Exception("Invalid game link detected.  All links to games must end with a forward slash.");
                return RedirectPermanent("Game-" + id + "/");
            }

            Initalize(id);

            if (game.ForceEndCheck())
            {
                game.ForceEnd();
                Initalize(id);
            }

            LoadMessages();

            return View(game);
        }

        public ActionResult Create()
        {
            if (!LoggedIn)
                return Redirect("/");

            return View(game);
        }

        [HttpPost]
        public ActionResult Create(Game model)
        {
            if (ModelState.IsValid)
            {
                if (model.IsTraining)
                    model.MaxPlayers = 2;

                GameServer.SaveNewGame(model);

                model.Join(Account.Id, Account.Name, Account.Rating);
                GameServer.PlayerJoined(model, Account.Id);

                if (model.IsTraining)
                    model.Join(1, "Computer", 0).Done = true;

                return Redirect("/Game-" + model.Id + "/");
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }


        public ActionResult Join(int id)
        {
            Initalize(id);

            LoadMessages();

            if (!LoggedIn)
            {
                ViewBag.ErrorMessage = "You must be logged in to join the game.";
                return View("Index", game);
            }

            if (game.TourneyId != 0)
            {
                ViewBag.ErrorMessage = "This game is part of a tournament and can not be directly joined.";
                return View("Index", game);
            }

            if (game.GetPlayer(Account.Id) != null)
            {
                ViewBag.ErrorMessage = "You are already part of the game.";
                return View("Index", game);
            }

            if (game.Started)
            {
                ViewBag.ErrorMessage = "Game already started.";
                return View("Index", game);
            }

            if (game.IsPrivate)
            {
                var invite = (from i in game.Invites where i.AccountId == Account.Id select i).FirstOrDefault();
                if (invite == null)
                {
                    ViewBag.ErrorMessage = "You must be invited to a private game.";
                    return View("Index", game);
                }
            }

            //if (Options.GetInt("MINR") > (int)Account["rating"])
            //    ResultString = "Your rating (" + Account["rating"] + ") must be at least " + Options.GetInt("MINR").ToString() + " to join this game.";

            //if (Options.GetInt("MING") > (int)Account["games"])
            //    ResultString = "Your game count (" + Account["games"] + ") must be at least " + Options.GetInt("MING").ToString() + " to join this game.";

            //if (Options.GetInt("P") != 0 && (GetInt("PassKey") != Options.GetInt("P")) && GetInt("ThisIsBryan") != 1701)
            //    ResultString = "Incorrect Pass Key Entered.  " + Request["PassKey"];

            //// check to make sure it's not full
            //if ((int)LGame["cur_players"] >= (int)LGame["max_players"])
            //    return "Unable to join game. Game already full.";

            game.Join(Account.Id, Account.Name, Account.Rating);
            GameServer.PlayerJoined(game, Account.Id);

            Initalize(id);

            LoadMessages();

            return View("Index", game);
        }

        public ActionResult Invite(int id, string inviteEmail)
        {
            Initalize(id);

            if (!String.IsNullOrEmpty(inviteEmail))
            {
                //find online players
                //if (inviteEmail == "online")
                //{
                //    inviteEmail = null;
                //    lock (GameServer.OnlineAccounts)
                //    {
                //        foreach (var onlineAccount in GameServer.OnlineAccounts)
                //        {
                //            inviteEmail += onlineAccount.Name + ',';
                //        }
                //    }
                //}

                var invites = inviteEmail.Split(',', '\n');
                foreach (var invite in invites)
                {
                    var trimmedInvite = invite.Trim();
                    if (!String.IsNullOrEmpty(trimmedInvite))
                    {
                        ////find random players
                        //if (trimmedInvite == "random")
                        //{
                        //    Random rand = new Random();
                        //    int randID = rand.Next(1, 90000);
                        //    using (var db = new DBConnection())
                        //    {
                        //        var randAccountName = db.Evaluate("select name from account where id = {0}", randID) as String;
                        //        trimmedInvite = randAccountName;
                        //    }
                        //}

                        using (var db = new DBConnection())
                        {
                            var account = FindAccount(trimmedInvite);
                            bool accountCreated = false;
                            if (account == null)
                            {
                                int accountId;
                                AddErrorMessage(CreateAccount(trimmedInvite, game.Id, out accountId));
                                accountCreated = true;
                                account = Account.Load(db.EvaluateRow("select * from account where id = {0}", accountId));
                            }

                            if (account != null)
                            {
                                // Check for existing invite
                                if ((from i in game.Invites where i.AccountId == account.Id select i).Count() > 0)
                                {
                                    AddErrorMessage(account.Name + " has already been invited to this game.");
                                    continue;
                                }

                                if ((from p in game.Players where p.AccountId == account.Id select p).Count() > 0)
                                {
                                    AddErrorMessage(account.Name + " is already playing this game.");
                                    continue;
                                }

                                game.Invites.Add(new Invite() { AccountId = account.Id, Name = account.Name });
                                game.SendForumMessage(String.Format("{0} invited {1} to this game.", Account.Name, account.Name));
                                GameServer.PlayerInvited(game, account);

                                if (!accountCreated)
                                {
                                    GameServer.SendMessage(db, account.Id, Account.Id, Account.Name, String.Format(
@"You've been challenged to a game of Global Combat by {0}.

Visit http://{1}/Game-{2}/ to view the details and join the game.
", Account.Name, Request.Url.Host, game.Id));

                                    //                                GameServer.SendEmail(account.EmailAddress, account.Name, "You've been challenged to a game.", String.Format(
                                    //@"You've been challenged to a game of Global Combat by {0}.
                                    //
                                    //Visit http://{1}/Game-{2}/ to view the details and join the game.
                                    //", Account.Name, Request.Url.Host, game.Id));
                                }
                            }
                        }
                    }
                }
            }

            LoadMessages();

            return View("Index", game);
        }

        void AddErrorMessage(string errorMessage)
        {
            if (ViewBag.ErrorMessage != null)
            {
                ViewBag.ErrorMessage += "<br />" + errorMessage;
            }
            else
                ViewBag.ErrorMessage = errorMessage;
        }

        public ActionResult Error(string message)
        {
            ViewBag.ErrorMessage = message;
            return View("Index", game);
        }

        public ActionResult Quit(int id)
        {
            Initalize(id);

            if (!LoggedIn || !IsPlaying)
                return Redirect("/Game-" + game.Id + "/");

            if (game.Started)
                game.EliminatePlayer(player);
            else
            {
                if (game.TourneyId > 0)
                {
                    ViewBag.ErrorMessage = "Unable to quit a tournament game.";
                }
                else
                {
                    game.Unjoin(player);
                    GameServer.PlayerUnjoined(game, Account.Id);

                    if (game.Players.Count <= 0)
                        return Redirect("/");
                }
            }

            Initalize(id);

            LoadMessages();

            return View("Index", game);
        }

        public ActionResult Kick(int id, int playerNumber)
        {
            Initalize(id);

            if (!LoggedIn)
                return Redirect("/Game-" + game.Id + "/");

            if (!game.Started && IsHost && game.TourneyId == 0)
            {
                var kickPlayer = game.GetPlayerByNumber(playerNumber);
                if (kickPlayer != null)
                {
                    game.Unjoin(kickPlayer);
                    GameServer.PlayerUnjoined(game, kickPlayer.AccountId);

                    if (game.Players.Count <= 0)
                        return Redirect("/");
                }
            }

            Initalize(id);

            LoadMessages();

            return View("Index", game);
        }

        public ActionResult Start(int id)
        {
            Initalize(id);

            if (!LoggedIn)
                return Redirect("/Game-" + game.Id + "/");

            if (!game.Started && IsHost && game.TourneyId == 0 && game.CurrentPlayers >= 2)
            {
                game.MaxPlayers = game.CurrentPlayers;
                game.Start();
            }

            Initalize(id);

            LoadMessages();

            return View("Index", game);
        }

        [ValidateInput(false)]
        public string Send(int id, string message)
        {
            if (String.IsNullOrWhiteSpace(message))
                return null;

            Initalize(id);

            message = HttpUtility.HtmlEncode(message);
            game.SendForumMessage(message, Account.Id, Account.Name);

            return null;
        }

        public ActionResult Done(int id)
        {
            Initalize(id);

            if (IsPlaying)
                game.Done(player);

            return null;
        }

        public ActionResult ForceTurn(int id)
        {
            Initalize(id);

            if (IsPlaying)// && !player.IsEliminated)
                game.ForceTurn(player);

            return null;
        }

        public int Assign(int id, int areaId = 0, int amount = 0)
        {
            Initalize(id);

            if (!IsPlaying)
                return 0;

            var area = game.GetArea(areaId);
            if (area == null || area.Owner != player)
                return 0;

            var result = game.SetAssigned(area, amount);
            if (result != 0)
                GameServer.SaveGame(game);
            return result;
        }

        public int Unassign(int id, int areaId = 0)
        {
            Initalize(id);

            if (!IsPlaying)
                return 0;

            var area = game.GetArea(areaId);
            if (area == null || area.Owner != player)
                return 0;

            var result = game.ClearAssigned(area);
            if (result != 0)
                GameServer.SaveGame(game);
            return result;
        }

        public int Transfer(int id, int areaId = 0, int targetAreaId = 0, int amount = 0)
        {
            Initalize(id);

            if (!IsPlaying)
                return 0;

            var area = game.GetArea(areaId);
            var targetArea = game.GetArea(targetAreaId);
            if (area == null || area.Owner != player || targetArea == null || targetArea.Owner != player)
                return 0;

            var result = game.SetTransfer(area, targetArea, amount);
            GameServer.SaveGame(game);
            return result;
        }

        public int Attack(int id, int areaId = 0, int targetAreaId  = 0, int amount = 0)
        {
            Initalize(id);

            if (!IsPlaying)
                return 0;

            var area = game.GetArea(areaId);
            var targetArea = game.GetArea(targetAreaId);
            if (area == null || area.Owner != player || targetArea == null || targetArea.Owner == player)
                return 0;

            var result = game.SetAttack(area, targetArea, amount);
            GameServer.SaveGame(game);
            return result;
        }
    }
}