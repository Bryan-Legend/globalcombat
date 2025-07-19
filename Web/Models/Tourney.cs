using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using ProtoBuf;
using System.Collections;
using GlobalCombat.Core;
using LT;

namespace WebGame
{
	public class Tourney
	{
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Status { get; set; }
        public int MaxPlayers { get; set; }
        public DateTime CreateTime { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public int GameSize { get; set; }
        public int Winners { get; set; }
        public bool IsDoubleElimination { get; set; }
        //public int Kitty { get; set; }
        //public int Cost { get; set; }
        //public string Options { get; set; }
        public bool AutoStart { get; set; }
        public bool Recurring { get; set; }
        public int OptionGameId { get; set; }

        public List<Account> Players = new List<Account>();
        public List<Game> Games = new List<Game>();

        public List<TourneyRound> WinnerBracket = new List<TourneyRound>();
        public List<TourneyRound> LoserBracket = new List<TourneyRound>();
        public TourneyRound FinalRound = new TourneyRound();

        public int CurrentPlayers { get { return Players.Count; } }
        public int InitialGames
        {
            get
            {
                if (GameSize == 0)
                    return 0;
                return MaxPlayers / GameSize;
            }
            set { MaxPlayers = value * GameSize; }
        }
        public int Losers { get { return GameSize - Winners; } }
        public bool IsStarted { get { return Status != "New"; } }
        public bool IsEnded { get { return Status == "Finished"; } }

        Game optionGame;
        public Game OptionGame
        {
            get
            {
                if (optionGame == null)
                    optionGame = GameServer.GetGame(OptionGameId);
                return optionGame;
            }
        }

        public Tourney()
        {
            Status = "New";
            GameSize = 2;
            InitialGames = 4;
            Winners = 1;
            OptionGameId = 700460;
            AutoStart = true;
        }

        public void CreateTourneyGame(DBConnection db, int gameNumber, TourneyRound round, List<TourneyRound> allRound)
        {
            var newGame = new Game() { GameName = Name + " - Game " + gameNumber, MaxPlayers = round.GameSize, TourneyId = Id };
            
            newGame.TurnLength = OptionGame.TurnLength;
            newGame.IsFogged = OptionGame.IsFogged;
            newGame.IsNonRandom = OptionGame.IsNonRandom;
            newGame.MapName = OptionGame.MapName;
            newGame.MinimumArmies = OptionGame.MinimumArmies;
            newGame.ReverseAttackOrder = OptionGame.ReverseAttackOrder;
            
            GameServer.SaveNewGame(newGame);

            // insert the tourneygame row
            var winnerRound = (from r in allRound where r.WinnersOfRoundNumber == round.Number || r.LosersOfRoundNumber == -round.Number select r).FirstOrDefault();
            var loserRound = (from r in allRound where r.LosersOfRoundNumber == round.Number select r).FirstOrDefault();
            db.Execute("insert into tourneygame (tourney_id, game_id, game_num, round, winners, winner_round, loser_round) values ({0}, {1}, {2}, {3}, {4}, {5}, {6})", Id, newGame.Id, gameNumber, round.Number, Winners, winnerRound == null ? 0 : winnerRound.Number, loserRound == null ? 0 : loserRound.Number);
        }

        public void Start()
        {
            using (var db = new DBConnection())
            {
                if (db.Evaluate("select status from tourney where id = {0}", Id) as String != "New")
                    throw new Exception(String.Format("Attempt to start tourney {0} that has already started.", Id));

                db.Execute("update tourney set status = 'Running' where id = " + Id);

                var allRounds = new List<TourneyRound>();
                allRounds.AddRange(WinnerBracket);
                if (IsDoubleElimination)
                {
                    allRounds.AddRange(LoserBracket);
                    allRounds.Add(FinalRound);
                }

                foreach (var round in allRounds)
                {
                    CreateRoundGames(db, round, allRounds);
                }

                var players = db.EvaluateTable("select t.*, a.* from tourneyplayer t, account a where a.id = t.account_id and tourney_id = " + Id);
                Shuffle(players);

                int currentPlayer = 0;
                foreach (var gameRow in db.EvaluateTable("select * from tourneygame where round = 1 and tourney_id = " + Id))
                {
                    var game = GameServer.GetGame((int)gameRow["game_id"]);
                    for (int playerCount = 0; playerCount < GameSize; playerCount++)
                    {
                        var player = players[currentPlayer];

                        game.Join((int)player["account_id"], (string)player["name"], (int)player["rating"]);
                        GameServer.PlayerJoined(game, (int)player["account_id"]);

                        currentPlayer++;
                    }
                }

                if (Recurring)
                    CreateTournament(this);
            }
        }

        // http://stackoverflow.com/questions/273313/randomize-a-listt-in-c-sharp
        public static void Shuffle<T>(IList<T> list)
        {
            var rng = new Random();
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = rng.Next(n + 1);
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }

        public static string CreateTournament(Tourney tourney)
        {
            using (var db = new DBConnection())
            {
                if (tourney.InitialGames < 2)
                    return "At least two initial games required";

                if (tourney.InitialGames > 8 && tourney.IsDoubleElimination)
                    return "A max of 8 initial games with double elimination.";

                if (tourney.InitialGames != 2 && tourney.InitialGames != 4 && tourney.InitialGames != 8 && tourney.InitialGames != 16 && tourney.InitialGames != 32 && tourney.InitialGames != 64 && tourney.InitialGames != 128 && tourney.InitialGames != 256)
                    return "Initial games must be a power of two.";

                if (tourney.Winners >= tourney.GameSize)
                    return "Winners must be less than initial game size.";

                var oldName = tourney.Name;

                // Handle recurring tourney name incrementing
                if (tourney.Recurring)
                {
                    if (tourney.Name.Contains("#"))
                    {
                        string[] splitName = tourney.Name.Split('#');
                        int tourneyCounter = Int32.Parse(splitName[1]) + 1;
                        tourney.Name = String.Format("{0}#{1}", splitName[0], tourneyCounter);
                    }
                    else
                        tourney.Name = tourney.Name + " #1";
                }

                db.Execute
                (
                    String.Format
                    (
                        "insert into tourney (name, players, create_time, gamesize, winners, doubleelim, OptionGameId, autostart, recurring, description) values ('{0}', {1}, {2}, {3}, {4}, '{5}', '{6}', '{7}', '{8}', '{9}')",
                        DBConnection.AddSlashes(tourney.Name),
                        tourney.InitialGames * tourney.GameSize,
                        Utility.UnixTimestamp(DateTime.UtcNow),
                        tourney.GameSize,
                        tourney.Winners,
                        tourney.IsDoubleElimination ? "True" : "False",
                        tourney.OptionGameId,
                        tourney.AutoStart ? "True" : "False",
                        tourney.Recurring ? "True" : "False",
                        DBConnection.AddSlashes(tourney.Description)
                    )
                );

                tourney.Name = oldName;

                return "Tournament Created.";
            }
        }

        void CreateRoundGames(DBConnection db, TourneyRound round, List<TourneyRound> allRounds)
        {
            for (var gameNumber = round.StartGame; gameNumber < round.StartGame + round.GameCount; gameNumber++)
            {
                CreateTourneyGame(db, gameNumber, round, allRounds);
            }
        }

        public static Tourney Load(Hashtable row)
        {
            if (row == null)
                return null;

            var tourney = new Tourney();

            tourney.Id = (int)row["id"];
            tourney.Name = (string)row["name"];
            tourney.Description = (string)row["description"];
            tourney.Status = (string)row["status"];
            tourney.MaxPlayers = (int)row["players"];
            tourney.CreateTime = Utility.FromUnixTimestamp((int)row["create_time"]);
            tourney.StartTime = Utility.FromUnixTimestamp((int)row["start_time"]);
            tourney.EndTime = Utility.FromUnixTimestamp((int)row["end_time"]);
            tourney.GameSize = (int)row["gamesize"];
            tourney.Winners = (int)row["winners"];
            tourney.IsDoubleElimination = (string)row["doubleelim"] == "True";
            //result.Kitty = (int)row["kitty"];
            //result.Cost = (int)row["cost"];
            //tourney.Options = (string)row["Options"];
            tourney.AutoStart = (string)row["AutoStart"] == "True";
            tourney.Recurring = (string)row["Recurring"] == "True";
            tourney.OptionGameId = (int)row["OptionGameId"];

            tourney.BuildRounds();

            return tourney;
        }

        public void BuildRounds()
        {
            int currentRound = 1;
            int startGame = 1;

            // intial round
            WinnerBracket.Add(new TourneyRound() { Tourney = this, Number = currentRound, GameCount = InitialGames, GameSize = GameSize, StartGame = startGame });
            startGame += InitialGames;

            var previousGameCount = InitialGames;
            while (previousGameCount > 1)
            {
                currentRound++;
                previousGameCount = previousGameCount / 2;
                WinnerBracket.Add(new TourneyRound() { Tourney = this, Number = currentRound, GameCount = previousGameCount, GameSize = Winners * 2, WinnersOfRoundNumber = currentRound - 1, StartGame = startGame });
                startGame += previousGameCount;
            }

            if (IsDoubleElimination)
            {
                int WinnerRound = 1;
                currentRound++;
                previousGameCount = InitialGames / 2;
                LoserBracket.Add(new TourneyRound() { Tourney = this, Number = currentRound, GameCount = previousGameCount, GameSize = Losers * 2, LosersOfRoundNumber = WinnerRound, StartGame = startGame });
                startGame += previousGameCount;

                int WinnerRoundGameCount = InitialGames / 2;
                while (previousGameCount > 1)
                {
                    int count = previousGameCount / 2;
                    bool AddFlag = true;
                    if ((WinnerRoundGameCount == 2) && (count > 1))
                        AddFlag = false;
                    if (WinnerRoundGameCount < 2)
                        AddFlag = false;
                    // TODO: if $PrevGameCount(times two?) + $ThisRoundGameCount != power of 2, then AddFlag = 0;
                    // otherwise there will be bad round game counts when initial game > 16
                    // may replace the above two checks

                    currentRound++;
                    if (!AddFlag)
                    {
                        previousGameCount = previousGameCount / 2;
                        LoserBracket.Add(new TourneyRound() { Tourney = this, Number = currentRound, GameCount = previousGameCount, GameSize = Winners * 2, WinnersOfRoundNumber = currentRound - 1, StartGame = startGame });
                        startGame += previousGameCount;
                    }
                    else
                    {
                        WinnerRound++;
                        WinnerRoundGameCount = WinnerRoundGameCount / 2;
                        LoserBracket.Add(new TourneyRound() { Tourney = this, Number = currentRound, GameCount = previousGameCount, GameSize = Winners * 2, WinnersOfRoundNumber = currentRound - 1, LosersOfRoundNumber = WinnerRound, StartGame = startGame });
                        startGame += previousGameCount;
                    }
                }

                WinnerRound++;
                currentRound++;
                LoserBracket.Add(new TourneyRound() { Tourney = this, Number = currentRound, GameCount = 1, GameSize = Winners * 2, WinnersOfRoundNumber = currentRound - 1, LosersOfRoundNumber = WinnerRound, StartGame = startGame });
                startGame += 1;

                currentRound++;
                FinalRound = new TourneyRound { Tourney = this, Number = currentRound, GameCount = 1, GameSize = Winners * 2, WinnersOfRoundNumber = WinnerRound, LosersOfRoundNumber = -(currentRound - 1), StartGame = startGame };
            }
        }

        public void LoadGames(DBConnection db)
        {
            foreach (var tourneyGameRow in db.EvaluateTable("select * from tourneygame where tourney_id = {0} order by game_num", Id))
            {
                Games.Add(GameServer.GetGame((int)tourneyGameRow["game_id"]));
            }
        }

        //if (Status == "Running")
        //{
        //    int lastGameStatus = Convert.ToInt32(Evaluate("SELECT game.status FROM tourneygame JOIN game WHERE tourneygame.tourney_id = " + Tourney["id"] + " and game.id = tourneygame.game_id ORDER BY game_num DESC"));
        //    if (lastGameStatus == 2)
        //        Execute("update tourney set status = 'Finished' where id = " + Tourney["id"]);
        //}	

        public bool IsPlaying(int accountId)
        {
            return (from p in Players where p.Id == accountId select p).FirstOrDefault() != null;
        }

        public Game GetGame(int gameNumber)
        {
            if (Games.Count <= gameNumber - 1)
                return null;
            return Games[gameNumber - 1];
        }

        public static void TourneyFinishedCheck(DBConnection db, Game game)
        {
            // check to see if tourney ended
            var tourneyGameNumber = db.Evaluate("select game_num from tourneygame where game_id = " + game.Id) as int?;
            if (tourneyGameNumber == null)
                return;

            var maxTourneyGameNumber = db.Evaluate("select max(game_num) from tourneygame where tourney_id = {0}", game.TourneyId) as int?;
            if (maxTourneyGameNumber != null && maxTourneyGameNumber == tourneyGameNumber)
                db.Execute("update tourney set status = 'Finished' where id = {0}", game.TourneyId);
        }

        public static void PlayerFinishedCheck(DBConnection db, Game game, Player player)
        {
            if (player == null)
                return;

            var tourneyGame = db.EvaluateRow("select * from tourneygame where game_id = " + game.Id);
            if (tourneyGame != null)
            {
                int targetRound = 0;

                if (player.Place > (int)tourneyGame["winners"])
                    targetRound = (int)tourneyGame["loser_round"];
                else
                    targetRound = (int)tourneyGame["winner_round"];

                if (targetRound != 0)
                {
                    foreach (var tourneyGameRow in db.EvaluateTable("select * from tourneygame where tourney_id = {0} and round = {1} order by game_num", game.TourneyId, targetRound))
                    {
                        var targetGame = GameServer.GetGame((int)tourneyGameRow["game_id"]);
                        if (!targetGame.Started)
                        {
                            targetGame.Join(player.AccountId, player.Name, player.Rating);
                            GameServer.PlayerJoined(targetGame, player.AccountId);
                            return;
                        }
                    }

                    throw new Exception("Global Combat Tourney Error: Game not found for " + game.TourneyId + ".  Account " + player.AccountId + " is unable to join round " + targetRound + " and tourney is now messed up.");
                }
            }
        }
    }
}
