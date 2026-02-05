using WingsEmu.DTOs.BCards;
using WingsEmu.Game.Battle;
using WingsEmu.Game.Entities;
using WingsEmu.Game.Helpers.Damages;
using WingsEmu.Packets.ServerPackets.Battle;

namespace WingsEmu.Game.Buffs;

public interface IBCardEventContextFactory
{
    IBCardEffectContext NewContext(IBattleEntity sender, IBattleEntity target, BCardDTO bCard, SkillInfo skill = null, Position position = default, SuPacketHitMode hitMode = SuPacketHitMode.SuccessAttack, int damageDealt = 0);
}