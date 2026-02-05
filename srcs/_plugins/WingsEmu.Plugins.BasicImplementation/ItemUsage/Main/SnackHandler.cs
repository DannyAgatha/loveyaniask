using System;
using System.Linq;
using System.Threading.Tasks;
using PhoenixLib.Logging;
using WingsAPI.Game.Extensions.ItemExtension.Inventory;
using WingsAPI.Packets.Enums.Chat;
using WingsEmu.DTOs.BCards;
using WingsEmu.Game._enum;
using WingsEmu.Game._i18n;
using WingsEmu.Game._ItemUsage;
using WingsEmu.Game._ItemUsage.Event;
using WingsEmu.Game.Battle;
using WingsEmu.Game.Buffs;
using WingsEmu.Game.Characters;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Items;
using WingsEmu.Game.Networking;
using WingsEmu.Game.SnackFood;
using WingsEmu.Packets.Enums;
using WingsEmu.Packets.Enums.Character;
using WingsEmu.Packets.Enums.Chat;

namespace NosEmu.Plugins.BasicImplementations.ItemUsage.Main;

public class SnackHandler : IItemHandler
{
    private readonly IBCardEffectHandlerContainer _bCardEffectHandlerContainer;
    private readonly IBuffFactory _buff;
    private readonly SnackFoodConfiguration _configuration;
    private readonly IGameLanguageService _gameLanguage;

    public SnackHandler(IGameLanguageService gameLanguage, SnackFoodConfiguration configuration, IBuffFactory buff, IBCardEffectHandlerContainer bCardEffectHandlerContainer)
    {
        _gameLanguage = gameLanguage;
        _configuration = configuration;
        _buff = buff;
        _bCardEffectHandlerContainer = bCardEffectHandlerContainer;
    }

    public ItemType ItemType => ItemType.Snack;
    public long[] Effects => [0];

    public async Task HandleAsync(IClientSession session, InventoryUseItemEvent e)
    {
        IPlayerEntity character = session.PlayerEntity;
        IGameItem gameItem = e.Item.ItemInstance.GameItem;
        DateTime now = DateTime.UtcNow;

        if (character.RainbowBattleComponent.IsInRainbowBattle)
        {
            session.SendChatMessage(session.GetLanguage(GameDialogKey.ITEM_MESSAGE_CANT_USE), ChatMessageColorType.Yellow);
            return;
        }
        
        if (character.BCardComponent.HasBCard(BCardType.TimeCircleSkills, (byte)AdditionalTypes.TimeCircleSkills.ItemCannotBeUsed))
        {
            session.SendChatMessage(session.GetLanguage(GameDialogKey.ITEM_MESSAGE_CANT_USE), ChatMessageColorType.Yellow);
            return;
        }
        
        if (character.BCardComponent.HasBCard(BCardType.FourthGlacernonFamilyRaid, (byte)AdditionalTypes.FourthGlacernonFamilyRaid.DisableHPMPRecovery))
        {
            session.SendMsgi(MessageType.Default, Game18NConstString.ThatIsNotCurrentlyPossible);
            session.SendSayi(ChatMessageColorType.Red, Game18NConstString.ThatIsNotCurrentlyPossible);
            return;
        }

        if (character.LastSnack > now)
        {
            Log.Debug($"{nameof(FoodHandler)} Food in cooldown (You can only use snack every {_configuration.DelayBetweenSnack})");
            return;
        }

        if (!session.PlayerEntity.IsAlive())
        {
            return;
        }

        if (gameItem.Id == (int)ItemVnums.BATTLE_POTION)
        {
            if (session.PlayerEntity.IsInRaidParty)
            {
                return;
            }

            if (session.CantPerformActionOnAct4())
            {
                return;
            }

            Buff buff = _buff.CreateBuff((int)BuffVnums.PVP, character);
            await character.AddBuffAsync(buff);
            await character.Session.RemoveItemFromInventory(item: e.Item);
            return;
        }

        bool softCapReached = character.AddSnack(gameItem);
        if (softCapReached)
        {
            string message = _gameLanguage.GetLanguage(character.Gender == GenderType.Male ? GameDialogKey.INFORMATION_MESSAGE_NOT_HUNGRY_MALE : GameDialogKey.INFORMATION_MESSAGE_NOT_HUNGRY_FEMALE,
                session.UserLanguage);
            session.SendChatMessage(message, ChatMessageColorType.PlayerSay);

            return;
        }

        if (gameItem.BCards.Any())
        {
            // Checks, if item has additional HP/MP inside item.BCards
            foreach (BCardDTO bCard in gameItem.BCards)
            {
                if (bCard.Type != (short)BCardType.HPMP)
                {
                    _bCardEffectHandlerContainer.Execute(character, character, bCard);
                    continue;
                }

                int firstDataValue = bCard.FirstDataValue(character.Level);
                int secondDataValue = bCard.SecondDataValue(character.Level);

                switch (bCard.SubType)
                {
                    case (byte)AdditionalTypes.HPMP.ReceiveAdditionalHP:
                        character.AddAdditionalSnack(character.MaxHp, firstDataValue, firstDataValue >= 0, secondDataValue);
                        break;
                    case (byte)AdditionalTypes.HPMP.ReceiveAdditionalMP:
                        character.AddAdditionalSnack(character.MaxMp, firstDataValue, firstDataValue < 0, secondDataValue);
                        break;
                }
            }
        }

        character.LastSnack = DateTime.UtcNow.AddMilliseconds(_configuration.DelayBetweenSnack);

        await session.RemoveItemFromInventory(item: e.Item);
        session.SendEffect(EffectType.Eat);
    }
}