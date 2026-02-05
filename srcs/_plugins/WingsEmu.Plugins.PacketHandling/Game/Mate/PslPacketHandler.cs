using System;
using System.Threading.Tasks;
using PhoenixLib.Events;
using WingsEmu.Core.Extensions;
using WingsEmu.Game._i18n;
using WingsEmu.Game.Battle;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Extensions.Mates;
using WingsEmu.Game.Managers;
using WingsEmu.Game.Mates;
using WingsEmu.Game.Mates.Events;
using WingsEmu.Game.Networking;
using WingsEmu.Game.Networking.Broadcasting;
using WingsEmu.Packets.ClientPackets;
using WingsEmu.Packets.Enums;
using WingsEmu.Packets.Enums.Chat;

namespace WingsEmu.Plugins.PacketHandling.Game.Mate;

public class PslPacketHandler : GenericGamePacketHandlerBase<PslPacket>
{
    private readonly IDelayManager _delayManager;
    private readonly IAsyncEventPipeline _eventPipeline;
    private readonly IGameLanguageService _gameLanguage;

    public PslPacketHandler(IAsyncEventPipeline eventPipeline, IDelayManager delayManager, IGameLanguageService languageService)
    {
        _eventPipeline = eventPipeline;
        _delayManager = delayManager;
        _gameLanguage = languageService;
    }

    protected override async Task HandlePacketAsync(IClientSession session, PslPacket packet)
    {
        if (session.PlayerEntity.IsOnVehicle)
        {
            return;
        }

        IMateEntity mateEntity = session.PlayerEntity.MateComponent.GetMate(x => x.IsTeamMember && x.MateType == MateType.Partner);
        if (mateEntity == null)
        {
            return;
        }

        if (mateEntity.Specialist == null)
        {
            session.SendMsg(_gameLanguage.GetLanguage(GameDialogKey.PARTNER_MESSAGE_NO_SP_EQUIPPED, session.UserLanguage), MsgMessageType.Middle);
            return;
        }

        if (!mateEntity.IsAlive())
        {
            return;
        }

        if (!mateEntity.IsSpCooldownElapsed())
        {
            session.SendMsg(_gameLanguage.GetLanguageFormat(GameDialogKey.PARTNER_SHOUTMESSAGE_SP_IN_COOLDOWN, session.UserLanguage, mateEntity.GetSpCooldown()), MsgMessageType.Middle);
            return;
        }

        if (mateEntity.IsSitting)
        {
            await mateEntity.Owner.Session.EmitEventAsync(new MateRestEvent
            {
                MateEntity = mateEntity,
                Force = true
            });
        }

        if (packet.Type == 0)
        {
            if (!mateEntity.IsUsingSp)
            {
                DateTime time = await _delayManager.RegisterAction(mateEntity, DelayedActionType.PartnerWearSp);
                session.SendMateDelay(mateEntity, time.GetTotalMillisecondUntilNow(), GuriType.Transforming, "#psl^1");
                session.Broadcast(mateEntity.GenerateMateDance(), new RangeBroadcast(mateEntity.PositionX, mateEntity.PositionY));
                return;
            }

            await session.EmitEventAsync(new MateSpUntransformEvent
            {
                MateEntity = mateEntity
            });
            return;
        }

        bool canPerformAction = await _delayManager.CanPerformAction(mateEntity, DelayedActionType.PartnerWearSp);
        if (!canPerformAction)
        {
            return;
        }

        await _delayManager.CompleteAction(session.PlayerEntity, DelayedActionType.PartnerWearSp);

        await session.EmitEventAsync(new MateSpTransformEvent
        {
            MateEntity = mateEntity
        });
    }
}