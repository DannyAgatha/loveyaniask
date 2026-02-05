using System;
using System.Threading.Tasks;
using PhoenixLib.Events;
using WingsAPI.Game.Extensions.ItemExtension.Inventory;
using WingsAPI.Packets.Enums;
using WingsEmu.Game._i18n;
using WingsEmu.Game._ItemUsage;
using WingsEmu.Game._ItemUsage.Event;
using WingsEmu.Game.Configurations;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Managers;
using WingsEmu.Game.Networking;
using WingsEmu.Packets.Enums;
using WingsEmu.Packets.Enums.Chat;

namespace NosEmu.Plugins.BasicImplementations.ItemUsage.Main.Special;

/// <summary>
///     This handler is called for Small/Medium/Large NosMall sp point potions and Fafnir's Fried Dinner
/// </summary>
public class SpecialPotionHandler : IItemHandler
{
    /**
     * Probably better to switch to enum ?
     * Idk if it's used somewhere else
     */
    
    private const int Small = 1;
    private const int Medium = 2;
    private const int Large = 3;
    private const int Fafnir = 4;

    private readonly IAsyncEventPipeline _eventPipeline;
    private readonly IGameLanguageService _gameLanguage;
    private readonly IServerManager _serverManager;
    private readonly IEvtbConfiguration _evtbConfiguration;

    public SpecialPotionHandler(IAsyncEventPipeline eventPipeline, IGameLanguageService gameLanguage, IServerManager serverManager, IEvtbConfiguration evtbConfiguration)
    {
        _eventPipeline = eventPipeline;
        _gameLanguage = gameLanguage;
        _serverManager = serverManager;
        _evtbConfiguration = evtbConfiguration;
    }

    public ItemType ItemType => ItemType.Special;
    public long[] Effects => new long[] { 151 };

    public async Task HandleAsync(IClientSession session, InventoryUseItemEvent e)
    {
        if (session.PlayerEntity.Hp == session.PlayerEntity.MaxHp && session.PlayerEntity.SpPointsBonus == _serverManager.MaxAdditionalSpPoints)
        {
            return;
        }

        if (session.PlayerEntity.RainbowBattleComponent.IsInRainbowBattle)
        {
            session.SendChatMessage(session.GetLanguage(GameDialogKey.ITEM_MESSAGE_CANT_USE), ChatMessageColorType.Yellow);
            return;
        }

        int type = e.Item.ItemInstance.GameItem.EffectValue;

        /*
         * Unlike classic Sp point potion/classic potion there is no values specified in Item.dat (for sp point), so it's hardcoded :c
         * Actually these values was hardcoded directly at parsing, i've removed them so we can easily custom these values via a config file or something else in the future
        */

        if (type != Fafnir)
        {
            if (session.CantPerformActionOnAct4())
            {
                return;
            }
        }

        switch (type)
        {
            case Small:
                session.PlayerEntity.SpPointsBonus += (int)(30000 * (1 + _evtbConfiguration.GetValueForEventType(EvtbType.INCREASE_FULLNESS_POINTS_RECEIVED) * 0.01));
                session.PlayerEntity.Hp = session.PlayerEntity.MaxHp;
                session.SendMsg(_gameLanguage.GetLanguageFormat(GameDialogKey.SPECIALIST_SHOUTMESSAGE_POINTS_ADDED, session.UserLanguage, (int)(30000 * (1 + _evtbConfiguration.GetValueForEventType(EvtbType.INCREASE_FULLNESS_POINTS_RECEIVED) * 0.01))), MsgMessageType.Middle);
                break;
            case Medium:
                session.PlayerEntity.SpPointsBonus += (int)(70000 * (1 + _evtbConfiguration.GetValueForEventType(EvtbType.INCREASE_FULLNESS_POINTS_RECEIVED) * 0.01));
                session.PlayerEntity.Hp = session.PlayerEntity.MaxHp;
                session.PlayerEntity.Mp = session.PlayerEntity.MaxMp;
                session.SendMsg(_gameLanguage.GetLanguageFormat(GameDialogKey.SPECIALIST_SHOUTMESSAGE_POINTS_ADDED, session.UserLanguage, (int)(70000 * (1 + _evtbConfiguration.GetValueForEventType(EvtbType.INCREASE_FULLNESS_POINTS_RECEIVED) * 0.01))), MsgMessageType.Middle);
                break;
            case Large:
                session.PlayerEntity.SpPointsBonus += (int)(180000 * (1 + _evtbConfiguration.GetValueForEventType(EvtbType.INCREASE_FULLNESS_POINTS_RECEIVED) * 0.01));
                session.PlayerEntity.Hp = session.PlayerEntity.MaxHp;
                session.PlayerEntity.Mp = session.PlayerEntity.MaxMp;
                session.SendMsg(_gameLanguage.GetLanguageFormat(GameDialogKey.SPECIALIST_SHOUTMESSAGE_POINTS_ADDED, session.UserLanguage, (int)(180000 * (1 + _evtbConfiguration.GetValueForEventType(EvtbType.INCREASE_FULLNESS_POINTS_RECEIVED) * 0.01))), MsgMessageType.Middle);
                break;
            case Fafnir:
                session.PlayerEntity.SpPointsBonus += (int)(100 * (1 + _evtbConfiguration.GetValueForEventType(EvtbType.INCREASE_FULLNESS_POINTS_RECEIVED) * 0.01));
                session.PlayerEntity.Hp += session.PlayerEntity.MaxHp / 100 * 20;
                session.PlayerEntity.Mp += session.PlayerEntity.MaxMp / 100 * 20;
                session.SendMsg(_gameLanguage.GetLanguageFormat(GameDialogKey.SPECIALIST_SHOUTMESSAGE_POINTS_ADDED, session.UserLanguage, (int)(100 * (1 + _evtbConfiguration.GetValueForEventType(EvtbType.INCREASE_FULLNESS_POINTS_RECEIVED) * 0.01))), MsgMessageType.Middle);
                break;
        }

        if (session.PlayerEntity.Hp > session.PlayerEntity.MaxHp)
        {
            session.PlayerEntity.Hp = session.PlayerEntity.MaxHp;
        }

        if (session.PlayerEntity.Mp > session.PlayerEntity.MaxMp)
        {
            session.PlayerEntity.Mp = session.PlayerEntity.MaxMp;
        }

        if (session.PlayerEntity.SpPointsBonus > _serverManager.MaxAdditionalSpPoints)
        {
            session.PlayerEntity.SpPointsBonus = _serverManager.MaxAdditionalSpPoints;
        }

        session.RefreshStat();
        session.RefreshSpPoint();

        await session.RemoveItemFromInventory(item: e.Item);
    }
}