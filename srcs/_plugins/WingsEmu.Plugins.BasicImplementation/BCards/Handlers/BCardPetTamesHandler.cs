using WingsAPI.Game.Extensions.ItemExtension.Inventory;
using WingsEmu.DTOs.BCards;
using WingsEmu.Game._enum;
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

public class BCardPetTamesHandler : IBCardEffectAsyncHandler
{
    private readonly IGameLanguageService _gameLanguage;

    public BCardPetTamesHandler(IGameLanguageService gameLanguage) => _gameLanguage = gameLanguage;

    public BCardType HandledType => BCardType.PetTames;

    public async void Execute(IBCardEffectContext ctx)
    {
        IBattleEntity sender = ctx.Sender;
        IBattleEntity target = ctx.Target;
        BCardDTO bCard = ctx.BCard;
        int firstData = ctx.BCard.FirstData;
        int secondData = ctx.BCard.SecondData;

        if (ctx.Sender is not IPlayerEntity playerEntity)
        {
            return;
        }

        IClientSession session = playerEntity.Session;

        switch (bCard.SubType)
        {
            case (byte)AdditionalTypes.PetTames.TameStarPetsWithChance:
                if (playerEntity.MapInstance.MapInstanceType == MapInstanceType.RaidInstance)
                {
                    session.SendMsg(_gameLanguage.GetLanguage(GameDialogKey.SKILL_SHOUTMESSAGE_CAPTURE_IN_RAID, session.UserLanguage), MsgMessageType.Middle);
                    session.SendCancelPacket(CancelType.NotInCombatMode);
                    return;
                }

                if (target is not IMonsterEntity monsterToCapture)
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

                if (monsterToCapture.Stars != firstData)
                {
                    session.SendMsg(_gameLanguage.GetLanguage(GameDialogKey.SKILL_SHOUTMESSAGE_CAPTURE_IMPOSSIBLE, session.UserLanguage), MsgMessageType.Middle);
                    session.SendCancelPacket(CancelType.NotInCombatMode);
                    return;
                }

                if (monsterToCapture.Stars == 2 && !session.PlayerEntity.HasItem((int)ItemVnums.LURE))
                {
                    session.SendSayi(ChatMessageColorType.Yellow, Game18NConstString.FollowingItemRequired, 2, (int)ItemVnums.LURE);
                    session.SendCancelPacket(CancelType.NotInCombatMode);
                    return;
                }

                if (playerEntity.GetDignityIco() > 3)
                {
                    session.SendMsg(session.GetLanguage(GameDialogKey.SKILL_SHOUTMESSAGE_CAPTURE_DIGNITY_LOW), MsgMessageType.Middle);
                    session.SendCancelPacket(CancelType.NotInCombatMode);
                    return;
                }
                
                if (monsterToCapture.Stars is 2)
                {
                    await session.RemoveItemFromInventory((int)ItemVnums.LURE);
                }

                await session.EmitEventAsync(new TrainerCaptureEvent(monsterToCapture, true, secondData, ctx.Skill));
                break;
        }
    }
}