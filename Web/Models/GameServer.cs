using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ProtoBuf;
using System.IO;
using GlobalCombat.Core;
using System.Web.Caching;
using LT;
using System.Net.Mail;
using System.Configuration;
using System.Web.Mvc;
using System.Text;

namespace WebGame
{
    [ProtoContract]
    public static class GameServer
    {
        static SmtpClient smtpClient;
        public static SmtpClient SmtpClient
        {
            get
            {
                if (smtpClient == null)
                    smtpClient = new SmtpClient(ConfigurationManager.AppSettings["MailServer"]);
                return smtpClient;
            }
        }

        public static string FromAddress
        {
            get { return ConfigurationManager.AppSettings["FromAddress"]; }
        }

        public static string ContactEmail
        {
            get { return ConfigurationManager.AppSettings["ContactEmail"]; }
        }

        public static List<Account> OnlineAccounts = new List<Account>();

        static GameServer()
        {
            Game.OnMessage = (game, text, sourceId, sourceName) =>
                {
                    using (var db = new DBConnection())
                    {
                        SendMessage(db, -game.Id, sourceId, sourceName, text);
                    }

                    var message = new Message() { Sent = DateTime.UtcNow, Text = text, SourceId = sourceId, SourceName = sourceName };
                    GameHub.Say("Game-" + game.Id, message.Print(false));
                };

            Game.OnStart = (game) =>
                {
                    SaveGame(game);
                    using (var db = CreateDB())
                    {
                        db.Execute("delete from player where game_id = {0} and isInvite = 1", game.Id);
                    }
                    EmailAllPlayers(game, game.GameName + " Started", "The Global Combat game (" + game.GameName + ") has started.\n\nVisit https://globalcombat.com/Game-" + game.Id + " to play your turn.", true);
                    GameHub.Refresh("Game-" + game.Id);
                };

            Game.OnDone = (game, source) =>
                {
                    SaveGame(game);
                    GameHub.SetDone("Game-" + game.Id, source.Number);
                };

            Game.OnRunTurn = (game, message) =>
                {
                    // flag a deadbeat players as done
                    var deadbeatLimit = DateTime.UtcNow - TimeSpan.FromMinutes(Math.Max(game.TurnLength * 2, 2880));
                    if (game.PreviousTurnTime != DateTime.MinValue)
                    {
                        using (var db = new DBConnection())
                        {
                            foreach (var player in game.Players)
                            {
                                if (!player.IsEliminated && GetLastLogin(db, player.AccountId) < deadbeatLimit)
                                    player.Done = true;
                            }
                        }
                    }

                    SaveGame(game);

                    foreach (var player in game.Players)
                    {
                        var account = GetOnlineAccount(player.AccountId);
                        if (account != null)
                            GameHub.SendNotification(account.SessionKey, game.GameName + " Turn Run", "", "/Game-" + game.Id + "/");
                    }

                    EmailAllPlayers
                    (
                        game,
                        game.GameName + " Turn " + game.Turn,
                        String.Format("The turn ran for Global Combat game ({0}).\n\n{1}\n\nVisit http://{2}/Game-{3}/ to play your turn.", game.GameName, BasePage.StripHtml(message), HttpContext.Current.Request.Url.Host, game.Id)
                    );

                    GameHub.Refresh("Game-" + game.Id);
                };

            Game.OnEliminated = (game, loser) =>
            {
                if (!game.IsTraining)
                {
                    using (var db = new DBConnection())
                    {
                        if (game.TourneyId > 0)
                            Tourney.PlayerFinishedCheck(db, game, loser);

                        SaveGame(game);

                        db.Execute("update account set games = games + 1 where id = " + loser.AccountId);
                    }
                }
                else
                    SaveGame(game);
            };

            Game.OnEnd = (game, winner) =>
            {
                if (!game.IsTraining)
                {
                    using (var db = new DBConnection())
                    {
                        if (game.TourneyId > 0)
                        {
                            Tourney.PlayerFinishedCheck(db, game, winner);
                            Tourney.TourneyFinishedCheck(db, game);
                        }

                        SaveGame(game);

                        if (winner != null)
                        {
                            db.Execute("update account set wins = wins + 1, games = games + 1 where id = " + winner.AccountId);
                        }

                        // award points
                        foreach (var player in game.Players)
                            db.Execute("update account set rating = rating + " + player.RatingChange + " where id=" + player.AccountId);

                        var results = new StringBuilder();
                        foreach (var player in game.Players)
                        {
                            results.AppendFormat("\n{4} {0} (Score Expected = {1}, Score = {2}, Rating Change = {3})</td>", player.GetPlace(), player.ScoreExpected, player.Score, player.RatingChange, player.Name);
                        }
                        EmailAllPlayers(game, game.GameName + " Ended", String.Format("The Global Combat game ({0}) has ended.\n\n{1}\n\nVisit http://{2}/Game-{3}/ to view the results.", game.GameName, BasePage.StripHtml(results.ToString()), HttpContext.Current.Request.Url.Host, game.Id));
                    }
                }
                else
                    SaveGame(game);

                GameHub.Refresh("Game-" + game.Id);
            };
        }

        static DateTime GetLastLogin(DBConnection db, int accountId)
        {
            if (GetOnlineAccount(accountId) != null)
                return DateTime.UtcNow;

            var result = db.Evaluate("select datetime from account_login where account_id = {0} order by datetime desc limit 1", accountId);
            if (result is Int32)
                return Utility.FromUnixTimestamp((Int32)result);
            else
                return DateTime.MinValue;
        }

        static bool CanSend(DBConnection db, int sourceId)
        {
            var time = Utility.UnixTimestamp(DateTime.UtcNow) - 3600;
            return Convert.ToInt32(db.Evaluate("select Count(distinct to_id) from message where from_id = " + sourceId + " and time > " + time)) <= 10;
        }

        public static string SendMessage(DBConnection db, int destinationId, int sourceId, string sourceName, string text)
        {
            //if (sourceId > 1 && !CanSend(db, sourceId))
            //    return "Unable to send message due to spam guard.  Everyone is only allowed to message ten different places within a single hour.";

            db.Execute
            (
                "insert into message (to_id, from_id, time, text) values ({0}, {1}, {2}, '{3}')",
                destinationId,
                sourceId,
                Utility.UnixTimestamp(DateTime.UtcNow),
                DBConnection.AddSlashes(text)
            );

            if (destinationId > 0)
            {
                var account = GameServer.GetOnlineAccount(destinationId);
                if (account != null)
                {
                    GameHub.SendMessage(account.SessionKey, sourceId, sourceName, text);
                }
                else
                {
                    // send email?
                    var destAccount = db.EvaluateRow("select name, email, forward_emails from account where id = " + destinationId);
                    if ((string)destAccount["forward_emails"] == "All")
                        GameServer.SendEmail((string)destAccount["email"], (string)destAccount["name"], "Message from " + sourceName, sourceName + " wrote:\n" + text);
                }
            }

            return "Message Sent";
        }

        public static void EmailAllPlayers(Game game, string subject, string message, bool isGameStart = false)
        {
            using (var db = new DBConnection())
            {
                foreach (var player in game.Players)
                {
                    if (GetOnlineAccount(player.AccountId) == null)
                    {
                        var accountRow = db.EvaluateRow("select name, email, forward_emails from account where id = {0}", player.AccountId);
                        if (accountRow != null)
                        {
                            string forwardEmails = (string)accountRow["forward_emails"];
                            if (isGameStart && forwardEmails == "GameStarts")
                                SendEmail((string)accountRow["email"], (string)accountRow["name"], subject, message);
                            else
                            {
                                if (forwardEmails == "All" || forwardEmails == "AllGame")
                                    SendEmail((string)accountRow["email"], (string)accountRow["name"], subject, message);
                            }
                        }
                    }
                }
            }
        }

        public static void SendEmail(string toAccountEmail, string toAccountName, string subject, string message)
        {
            try
            {
                SmtpClient.Send
                (
                    FromAddress,
                    toAccountEmail + " (" + toAccountName + ")",
                    String.Format("[{0}] {1}", LT.BasePage.SiteName, subject),
                    message + "\n\n\nThese emails will only be sent when you are not logged in.\n\nTo change what kind of messages you recieve login to global combat and go to your account config page."
                );
            }
            catch
            {
                // throw away email sending failures.
                //if (Request.IsLocal)
                //    throw;
            }
        }

        public static Account GetOnlineAccount(int accountId)
        {
            lock (OnlineAccounts)
            {
                return (from a in OnlineAccounts where a != null && a.Id == accountId select a).FirstOrDefault();
            }
        }

        public static Game GetGame(int id)
        {
            var result = HttpContext.Current.Cache[id.ToString()] as Game;

            if (result == null)
            {
                // load from server
                using (var db = CreateDB())
                {
                    var bytes = db.Evaluate("select Serialized from game where id = {0}", id) as byte[];
                    if (bytes != null)
                    {
                        result = Game.Load(bytes);

                        // repair my botched import of tourney game names.
                        if (result.TourneyId != 0 && result.GameName.StartsWith("Game"))
                        {
                            using (var mainDb = new DBConnection())
                            {
                                var row = mainDb.EvaluateRow("select * from tourneygame where game_id = {0}", result.Id);
                                if (row != null)
                                {
                                    var tourneyName = mainDb.Evaluate("select name from tourney where id = {0}", row["tourney_id"]) as string;
                                    result.GameName = String.Format("{0} - Game {1}", tourneyName, row["game_num"]);
                                    result.Save();
                                }
                            }
                        }

                        HttpContext.Current.Cache[result.Id.ToString()] = result;
                    }
                }
            }

            return result;
        }

        public static DBConnection CreateDB()
        {
            return new DBConnection(DBConnection.GetConnectionString("Global Combat Games"));
        }

        public static void SaveNewGame(Game game)
        {
            using (var db = CreateDB())
            {
                db.ExecuteWithParams("insert into game (Status, Serialized, Private) values (@Status, @Serialized, @Private) ", new { Status = game.Status, Serialized = game.Save(), @Private = (game.TourneyId != 0 || game.IsPrivate ? 1 : 0) });
                game.Id = (int)db.LastInsertID;
                if (String.IsNullOrEmpty(game.GameName))
                    game.GameName = "Game #" + game.Id;
            }

            // save the new id
            SaveGame(game);

            // insert into cache
            HttpContext.Current.Cache[game.Id.ToString()] = game;
        }

        public static void SaveGame(Game game)
        {
            using (var db = CreateDB())
            {
                db.ExecuteWithParams("update game set Status = @Status, Serialized = @Serialized, Private = @Private where id = @Id", new { Id = game.Id, Status = game.Status, Serialized = game.Save(), @Private = (game.TourneyId != 0 || game.IsPrivate ? 1 : 0) });
            }
        }

        public static List<Game> GetNewGames()
        {
            var result = new List<Game>();
            using (var db = CreateDB())
            {
                foreach (var row in db.EvaluateTable("select Id from game where status = 0 and private = 0 order by id desc"))
                {
                    result.Add(GetGame((int)row["Id"]));
                }
            }
            return result;
        }

        public static List<Game> GetPlayerGames(int accountId, bool allGames = false, bool invites = false)
        {
            if (invites)
                allGames = false;

            var result = new List<Game>();
            using (var db = CreateDB())
            {
                string query;
                if (!allGames)
                    query = String.Format("select g.id game_id from player p, game g where p.isInvite = {1} and p.game_id = g.id and p.account_id = {0} and g.status < 2 order by id desc", accountId, invites ? 1 : 0);
                else
                    query = String.Format("select game_id from player where isInvite = 0 and account_id = {0} order by game_id desc limit 100", accountId);

                foreach (var row in db.EvaluateTable(query))
                {
                    var game = GetGame((int)row["game_id"]);
                    if (game != null)
                        result.Add(game);
                }
            }
            return result;
        }

        public static List<Game> GetActiveGames()
        {
            var result = new List<Game>();
            using (var db = CreateDB())
            {
                foreach (var row in db.EvaluateTable("select Id from game"))
                {
                    result.Add(GetGame((int)row["Id"]));
                }
            }
            return result;
        }

        public static void PlayerJoined(Game game, int accountId)
        {
            SaveGame(game);
            using (var db = CreateDB())
            {
                db.Execute("delete from player where game_id = {0} and account_id = {1} and isInvite = 1", game.Id, accountId);
                db.Execute("insert into player (game_id, account_id) values ({0}, {1}) ", game.Id, accountId);
            }
        }

        public static void PlayerUnjoined(Game game, int accountId)
        {
            using (var db = CreateDB())
            {
                db.Execute("delete from player where game_id = {0} and account_id = {1}", game.Id, accountId);

                if (game.Players.Count <= 0)
                    KillGame(game.Id);
            }
        }

        public static void PlayerInvited(Game game, Account account)
        {
            SaveGame(game);
            using (var db = CreateDB())
            {
                db.Execute("insert into player (game_id, account_id, isInvite) values ({0}, {1}, 1) ", game.Id, account.Id);
            }
        }

        public static void KillGame(int gameId)
        {
            using (var db = CreateDB())
            {
                db.Execute("delete from game where id = {0}", gameId);
                db.Execute("delete from player where game_id = {0}", gameId);
            }
        }
    }
}