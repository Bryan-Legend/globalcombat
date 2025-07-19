using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using ProtoBuf;
using System.IO;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace GlobalCombat.Core
{
    [ProtoContract]
    public class Game
    {
        public static Action<Game, string, int, string> OnMessage;
        public static Action<Game> OnStart;
        public static Action<Game, Player> OnDone;
        public static Action<Game, string> OnRunTurn;
        public static Action<Game, Player> OnEliminated;
        public static Action<Game, Player> OnEnd;

        [ProtoMember(1)]
        public int Id { get; set; }
        [ProtoMember(2)]
        public string GameName { get; set; }

        [Required]
        [Display(Name = "Map Name")]
        [ProtoMember(3)]
        public MapName MapName { get; set; }

        [Required]
        [Display(Name = "Turn Timeout Length")]
        [Range(1, 5000, ErrorMessage = "Turn length must be between 1 and 5000 minutes")]
        [ProtoMember(4)]
        public int TurnLength { get; set; }

        [Required]
        [Display(Name = "Max Number of Players")]
        [Range(2, 8, ErrorMessage = "Max players must be between 2 and 8")]
        [ProtoMember(5)]
        public int MaxPlayers { get; set; }

        [Required]
        [Display(Name = "Fog of War")]
        [ProtoMember(6)]
        public bool IsFogged { get; set; }

        [Required]
        [Display(Name = "Non-Random Attacks")]
        [ProtoMember(7)]
        public bool IsNonRandom { get; set; }

        [Required]
        [Display(Name = "Reverse Attack Order")]
        [ProtoMember(8)]
        public bool ReverseAttackOrder { get; set; }

        [Required]
        [Display(Name = "Minimum Army Bonus")]
        [Range(0, 5, ErrorMessage = "Min armies must be between 0 and 5")]
        [ProtoMember(9)]
        public int MinimumArmies { get; set; }

        [Required]
        [Display(Name = "Invite Only")]
        [ProtoMember(20)]
        public bool IsPrivate { get; set; }

        [Required]
        [Display(Name = "Training Mode")]
        [ProtoMember(21)]
        public bool IsTraining { get; set; }

        [ProtoMember(10)]
        public int Turn { get; set; }
        [ProtoMember(11)]
        public DateTime PreviousTurnTime { get; set; }
        [ProtoMember(12)]
        public DateTime LastTurnTime { get; set; }

        [ProtoMember(13)]
        public bool Started { get; set; }
        [ProtoMember(14)]
        public DateTime StartTime { get; set; }

        [ProtoMember(15)]
        public bool Ended { get; set; }
        [ProtoMember(16)]
        public DateTime EndTime { get; set; }

        [ProtoMember(17, AsReference = true)]
        public List<Area> Areas { get; set; }
        [ProtoMember(18, AsReference = true)]
        public List<Player> Players { get; set; }

        [ProtoMember(22, IsRequired = false)]
        public List<Invite> Invites { get; set; }

        [ProtoMember(23, IsRequired = false)]
        public int TourneyId { get; set; }

        public MapInfo MapInfo
        {
            get { return MapInfo.GetMap(MapName); }
        }

        public bool Running
        {
            get { return Started && !Ended; }
        }

        public int CurrentPlayers
        {
            get { return Players.Count; }
        }

        public TimeSpan TimeLeft
        {
            get
            {
                return LastTurnTime + TimeSpan.FromMinutes(TurnLength) - DateTime.UtcNow;
            }
        }

        public int Status
        {
            get
            {
                if (!Started)
                    return 0;
                return Ended ? 2 : 1;
            }
        }

        public string TurnLengthDisplay
        {
            get
            {
                if (TurnLength == 1)
                    return "1 Minute";
                else if (TurnLength <= 60)
                    return TurnLength + " Minutes";
                else if (TurnLength <= 1440)
                    return (TurnLength / 60) + " Hours";
                else
                    return (TurnLength / 1440) + " Days";
            }
        }

        public static Random Random = new Random();

        public Game()
        {
            MapName = MapName.Original;
            MaxPlayers = 2;
            TurnLength = 1440;
            Turn = 1;

            Players = new List<Player>();
            Areas = new List<Area>();
            Invites = new List<Invite>();
        }

        public Player GetPlayer(int accountId)
        {
            foreach (var player in Players)
            {
                if (player.AccountId == accountId)
                    return player;
            }
            return null;
        }

        public Player GetPlayerByNumber(int playerNumber)
        {
            return (from p in Players where p.Number == playerNumber select p).FirstOrDefault();
        }

        public Area GetArea(int areaId)
        {
            return (from a in Areas where a.AreaInfo.Number == areaId select a).FirstOrDefault();
        }

        public Player Join(int accountId, string name, int rating)
        {
            var result = new Player() { AccountId = accountId, Name = name, Rating = rating };
            Join(result);
            return result;
        }

        public void Join(Player player)
        {
            Players.Add(player);
            player.Number = Players.Count;

            foreach (var invite in (from i in Invites where i.AccountId == player.AccountId select i))
            {
                Invites.Remove(invite);
                break;
            }

            SendForumMessage(player.Name + " joined the game.");

            if (Players.Count >= MaxPlayers)
                Start();
        }

        public void Unjoin(Player player)
        {
            Players.Remove(player);

            UpdatePlayerNumbers();

            SendForumMessage(player.Name + " left the game.");
        }

        void UpdatePlayerNumbers()
        {
            // update player numbers
            foreach (var player in Players)
                player.Number = Players.IndexOf(player) + 1;
        }

        public void Done(Player target)
        {
            if (target.Done)
                return;

            target.Done = true;

            foreach (var player in Players)
            {
                if (!player.Done)
                {
                    if (OnDone != null)
                        OnDone(this, target);
                    return;
                }
            }

            RunTurn();
        }

        public void SendForumMessage(string message, int sourceId = 1, string sourceName = "Computer")
        {
            if (OnMessage != null)
                OnMessage(this, message, sourceId, sourceName);
        }

        public void Start()
        {
            // make sure not already created
            if (Started)
                throw new Exception("Game already started.");

            if (Players.Count < 2)
                throw new Exception("Must have 2 players to start.");

            Started = true;

            UpdatePlayerNumbers();

            Areas.Clear();
            foreach (AreaInfo area in MapInfo.Areas)
                Areas.Add(new Area() { AreaInfo = area, Number = area.Number });

            List<int> tempPlayerAreas = new List<int>();
            int initalAreaCount;
            do
            {
                tempPlayerAreas.Clear();

                // evenly divide areas among players
                int areaCount = MapInfo.NumAreas;
                initalAreaCount = MapInfo.NumAreas / CurrentPlayers;
                foreach (Player p in Players)
                {
                    p.Areas = initalAreaCount;
                    tempPlayerAreas.Add(initalAreaCount);
                    areaCount -= initalAreaCount;
                }

                // divy up remainder
                while (areaCount > 0)
                {
                    Player player = Players[Random.Next(CurrentPlayers)];
                    if (player.Areas == initalAreaCount)
                    {
                        player.Areas++;
                        tempPlayerAreas[player.Number - 1]++;
                        areaCount--;
                    }
                }

                // assign areas
                foreach (Area area in Areas)
                {
                    // decide an owner
                    Player owner;
                    do
                    {
                        owner = Players[Random.Next(CurrentPlayers)];
                    } while (tempPlayerAreas[owner.Number - 1] == 0);
                    tempPlayerAreas[owner.Number - 1]--;
                    area.Owner = owner;
                }
            } while (IsAnyRegionOwned());

            // give inital armies to place and give army bonus for players who didn't get country
            foreach (Player player in Players)
            {
                player.UnassignedArmies = 20;
                if (player.Areas == initalAreaCount)
                    player.UnassignedArmies += 5;
                player.Armies = (player.Areas * 5) + player.UnassignedArmies;
            }

            // update game status
            LastTurnTime = DateTime.UtcNow;
            StartTime = DateTime.UtcNow;

            // send player messages
            SendForumMessage("Game #" + GameName + " Started");

            if (OnStart != null)
                OnStart(this);
        }

        public void RunTurn()
        {
            if (Ended)
                return;

            Turn++;
            PreviousTurnTime = LastTurnTime;
            LastTurnTime = DateTime.UtcNow;

            ResetDoneFlags();

            StringBuilder attackMessage = new StringBuilder();
            StringBuilder armyBonusMessage = new StringBuilder();

            // assign armies
            foreach (Area area in Areas)
            {
                area.Armies += area.AssignedArmies;
                area.AssignedArmies = 0;
            }

            // do transfers
            foreach (Area area in Areas)
            {
                if (area.Command == Command.Transfer)
                    DoTransfer(area);
            }

            // do attacks
            var sorted = new List<Area>();
            if (!ReverseAttackOrder)
                sorted = (from a in Areas orderby a.Amount descending select a).ToList();
            else
                sorted = (from a in Areas orderby a.Amount ascending select a).ToList();
            foreach (Area area in sorted)
            {
                if (area.Command == Command.Attack)
                    attackMessage.Append(DoAttack(area));
            }

            // clear commands
            foreach (Area a in Areas)
            {
                a.Amount = 0;
                a.Command = Command.None;
                a.Target = null;
            }

            // calc new armies, clear done_flags, check win/eliminates
            int alivePlayers = 0;
            foreach (Player player in Players)
            {
                if (!player.IsEliminated)
                {
                    if (player.Areas == 0)
                        EliminatePlayer(player);
                    else
                    {
                        alivePlayers++;
                        int newArmies = player.Areas / 2;

                        // region army bonuses
                        int regionBonus = 0;
                        foreach (RegionInfo region in MapInfo.Regions)
                        {
                            // I doubt that LINQ will be available in XNA.  So this query will probably need to be rewritten.
                            //var areasOwned = from a in Areas where a.Owner == player && a.AreaInfo.Region == region.Number select a;

                            int areasOwned = 0;
                            foreach (Area area in Areas)
                                if (area.Owner == player && area.AreaInfo.Region == region.Number)
                                    areasOwned++;

                            if (areasOwned == region.NumAreas)
                                regionBonus += region.ArmyBonus;
                        }
                        newArmies += regionBonus;

                        // check for minimum armies
                        if (newArmies < MinimumArmies)
                            newArmies = MinimumArmies;

                        armyBonusMessage.AppendFormat("\n &nbsp; &nbsp; &nbsp;<font color=\"{0}\">{1}</font>: {2} new armies ({3} from Region Bonuses)", player.GetColor(), player.Name, newArmies, regionBonus);

                        int totalArmies = 0;
                        foreach (Area area in Areas)
                        {
                            if (area.Owner == player)
                                totalArmies += area.Armies;
                        }

                        player.UnassignedArmies += newArmies;
                        player.Armies = totalArmies + player.UnassignedArmies;
                    }
                }
            }

            string resultMessage;
            if (attackMessage.Length != 0)
                resultMessage =
                    String.Format
                    (
                        "<b>Turn {0} Results</b>\nAttacks{1}\nArmy Bonuses{2}",
                        Turn - 1,
                        attackMessage,
                        armyBonusMessage
                    );
            else
                resultMessage =
                    String.Format
                    (
                        "<b>Turn {0} Results</b>\nArmy Bonuses{1}",
                        Turn - 1,
                        armyBonusMessage
                    );

            // send messages
            SendForumMessage(resultMessage);

            if (OnRunTurn != null)
                OnRunTurn(this, resultMessage);

            // check for endgame
            if (alivePlayers <= 1)
                End();
        }

        public void ResetDoneFlags()
        {
            // update done flags for all players except "computer"
            foreach (var player in Players)
            {
                if (player.AccountId != 1 && !player.IsEliminated)
                    player.Done = false;
                else
                    player.Done = true;
            }
        }

        public void DoTransfer(Area area)
        {
            area.Target.Armies += area.Amount;
            area.Armies -= area.Amount;
        }

        public string DoAttack(Area attacker)
        {
            Area defender = attacker.Target;

            // check ownership of dest
            if (attacker.Owner == defender.Owner)
                return String.Empty; // already won area

            // check amount
            if (attacker.Amount > attacker.Armies - 1)
            {
                attacker.Amount = attacker.Armies - 1;
                if (attacker.Amount <= 0)
                    return String.Empty;
            }

            // generated attack damage
            int attackDamage = 0;
            if (IsNonRandom)
                attackDamage = Convert.ToInt32(System.Math.Floor(attacker.Amount * .6));
            else
            {
                for (int count = 1; count <= attacker.Amount; count++) // for each attacking army
                {
                    if (Random.Next(1, 10 + 1) <= 6) // 60% chance of hit
                        attackDamage++;
                }
            }
            if (attackDamage > defender.Armies)
                attackDamage = defender.Armies;

            // generated defender damage
            int defendDamage = 0;
            if (IsNonRandom)
                defendDamage = Convert.ToInt32(System.Math.Floor(defender.Armies * .75));
            else
            {
                for (int count = 1; count <= defender.Armies; count++) // for each army
                {
                    if (Random.Next(1, 4 + 1) <= 3) // 75% chance of hit
                        defendDamage++;
                }
            }
            if (defendDamage > attacker.Amount)
                defendDamage = attacker.Amount;

            // send messages
            string attackMessage =
                String.Format
                (
                    "\n &nbsp; &nbsp; &nbsp;<font color={0}>{1}</font> w/ {2} armies ({3} lost) vs <font color={4}>{5}</font> w/ {6} armies ({7} ",
                    attacker.Owner.GetColor(),
                    attacker.Name,
                    attacker.Amount,
                    defendDamage,
                    defender.Owner.GetColor(),
                    defender.Name,
                    defender.Armies,
                    attackDamage
                );

            // deal damage
            if (attackDamage >= defender.Armies && defendDamage < attacker.Amount)
            {
                // Attacker won, take area
                attacker.Armies -= attacker.Amount;
                defender.Armies = attacker.Amount - defendDamage;

                attacker.Owner.Areas++;
                defender.Owner.Areas--;
                defender.Owner = attacker.Owner;

                defender.Command = Command.None;

                return attackMessage + "<b>lost</b>)";
            }
            else
            {
                // Defender won
                defender.Armies -= attackDamage;
                attacker.Armies -= defendDamage;
            }

            return attackMessage + "lost)";
        }

        public void EliminatePlayer(Player loser)
        {
            if (!loser.IsEliminated)
            {
                var place = (from p in Players where !p.IsEliminated select p).Count();

                loser.Areas = 0;
                loser.Armies = 0;
                loser.UnassignedArmies = 0;
                loser.Place = place;
                loser.Done = true;

                if (OnEliminated != null)
                    OnEliminated(this, loser);
            }

            if (loser.Place <= 2)
                End();
        }

        public bool ForceEndCheck()
        {
            return Running && LastTurnTime < DateTime.UtcNow.AddDays(-14) && StartTime < DateTime.UtcNow.AddDays(-14);
        }

        public void ForceEnd()        
        {
            SendForumMessage("Since the turn has not run in over 14 days the server has forced an end to this game.");

            foreach (var player in (from p in Players where !p.IsEliminated orderby p.Areas, p.Armies select p))
                EliminatePlayer(player);
        }

        public double GenScoreExpected(int playerRating, int opponentRating)
        {
            return 1 / (System.Math.Pow(10, ((opponentRating - playerRating) / 1500f)) + 1);
        }

        public double GetScoreExpected(Player player)
        {
            double scoreExpected = 0;
            foreach (var otherPlayer in Players)
            {
                if (otherPlayer.Number != player.Number)
                    scoreExpected += GenScoreExpected(player.Rating, otherPlayer.Rating);
            }
            scoreExpected /= CurrentPlayers - 1;
            return scoreExpected;
        }

        public double GenScore(int place)
        {
            return (1f / (CurrentPlayers - 1f)) * (CurrentPlayers - place);
        }

        public void End()
        {
            // make sure not already ended
            if (!Ended)
            {
                // Find winner
                var winner = (from p in Players where p.Place <= 1 select p).FirstOrDefault();

                // update winner
                if (winner != null)
                    winner.Place = 1;

                // do rating assignments
                if (!IsTraining)
                {
                    // get current rating
                    //foreach (Hashtable LPlayer in Players)
                    //    LPlayer["rating"] = Convert.ToInt32(Evaluate("select rating from account where id = " + LPlayer["id"]));

                    // generate scores, award points
                    foreach (var player in Players)
                    {
                        player.ScoreExpected = Math.Round(GetScoreExpected(player) * 100) / 100;
                        player.Score = Math.Round(GenScore(player.Place) * 100) / 100;
                        player.RatingChange = (int)System.Math.Round((player.Score - player.ScoreExpected) * 150);
                    }
                }

                // update game status
                Ended = true;
                EndTime = DateTime.UtcNow;

                if (OnEnd != null)
                    OnEnd(this, winner);
            }
        }

        // This function depends on all areas in a region to be sequential
        public bool IsAnyRegionOwned()
        {
            int areaCount = 0;
            foreach (RegionInfo region in MapInfo.Regions)
            {
                Player owner = Areas[areaCount].Owner;
                int innerCount;
                for (innerCount = 1; innerCount < region.NumAreas; innerCount++) // for each area in this region
                {
                    areaCount++;
                    if (Areas[areaCount].Owner != owner) // if owned by different player
                    {
                        areaCount += region.NumAreas - innerCount;
                        break;
                    }
                }

                if (innerCount == region.NumAreas)
                    return true;
            }
            return false;
        }

        public int SetAssigned(Area area, int amount)
        {
            if (amount > area.Owner.UnassignedArmies)
                amount = area.Owner.UnassignedArmies;

            if (amount > 0)
            {
                area.Owner.UnassignedArmies -= amount;
                area.AssignedArmies += amount;
            }
            return amount;
        }

        public int ClearAssigned(Area area)
        {
            var amount = area.AssignedArmies;
            area.Owner.UnassignedArmies += area.AssignedArmies;
            area.AssignedArmies = 0;
            area.Amount = Math.Min(area.Amount, area.Armies - 1);
            return amount;
        }

        public int SetTransfer(Area source, Area target, int amount)
        {
            if (source.Owner == target.Owner && source.AreaInfo.LinksTo(target.AreaInfo))
            {
                if (amount > source.TotalArmies - 1)
                    amount = source.TotalArmies - 1;

                if (amount >= 0)
                {
                    source.Amount = amount;
                    source.Command = Command.Transfer;
                    source.Target = target;
                }

                return amount;
            }
            return 0;
        }

        public int SetAttack(Area source, Area target, int amount)
        {
            if (source.Owner != target.Owner && source.AreaInfo.LinksTo(target.AreaInfo))
            {
                if (amount > source.TotalArmies - 1)
                    amount = source.TotalArmies - 1;

                if (amount >= 0)
                {
                    source.Amount = amount;
                    source.Command = Command.Attack;
                    source.Target = target;
                }

                return amount;
            }

            return 0;
        }

        public byte[] Save()
        {
            using (var result = new MemoryStream())
            {
                ProtoBuf.Serializer.Serialize(result, this);
                return result.ToArray();
            }
        }

        public static Game Load(byte[] data)
        {
            using (var stream = new MemoryStream(data))
            {
                var result = ProtoBuf.Serializer.Deserialize<Game>(stream);

                foreach (var area in result.Areas)
                {
                    area.AreaInfo = result.MapInfo.GetArea(area.Number);
                }

                if (String.IsNullOrEmpty(result.GameName))
                    result.GameName = "Game #" + result.Id;

                return result;
            }
        }

        public string DisplayGameStatus(int accountId, bool isAdmin)
        {
            var result = new StringBuilder();
            result.Append("<a href=\"Game-" + Id + "/\">" + GameName + "</a> ");

            switch (Status)
            {

                case 0:
                    for (int LCount = 0; LCount < MaxPlayers; LCount++)
                    {
                        if (LCount < CurrentPlayers)
                            result.Append("<img src=p.gif>");
                        else
                            result.Append("<img src=pe.gif>");
                    }

                    if (IsPrivate)
                        result.Append(" <img src=images/key.gif>");
                    break;

                case 1:
                    foreach (var player in Players)
                    {
                        if (player.IsEliminated)
                            result.Append("<img src=px.gif>");
                        else
                        {
                            if (player.AccountId == accountId)
                            {
                                if (player.Done)
                                    result.Append("<img src=pcd.gif>");
                                else
                                    result.Append("<img src=pc.gif>");
                            }
                            else
                            {
                                if (player.Done)
                                    result.Append("<img src=pd.gif>");
                                else
                                    result.Append("<img src=p.gif>");
                            }
                        }
                    }

                    result.Append(" Turn " + Turn);

                    if (TimeLeft.TotalSeconds > 0)
                        result.Append(" (" + Utility.PrintTimeSpan(TimeLeft, false) + ")");
                    else
                        result.Append(" <span class=\"error\">(0 sec)</span>");
                    break;
                default:
                    var finishedPlayer = Players.Where(p => p.AccountId == accountId).FirstOrDefault();
                    if (finishedPlayer != null)
                        result.Append(" " + finishedPlayer.GetPlace());
                    else
                        result.Append(" Finished");
                    break;
            }

            if (IsFogged)
                result.Append(" <img src=images/fog.gif>");

            if (isAdmin)
            {
                if (Status == 1) // running
                    result.Append(" <a href='javascript:if (confirm(\"You sure?\")) { self.location=\"Game-" + Id + "?ResetGame=1\" }'>RESET</a> ");
                if (Status < 2) // running or unstarted
                    result.Append(" <a href='javascript:if (confirm(\"You sure?\")) { self.location=\"Game-" + Id + "?KillGame=1\" }'>KILL</a> ");
            }

            result.Append("<br />");

            return result.ToString();
        }

        public void ForceTurn(Player player)
        {
            if (TimeLeft.TotalSeconds <= 0)
            {
                SendForumMessage(player.Name + " forced the turn to run.");
                RunTurn();
            }
        }
    }
}