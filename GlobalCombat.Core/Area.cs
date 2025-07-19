using System;
using System.Collections.Generic;
using System.Text;
using ProtoBuf;

namespace GlobalCombat.Core
{
    [ProtoContract]
	public class Area
	{
        [ProtoMember(1)]
        public int Number { get; set; }
        [ProtoMember(2, AsReference= true)]
        public Player Owner { get; set; }
        [ProtoMember(3)]
        public int Armies { get; set; }
        [ProtoMember(4)]
        public int AssignedArmies { get; set; }
        [ProtoMember(5)]
        public Command Command { get; set; }
        [ProtoMember(6, AsReference = true)]
        public Area Target;
        [ProtoMember(7)]
        public int Amount { get; set; }

		public AreaInfo AreaInfo;

        public string Name
        {
            get { return AreaInfo.Name; }
        }

        public int TotalArmies
        {
            get { return Armies + AssignedArmies; }
        }

		public Area()
		{
            Armies = 5;
		}

        public override string ToString()
        {
            return String.Format("{0} {1}: {2} {3} armies.  {4} {5} with {6}", AreaInfo.Region, Name, Owner.Name, Armies, Command, Target == null ? "nowhere" : Target.AreaInfo.Name, Amount);
        }

        //public static int ComparisonAscendingOrder(Area area1, Area area2) // will do smallest attacks first
        //{
        //    if (area1.Amount == area2.Amount)
        //        return area1.AreaInfo.Number.CompareTo(area2.AreaInfo.Number);
        //    return area1.Amount.CompareTo(area1.Amount);
        //}

        //public static int ComparisonDescendingOrder(Area area1, Area area2) // will do largest attacks first
        //{
        //    if (area1.Amount == area2.Amount)
        //        return area1.AreaInfo.Number.CompareTo(area2.AreaInfo.Number);
        //    return area2.Amount.CompareTo(area1.Amount);
        //}
	}
}
