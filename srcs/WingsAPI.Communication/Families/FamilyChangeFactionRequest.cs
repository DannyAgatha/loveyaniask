using ProtoBuf;
using WingsEmu.Packets.Enums;

namespace WingsAPI.Communication.Families
{
    [ProtoContract]
    public class FamilyChangeFactionRequest
    {
        [ProtoMember(1)]
        public long FamilyId { get; init; }

        [ProtoMember(2)]
        public FactionType NewFaction { get; init; }
    }
}