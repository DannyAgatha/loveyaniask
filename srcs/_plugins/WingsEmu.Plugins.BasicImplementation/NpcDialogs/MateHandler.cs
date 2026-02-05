using System;
using System.Threading.Tasks;
using WingsEmu.Game._enum;
using WingsEmu.Game._i18n;
using WingsEmu.Game._NpcDialog;
using WingsEmu.Game._NpcDialog.Event;
using WingsEmu.Game.Battle;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Extensions.Mates;
using WingsEmu.Game.Managers;
using WingsEmu.Game.Mates;
using WingsEmu.Game.Mates.Events;
using WingsEmu.Game.Networking;
using WingsEmu.Packets.Enums;
using WingsEmu.Packets.Enums.Chat;

namespace NosEmu.Plugins.BasicImplementations.NpcDialogs;

public class MateHandler : INpcDialogAsyncHandler
{
    private readonly IDelayManager _delayManager;
    private readonly IGameLanguageService _gameLanguage;

    public MateHandler(IGameLanguageService gameLanguage, IDelayManager delayManager)
    {
        _gameLanguage = gameLanguage;
        _delayManager = delayManager;
    }

    public NpcRunType[] NpcRunTypes => new[] { NpcRunType.MATE };

    public async Task Execute(IClientSession session, NpcDialogEvent e)
    {
        IMateEntity mateEntity = session.PlayerEntity.MateComponent.GetMate(m => m.Id == e.NpcId);

        if (mateEntity == null)
        {
            return;
        }

        if (!mateEntity.IsAlive())
        {
            return;
        }

        switch ((MateNrunType)e.Argument)
        {
            case MateNrunType.CompanyOrSendBack:
                if (session.PlayerEntity.Miniland.Id != session.PlayerEntity.MapInstance.Id)
                {
                    return;
                }

                await session.EmitEventAsync(new MateJoinInMinilandEvent { MateEntity = mateEntity });
                break;
            case MateNrunType.Stay:
                if (session.PlayerEntity.Miniland.Id != session.PlayerEntity.MapInstance.Id)
                {
                    return;
                }
                session.SendEmptyMateSkillPacket();
                await session.EmitEventAsync(new MateStayInsideMinilandEvent { MateEntity = mateEntity });
                break;
            case MateNrunType.KickPetFromAnywhere:
                if (session.PlayerEntity.Miniland.Id != session.PlayerEntity.MapInstance.Id)
                {
                    session.SendQnaPacket($"n_run 4 5 3 {mateEntity.Id}", _gameLanguage.GetLanguage(GameDialogKey.PET_DIALOG_ASK_SEND_BACK, session.UserLanguage));
                    return;
                }
                session.SendEmptyMateSkillPacket();
                await session.EmitEventAsync(new MateStayInsideMinilandEvent { MateEntity = mateEntity });
                break;
            case MateNrunType.TriggerPetKick:
                DateTime waitUntil = await _delayManager.RegisterAction(session.PlayerEntity, DelayedActionType.KickPet);
                session.SendDelay((int)(waitUntil - DateTime.UtcNow).TotalMilliseconds, GuriType.SendBack, $"n_run 4 6 3 {mateEntity.Id}");
                break;
            case MateNrunType.KickPet:
                bool canKickPet = await _delayManager.CanPerformAction(session.PlayerEntity, DelayedActionType.KickPet);
                if (session.PlayerEntity.Miniland.Id == session.PlayerEntity.MapInstance.Id)
                {
                    return;
                }

                if (!canKickPet)
                {
                    return;
                }

                await _delayManager.CompleteAction(session.PlayerEntity, DelayedActionType.KickPet);
                string mateName = string.IsNullOrEmpty(mateEntity.MateName) || mateEntity.MateName == mateEntity.Name
                    ? _gameLanguage.GetLanguage(GameDataType.NpcMonster, mateEntity.Name, session.UserLanguage)
                    : mateEntity.MateName;
                await session.EmitEventAsync(new MateLeaveTeamEvent { MateEntity = mateEntity });

                GameDialogKey key = mateEntity.MateType == MateType.Pet ? GameDialogKey.PET_MESSAGE_KICKED : GameDialogKey.PARTNER_MESSAGE_KICKED;

                session.SendChatMessage(_gameLanguage.GetLanguageFormat(key, session.UserLanguage, mateName), ChatMessageColorType.Red);
                session.SendMsg(_gameLanguage.GetLanguageFormat(key, session.UserLanguage, mateName), MsgMessageType.Middle);
                break;
            case MateNrunType.TriggerSummon:
                if (!mateEntity.IsSummonable)
                {
                    return;
                }

                if (!mateEntity.IsAlive())
                {
                    return;
                }

                if (e.Sender.PlayerEntity.MateComponent.GetMate(s => s.IsTeamMember && s.MateType == mateEntity.MateType) != null)
                {
                    e.Sender.SendChatMessage(_gameLanguage.GetLanguage(GameDialogKey.PET_MESSAGE_ALREADY_IN_TEAM, e.Sender.UserLanguage), ChatMessageColorType.Red);
                    e.Sender.SendMsg(_gameLanguage.GetLanguage(GameDialogKey.PET_MESSAGE_ALREADY_IN_TEAM, e.Sender.UserLanguage), MsgMessageType.Middle);
                    return;
                }

                waitUntil = await _delayManager.RegisterAction(e.Sender.PlayerEntity, DelayedActionType.SummonPet);
                e.Sender.SendDelay((int)(waitUntil - DateTime.UtcNow).TotalMilliseconds, GuriType.Summon, $"n_run 4 9 3 {mateEntity.Id}");
                break;

            case MateNrunType.Summon:
                bool canSummonPet = await _delayManager.CanPerformAction(e.Sender.PlayerEntity, DelayedActionType.SummonPet);
                if (!canSummonPet)
                {
                    return;
                }

                await _delayManager.CompleteAction(e.Sender.PlayerEntity, DelayedActionType.SummonPet);
                await session.EmitEventAsync(new MateSummonEvent
                {
                    MateEntity = mateEntity
                });
                break;
        }
    }
}