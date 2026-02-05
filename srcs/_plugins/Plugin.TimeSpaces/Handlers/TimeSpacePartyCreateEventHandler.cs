using System.Threading;
using System.Threading.Tasks;
using PhoenixLib.Events;
using WingsAPI.Game.Extensions.ItemExtension.Inventory;
using WingsAPI.Game.Extensions.ItemExtension.Item;
using WingsEmu.DTOs.Maps;
using WingsEmu.Game._enum;
using WingsEmu.Game._i18n;
using WingsEmu.Game.Configurations;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Inventory;
using WingsEmu.Game.Managers.StaticData;
using WingsEmu.Game.Networking;
using WingsEmu.Game.TimeSpaces;
using WingsEmu.Game.TimeSpaces.Events;
using WingsEmu.Packets.Enums.Chat;

namespace Plugin.TimeSpaces.Handlers;

public class TimeSpacePartyCreateEventHandler : IAsyncEventProcessor<TimeSpacePartyCreateEvent>
{
    private readonly IGameLanguageService _gameLanguage;
    private readonly IItemsManager _itemsManager;
    private readonly ISubActConfiguration _subActConfiguration;
    private readonly ITimeSpaceConfiguration _timeSpaceConfig;
    private readonly ITimeSpaceConfiguration _timeSpaceConfiguration;
    private readonly ITimeSpaceManager _timeSpaceManager;

    public TimeSpacePartyCreateEventHandler(ITimeSpaceManager timeSpaceManager, IGameLanguageService gameLanguage, ITimeSpaceConfiguration timeSpaceConfiguration, IItemsManager itemsManager,
        ISubActConfiguration subActConfiguration, ITimeSpaceConfiguration timeSpaceConfig)
    {
        _timeSpaceManager = timeSpaceManager;
        _gameLanguage = gameLanguage;
        _timeSpaceConfiguration = timeSpaceConfiguration;
        _itemsManager = itemsManager;
        _subActConfiguration = subActConfiguration;
        _timeSpaceConfig = timeSpaceConfig;
    }

    public async Task HandleAsync(TimeSpacePartyCreateEvent e, CancellationToken cancellation)
    {
        IClientSession session = e.Sender;

        if (session.PlayerEntity.IsInRaidParty)
        {
            return;
        }

        if (session.PlayerEntity.HasShopOpened)
        {
            return;
        }

        if (session.IsMuted())
        {
            session.SendMsg(session.GetLanguage(GameDialogKey.MUTE_SHOUTMESSAGE_YOU_ARE_MUTED), MsgMessageType.Middle);
            return;
        }

        if (!session.PlayerEntity.MapInstance.HasMapFlag(MapFlags.IS_BASE_MAP))
        {
            session.SendMsg(_gameLanguage.GetLanguage(GameDialogKey.INFORMATION_SHOUTMESSAGE_MUST_BE_IN_CLASSIC_MAP, session.UserLanguage), MsgMessageType.Middle);
            return;
        }

        TimeSpaceFileConfiguration timeSpace = _timeSpaceConfiguration.GetTimeSpaceConfiguration(e.TimeSpaceId);
        if (timeSpace == null)
        {
            return;
        }

        if (!session.CanJoinToTimeSpace(e.TimeSpaceId, _subActConfiguration, _timeSpaceConfig) && !session.IsGameMaster() && !timeSpace.IsSpecial && !timeSpace.IsHidden)
        {
            session.SendChatMessage(session.GetLanguage(GameDialogKey.TIMESPACE_SHOUTMESSAGE_WRONG_ACT), ChatMessageColorType.Red);
            session.SendMsg(session.GetLanguage(GameDialogKey.TIMESPACE_SHOUTMESSAGE_WRONG_ACT), MsgMessageType.Middle);
            return;
        }

        if (session.PlayerEntity.Level < timeSpace.MinLevel)
        {
            session.SendChatMessage(session.GetLanguage(GameDialogKey.TIMESPACE_CHATMESSAGE_LOW_LEVEL), ChatMessageColorType.Red);
            session.SendMsg(session.GetLanguage(GameDialogKey.TIMESPACE_CHATMESSAGE_LOW_LEVEL), MsgMessageType.Middle);
            return;
        }

        if (session.PlayerEntity.Level > timeSpace.MaxLevel)
        {
            session.SendChatMessage(session.GetLanguage(GameDialogKey.TIMESPACE_CHATMESSAGE_HIGH_LEVEL), ChatMessageColorType.PlayerSay);
            session.SendMsg(session.GetLanguage(GameDialogKey.TIMESPACE_CHATMESSAGE_HIGH_LEVEL), MsgMessageType.Middle);
            return;
        }

        if (timeSpace.SeedsOfPowerRequired != 0)
        {
            if (!session.PlayerEntity.HasItem((short)ItemVnums.SEED_OF_POWER, timeSpace.SeedsOfPowerRequired))
            {
                string itemName = _itemsManager.GetItem((short)ItemVnums.SEED_OF_POWER).GetItemName(_gameLanguage, session.UserLanguage);
                session.SendChatMessage(session.GetLanguageFormat(GameDialogKey.INVENTORY_SHOUTMESSAGE_NOT_ENOUGH_ITEMS, timeSpace.SeedsOfPowerRequired, itemName), ChatMessageColorType.Red);
                session.SendMsg(session.GetLanguageFormat(GameDialogKey.INVENTORY_SHOUTMESSAGE_NOT_ENOUGH_ITEMS, timeSpace.SeedsOfPowerRequired, itemName), MsgMessageType.Middle);
                return;
            }
        }

        if (e.IsChallengeMode)
        {
            if (session.IsActionForbidden())
            {
                return;
            }

            if (!session.PlayerEntity.RemoveGold(timeSpace.MinLevel * 50))
            {
                session.SendChatMessage(session.GetLanguage(GameDialogKey.INTERACTION_MESSAGE_NOT_ENOUGH_GOLD), ChatMessageColorType.Red);
                session.SendMsg(session.GetLanguage(GameDialogKey.INTERACTION_MESSAGE_NOT_ENOUGH_GOLD), MsgMessageType.Middle);
                return;
            }
        }

        if (timeSpace.SeedsOfPowerRequired != 0)
        {
            await session.RemoveItemFromInventory((short)ItemVnums.SEED_OF_POWER, timeSpace.SeedsOfPowerRequired);
        }

        var timeSpaceParty = new TimeSpaceParty(timeSpace, e.IsEasyMode, e.IsChallengeMode);

        InventoryItem itemToRemove = e.ItemToRemove;
        if (itemToRemove != null)
        {
            timeSpaceParty.ItemVnumToRemove = itemToRemove.ItemInstance.ItemVNum;
        }

        timeSpaceParty.AddMember(session);
        _timeSpaceManager.AddTimeSpace(timeSpaceParty);
        session.PlayerEntity.TimeSpaceComponent.SetTimeSpaceParty(timeSpaceParty);
    }
}