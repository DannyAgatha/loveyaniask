using ProtoBuf;

namespace WingsAPI.Data.Character;

[ProtoContract]
public class RainbowBattleLeaverBusterDto
{
    /// <summary>
    ///     The number of times a player has left the Rainbow Battle
    /// </summary>
    [ProtoMember(1)]
    public byte Exits { get; set; }

    /// <summary>
    ///     The amount of prizes the player will not get
    /// </summary>
    [ProtoMember(2)]
    public short RewardPenalty { get; set; }
}