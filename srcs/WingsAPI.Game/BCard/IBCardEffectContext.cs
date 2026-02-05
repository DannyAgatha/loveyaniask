using WingsEmu.DTOs.BCards;
using WingsEmu.Game.Battle;
using WingsEmu.Game.Entities;
using WingsEmu.Game.Helpers.Damages;
using WingsEmu.Packets.ServerPackets.Battle;

namespace WingsEmu.Game.Buffs;

/// <summary>
///     Bcard event context
/// </summary>
public interface IBCardEffectContext
{
    IBattleEntity Sender { get; }
    IBattleEntity Target { get; }
    SkillInfo Skill { get; }
    BCardDTO BCard { get; }
    Position Position { get; }
    SuPacketHitMode HitMode { get; }
    int DamageDealt { get; }
}