using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using ProtoBuf;
using System.Collections;
using GlobalCombat.Core;

namespace WebGame
{
	public class TourneyRound
	{
        public Tourney Tourney;
        public int Number;
        public int StartGame;
        public int GameCount;
        public int GameSize;
        public int WinnersOfRoundNumber;
        public int LosersOfRoundNumber;
    }
}
