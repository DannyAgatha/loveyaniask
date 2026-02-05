using ProtoBuf;

namespace WingsAPI.Data.Character
{
    [ProtoContract]
    public class CharacterCookingDto
    {
        [ProtoMember(1)]
        public long Amount { get; set; }

        [ProtoMember(2)]
        public long RecipeVnum { get; set; }
    }
}