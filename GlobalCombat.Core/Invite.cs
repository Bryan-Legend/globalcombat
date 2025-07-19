using System;
using System.Collections.Generic;
using System.Text;
using ProtoBuf;

namespace GlobalCombat.Core
{
    [ProtoContract]
    public class Invite
	{
        [ProtoMember(1)]
        public int AccountId { get; set; }
        [ProtoMember(2)]
        public string Name;
    }
}
