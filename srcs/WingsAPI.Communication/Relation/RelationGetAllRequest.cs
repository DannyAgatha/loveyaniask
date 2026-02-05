using ProtoBuf;

namespace WingsAPI.Communication.Relation
{
    [ProtoContract]
    public class RelationGetAllRequest
    {
        [ProtoMember(1)]
        public long CharacterId { get; init; }
    }
}