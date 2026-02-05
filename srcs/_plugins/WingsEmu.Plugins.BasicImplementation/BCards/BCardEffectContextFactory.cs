using WingsEmu.DTOs.BCards;
using WingsEmu.Game.Battle;
using WingsEmu.Game.Buffs;
using WingsEmu.Game.Entities;
using WingsEmu.Game.Helpers.Damages;
using WingsEmu.Packets.ServerPackets.Battle;

namespace NosEmu.Plugins.BasicImplementations.BCards;

public class BCardEffectContextFactory : IBCardEventContextFactory
{
    public IBCardEffectContext NewContext(IBattleEntity sender, IBattleEntity target, BCardDTO bcard, SkillInfo skill = null, Position position = default, SuPacketHitMode hitMode = SuPacketHitMode.SuccessAttack, int damageDealt = 0)
        => new BcardEffectContext(sender, target, bcard, skill, position, hitMode, damageDealt);
}