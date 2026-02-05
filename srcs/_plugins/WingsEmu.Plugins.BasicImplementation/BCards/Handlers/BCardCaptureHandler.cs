// NosEmu
// 


using WingsEmu.Game._i18n;
using WingsEmu.Game.Buffs;
using WingsEmu.Game.Characters;
using WingsEmu.Game.Characters.Events;
using WingsEmu.Game.Entities;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Maps;
using WingsEmu.Game.Networking;
using WingsEmu.Packets.Enums;
using WingsEmu.Packets.Enums.Battle;
using WingsEmu.Packets.Enums.Chat;

namespace NosEmu.Plugins.BasicImplementations.BCards.Handlers;

public class BCardCaptureHandler : IBCardEffectAsyncHandler
{
    private readonly IGameLanguageService _gameLanguage;

    public BCardCaptureHandler(IGameLanguageService gameLanguage) => _gameLanguage = gameLanguage;
    public BCardType HandledType => BCardType.Capture;

    public async void Execute(IBCardEffectContext ctx)
    {
        if (ctx.Sender is not IPlayerEntity playerEntity)
        {
            return;
        }

        IClientSession session = playerEntity.Session;
        
        if (ctx.BCard.SubType != 11)
        {
            return;
        }

        if (playerEntity.MapInstance.MapInstanceType == MapInstanceType.RaidInstance)
        {
            session.SendMsg(_gameLanguage.GetLanguage(GameDialogKey.SKILL_SHOUTMESSAGE_CAPTURE_IN_RAID, session.UserLanguage), MsgMessageType.Middle);
            session.SendCancelPacket(CancelType.NotInCombatMode);
            return;
        }

        if (ctx.Target is not IMonsterEntity monsterToCapture)
        {
            session.SendMsg(_gameLanguage.GetLanguage(GameDialogKey.SKILL_SHOUTMESSAGE_CAPTURE_IMPOSSIBLE, session.UserLanguage), MsgMessageType.Middle);
            session.SendCancelPacket(CancelType.NotInCombatMode);
            return;
        }

        if (monsterToCapture.Level > playerEntity.Level)
        {
            session.SendMsg(_gameLanguage.GetLanguage(GameDialogKey.SKILL_SHOUTMESSAGE_MONSTER_LEVEL_MUST_BE_LOWER_THAN_YOURS, session.UserLanguage), MsgMessageType.Middle);
            session.SendCancelPacket(CancelType.NotInCombatMode);
            return;
        }

        if (monsterToCapture.GetHpPercentage() >= 50)
        {
            session.SendMsg(_gameLanguage.GetLanguage(GameDialogKey.SKILL_SHOUTMESSAGE_MONSTER_MUST_BE_LOW_HP, session.UserLanguage), MsgMessageType.Middle);
            session.SendCancelPacket(CancelType.NotInCombatMode);
            return;
        }

        if (playerEntity.MaxPetCount <= playerEntity.MateComponent.GetMates(x => x.MateType == MateType.Pet).Count)
        {
            session.SendMsg(_gameLanguage.GetLanguage(GameDialogKey.INFORMATION_SHOUTMESSAGE_MAX_PET_COUNT, session.UserLanguage), MsgMessageType.Middle);
            session.SendCancelPacket(CancelType.NotInCombatMode);
            return;
        }

        if (!monsterToCapture.CanBeCaught)
        {
            session.SendMsg(_gameLanguage.GetLanguage(GameDialogKey.SKILL_SHOUTMESSAGE_CAPTURE_IMPOSSIBLE, session.UserLanguage), MsgMessageType.Middle);
            session.SendCancelPacket(CancelType.NotInCombatMode);
            return;
        }

        if (playerEntity.GetDignityIco() > 3)
        {
            session.SendMsg(session.GetLanguage(GameDialogKey.SKILL_SHOUTMESSAGE_CAPTURE_DIGNITY_LOW), MsgMessageType.Middle);
            session.SendCancelPacket(CancelType.NotInCombatMode);
            return;
        }

        await session.EmitEventAsync(new MonsterCaptureEvent(monsterToCapture, true, ctx.Skill));
    }
}