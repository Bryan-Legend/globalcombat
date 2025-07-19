using System;
using System.Collections.Generic;
using System.Text;
using ProtoBuf;
using System.ComponentModel;

namespace GlobalCombat.Core
{
    [ProtoContract]
    public class Player
	{
        [ProtoMember(1)]
        public int AccountId { get; set; }
        [ProtoMember(2)]
        public int Number;
        [ProtoMember(3)]
        public string Name;

        [ProtoMember(4)]
        public bool Done;
        //[ProtoMember(5)]
        //[DefaultValue(true)]
        //public bool Playing = true;

        [ProtoMember(6)]
        public int Areas;
        [ProtoMember(7)]
        public int Armies;
        [ProtoMember(8)]
        public int UnassignedArmies;

        [ProtoMember(9)]
        public int Place { get; set; }
        [ProtoMember(10)]
        public double Score { get; set; }
        [ProtoMember(11)]
        public double ScoreExpected { get; set; }
        [ProtoMember(12)]
        public int Rating { get; set; }
        [ProtoMember(13)]
        public int RatingChange { get; set; }

        public bool IsEliminated
        {
            get { return Place > 0; }
        }

        public string GetColor()
        {
            switch (Number % 9)
            {
                case 1: return "#005087";
                case 2: return "#357727";
                case 3: return "#FFE45F";
                case 4: return "#D45D00";
                case 5: return "#89547F";
                case 6: return "#50504C";
                case 7: return "#862215";
                case 8: return "#633D2E";
                default: return "#000000";
            }
        }

        public string GetPlace()
        {
            switch (Place)
            {
                case 1:
                    return "1st Place";
                case 2:
                    return "2nd Place";
                case 3:
                    return "3rd Place";
                default:
                    return Place + "th Place";
            }
        }

		public override string ToString()
		{
			return String.Format("{0} {1}: {2} armies.  {3} unassigned.  {4} areas.{5}", Number, Name, Armies, UnassignedArmies, Areas, Done ? "  DONE" : String.Empty);
		}
    }
}
