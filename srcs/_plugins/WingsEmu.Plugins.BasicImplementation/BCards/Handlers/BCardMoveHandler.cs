// NosEmu
// 


using System;
using WingsEmu.Game.Buffs;
using WingsEmu.Game.Characters;
using WingsEmu.Game.Entities;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Networking;

namespace NosEmu.Plugins.BasicImplementations.BCards.Handlers;

public class BCardMoveHandler : IBCardEffectAsyncHandler
{
    public BCardType HandledType => BCardType.Move;

    public void Execute(IBCardEffectContext ctx)
    {
        if (ctx.Sender is IMonsterEntity monsterEntity)
        {
            monsterEntity.RefreshStats();
        }

        IClientSession session = (ctx.Sender as IPlayerEntity)?.Session;
        if (session?.PlayerEntity == null)
        {
            return;
        }

        session.PlayerEntity.LastSpeedChange = DateTime.UtcNow;
        session.RefreshStat();
        session.SendCondPacket();
    }
}