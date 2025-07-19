using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using LT;

namespace WebGame.Controllers
{
    public class TourneyController : BaseController
    {
        Tourney LoadTourney(DBConnection db, int id)
        {
            var row = db.EvaluateRow("select * from tourney where id = {0}", id);
            var tourney = Tourney.Load(row);

            if (tourney != null)
            {
                foreach (var accountRow in db.EvaluateTable("select a.* from tourneyplayer t, account a where a.id = t.account_id and tourney_id = " + id))
                {
                    tourney.Players.Add(Account.Load(accountRow));
                }

                if (tourney.IsStarted)
                    tourney.LoadGames(db);
            }

            return tourney;
        }

        public ActionResult Index(int id)
        {
            //if (!Request.Url.PathAndQuery.EndsWith("/"))
            //{
            //    if (Request.IsLocal)
            //        throw new Exception("Invalid tourney link detected.  All links must end with a forward slash.");
            //    return RedirectPermanent("Tournament-" + id + "/");
            //}

            using (var db = new DBConnection())
            {
                var tourney = LoadTourney(db, id);
                if (tourney == null)
                    return HttpNotFound();

                if (LoggedIn && Account.IsAdmin)
                {
                    if (!tourney.IsStarted && IsSet("StartTourney"))
                    {
                        tourney.Start();
                        ViewBag.ErrorMessage = "Tournament Started";
                    }

                    if (IsSet("KillTourney"))
                    {
                        foreach (var game in tourney.Games)
                        {
                            if (game != null)
                                GameServer.KillGame(game.Id);
                        }

                        db.Execute("delete from tourneygame where tourney_id = {0}", tourney.Id);
                        db.Execute("delete from tourneyplayer where tourney_id = {0}", tourney.Id);
                        db.Execute("delete from tourney where id = {0}", tourney.Id);

                        return RedirectPermanent("/");
                    }
                }

                return View(tourney);
            }
        }

        //    if (Admin)
        //    {
        //        if (TourneyAdmin && IsSet("KillTourney"))
        //        {
        //            Execute("delete from tourneyplayer where tourney_id = " + TourneyID);
        //            Execute("delete from tourneygame where tourney_id = " + TourneyID);
        //            Execute("delete from tourney where id = " + TourneyID);
        //            Response.Redirect("Default");
        //            HttpContext.Current.Response.End();
        //        }

        //        if (IsSet("Drop"))
        //        {
        //            Execute("delete from tourneyplayer where tourney_id = " + TourneyID + " and account_id = " + GetInt("Drop"));
        //            ResultString += "Player Dropped";
        //            CurrentPlayerCount--;
        //        }
        //    }


        public ActionResult Create()
        {
            if (!LoggedIn || !Account.IsAdmin)
                return Redirect("/");

            return View(new Tourney());
        }

        [HttpPost]
        public ActionResult Create(Tourney model)
        {
            if (!LoggedIn || !Account.IsAdmin)
                return Redirect("/");

            if (ModelState.IsValid)
            {
                //if (model.IsTraining)
                //    model.MaxPlayers = 2;

                //GameServer.SaveNewGame(model);

                //model.Join(Account.Id, Account.Name, Account.Rating);
                //GameServer.PlayerJoined(model, Account.Id);

                //if (model.IsTraining)
                //    model.Join(1, "Computer", 0).Done = true;

                //return Redirect("/Game-" + model.Id + "/");

                ViewBag.ErrorMessage = Tourney.CreateTournament(model);
            }

            return View(model);
        }

        public ActionResult Join(int id)
        {
            if (!LoggedIn)
                return null;

            using (var db = new DBConnection())
            {
                var tourney = LoadTourney(db, id);

                if (tourney.CurrentPlayers >= tourney.MaxPlayers)
                    ViewBag.ErrorMessage = "This tournament is full.";
                else if (tourney.IsPlaying(Account.Id))
                    ViewBag.ErrorMessage = "You've already joined this tournament.";
                else if (tourney.IsStarted)
                    ViewBag.ErrorMessage = "This tournament has already started.";
                else
                {
                    db.Execute("insert into tourneyplayer (tourney_id, account_id) values ({0}, {1})", tourney.Id, Account.Id);
                    tourney.Players.Add(Account);
                    ViewBag.ErrorMessage = "You have joined this tournament.";
                }

                if (tourney.CurrentPlayers >= tourney.MaxPlayers && tourney.AutoStart)
                {
                    tourney.Start();
                    ViewBag.ErrorMessage = "Tournament Started";
                }

                return View("Index", tourney);
            }
        }

        public ActionResult Quit(int id)
        {
            if (!LoggedIn)
                return null;

            using (var db = new DBConnection())
            {
                var tourney = LoadTourney(db, id);

                if (!tourney.IsStarted && tourney.IsPlaying(Account.Id))
                {
                    ViewBag.ErrorMessage = "You quit this tournament.";
                    db.Execute("delete from tourneyplayer where tourney_id = {0} and account_id = {1}", tourney.Id, Account.Id);
                    tourney = LoadTourney(db, id);
                }

                return View("Index", tourney);
            }
        }
    }
}
