using System.Collections.Generic;
using PhoenixLib.Logging;
using WingsEmu.DTOs.BCards;
using WingsEmu.Game.Battle;
using WingsEmu.Game.Buffs;
using WingsEmu.Game.Entities;
using WingsEmu.Game.Helpers.Damages;
using WingsEmu.Packets.ServerPackets.Battle;

namespace NosEmu.Plugins.BasicImplementations.BCards;

public class BCardHandlerContainer : IBCardEffectHandlerContainer
{
    private readonly IBCardEventContextFactory _contextFactory;
    private readonly Dictionary<BCardType, IBCardEffectAsyncHandler> _handlers;

    public BCardHandlerContainer(IBCardEventContextFactory contextFactory)
    {
        _contextFactory = contextFactory;
        _handlers = new Dictionary<BCardType, IBCardEffectAsyncHandler>();
    }

    public void Register(IBCardEffectAsyncHandler handler)
    {
        _handlers.Add(handler.HandledType, handler);
        Log.Debug($"[BCARD][REGISTER_HANDLER] BCARDTYPE: {handler.HandledType} REGISTERED!");
    }

    public void Unregister(IBCardEffectAsyncHandler handler)
    {
        _handlers.Remove(handler.HandledType);
    }

    public void Execute(IBattleEntity target, IBattleEntity sender, BCardDTO bCard, SkillInfo skill = null, Position position = default,
        BCardNpcMonsterTriggerType triggerType = BCardNpcMonsterTriggerType.NONE, SuPacketHitMode hitMode = SuPacketHitMode.SuccessAttack, int damageDealt = 0)
    {
        if (target == null)
        {
            return;
        }

        if (!_handlers.TryGetValue((BCardType)bCard.Type, out IBCardEffectAsyncHandler handler))
        {
            return;
        }

        IBCardEffectContext context = _contextFactory.NewContext(sender, target, bCard, skill, position, hitMode, damageDealt);
        handler.Execute(context);
    }
}