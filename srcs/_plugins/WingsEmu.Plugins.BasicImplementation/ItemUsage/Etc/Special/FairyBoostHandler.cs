// NosEmu
// 


using System;
using System.Threading.Tasks;
using PhoenixLib.Events;
using WingsAPI.Game.Extensions.ItemExtension.Inventory;
using WingsAPI.Packets.Enums.Chat;
using WingsEmu.Game._enum;
using WingsEmu.Game._i18n;
using WingsEmu.Game._ItemUsage;
using WingsEmu.Game._ItemUsage.Event;
using WingsEmu.Game.Battle;
using WingsEmu.Game.Buffs;
using WingsEmu.Game.Entities.Extensions;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Inventory;
using WingsEmu.Game.Networking;
using WingsEmu.Packets.Enums;
using WingsEmu.Packets.Enums.Chat;

namespace NosEmu.Plugins.BasicImplementations.ItemUsage.Etc.Special;

public class FairyBoostHandler : IItemHandler
{
    private const int CARD_ID = 131;
    private const int MAX_HOURS = 12; 
    private readonly IBuffFactory _buffFactory;
    private readonly IGameLanguageService _gameLanguage;

    public FairyBoostHandler(IGameLanguageService gameLanguage, IBuffFactory buffFactory)
    {
        _gameLanguage = gameLanguage;
        _buffFactory = buffFactory;
    }

    public ItemType ItemType => ItemType.Special;
    public long[] Effects => [250];

    public async Task HandleAsync(IClientSession session, InventoryUseItemEvent e)
    {
        InventoryItem inv = e.Item;
        var duration = TimeSpan.FromHours(1);
            
        if (session.PlayerEntity.Fairy == null)
        {
            session.SendMsgi(MessageType.Default, Game18NConstString.NoFairyWithYou);
            return;
        }
            
        if (session.PlayerEntity.HasBuff(CARD_ID))
        {
            Buff existingBuff = session.PlayerEntity.BuffComponent.GetBuff(CARD_ID);
                
            double remainingMinutes = (existingBuff.Start - DateTime.UtcNow + existingBuff.Duration).TotalMinutes;
            var newDuration = TimeSpan.FromMinutes(duration.TotalMinutes + remainingMinutes);
                
            if (newDuration.TotalHours >= MAX_HOURS)
            {
                string buffNameLimit = _gameLanguage.GetLanguage(GameDataType.Card, existingBuff.Name, session.UserLanguage);
                session.SendPacket(session.PlayerEntity.GenerateSayPacket(_gameLanguage.GetLanguageFormat(GameDialogKey.ITEM_CHATMESSAGE_BUFF_LIMIT_REACHED, session.UserLanguage, buffNameLimit),
                    ChatMessageColorType.Red));
                return;
            }
            session.PlayerEntity.BuffComponent.GetBuff(CARD_ID).Duration = TimeSpan.FromMinutes(newDuration.TotalMinutes);
            session.SendStaticBuffUiPacket(existingBuff, existingBuff.RemainingTimeInMilliseconds());

            string buffName = _gameLanguage.GetLanguage(GameDataType.Card, existingBuff.Name, session.UserLanguage);
            session.SendPacket(session.PlayerEntity.GenerateSayPacket(_gameLanguage.GetLanguageFormat(GameDialogKey.ITEM_CHATMESSAGE_BUFF_EXTENDED, session.UserLanguage, buffName),
                ChatMessageColorType.Yellow));

            await session.PlayerEntity.RemoveBuffAsync(CARD_ID);
            await session.PlayerEntity.AddBuffAsync(_buffFactory.CreateBuff(CARD_ID, session.PlayerEntity, newDuration, BuffFlag.BIG_AND_KEEP_ON_LOGOUT));
        }
        else
        {
            await session.PlayerEntity.AddBuffAsync(_buffFactory.CreateOneHourBuff(session.PlayerEntity, CARD_ID, BuffFlag.BIG_AND_KEEP_ON_LOGOUT));
        }

        await session.RemoveItemFromInventory(inv.ItemInstance.ItemVNum);

        session.BroadcastPairy();
        session.RefreshFairy();
    }
}