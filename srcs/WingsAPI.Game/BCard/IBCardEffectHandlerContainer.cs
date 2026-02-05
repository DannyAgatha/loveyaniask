using WingsEmu.DTOs.BCards;
using WingsEmu.Game.Battle;
using WingsEmu.Game.Entities;
using WingsEmu.Game.Helpers.Damages;
using WingsEmu.Packets.ServerPackets.Battle;

namespace WingsEmu.Game.Buffs;

public interface IBCardEffectHandlerContainer
{
    void Register(IBCardEffectAsyncHandler handler);

    void Unregister(IBCardEffectAsyncHandler handler);

    void Execute(IBattleEntity target, IBattleEntity sender, BCardDTO bCard, SkillInfo skill = null, Position position = default,
        BCardNpcMonsterTriggerType triggerType = BCardNpcMonsterTriggerType.NONE, SuPacketHitMode hitMode = SuPacketHitMode.SuccessAttack, int damageDealt = 0);
}