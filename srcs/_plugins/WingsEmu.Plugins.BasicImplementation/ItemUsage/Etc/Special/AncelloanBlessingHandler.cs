using System;
using System.Threading.Tasks;
using WingsAPI.Game.Extensions.ItemExtension.Inventory;
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

public class AncelloanBlessingHandler : IItemHandler
{
    private const int CARD_ID = 121;
    private const int MAX_HOURS = 12; 
    private readonly IBuffFactory _buffFactory;
    private readonly IGameLanguageService _gameLanguage;

    public AncelloanBlessingHandler(IGameLanguageService gameLanguage, IBuffFactory buffFactory)
    {
        _gameLanguage = gameLanguage;
        _buffFactory = buffFactory;
    }

    public ItemType ItemType => ItemType.Special;
    public long[] Effects => [208];

    public async Task HandleAsync(IClientSession session, InventoryUseItemEvent e)
    {
        InventoryItem inv = e.Item;
        
        Buff existingBuff = session.PlayerEntity.BuffComponent.GetBuff(CARD_ID);
        var duration = TimeSpan.FromHours(1);
        
        if (existingBuff != null)
        {
            double minutes = (existingBuff.Start - DateTime.UtcNow + existingBuff.Duration).TotalMinutes;
            var updatedDuration = TimeSpan.FromMinutes(duration.TotalMinutes + minutes);
            
            if (updatedDuration.TotalHours >= MAX_HOURS)
            {
                string buffName = _gameLanguage.GetLanguage(GameDataType.Card, existingBuff.Name, session.UserLanguage);
                session.SendPacket(session.PlayerEntity.GenerateSayPacket(
                    _gameLanguage.GetLanguageFormat(GameDialogKey.ITEM_CHATMESSAGE_BUFF_LIMIT_REACHED, session.UserLanguage, buffName),
                    ChatMessageColorType.Yellow));
                return;
            }
            
            existingBuff.Duration = updatedDuration;
            session.SendStaticBuffUiPacket(existingBuff, existingBuff.RemainingTimeInMilliseconds());
            
            string extendedBuffName = _gameLanguage.GetLanguage(GameDataType.Card, existingBuff.Name, session.UserLanguage);
            session.SendPacket(session.PlayerEntity.GenerateSayPacket(
                _gameLanguage.GetLanguageFormat(GameDialogKey.ITEM_CHATMESSAGE_BUFF_EXTENDED, session.UserLanguage, extendedBuffName),
                ChatMessageColorType.Yellow));
        }
        else
        {
            await session.PlayerEntity.AddBuffAsync(_buffFactory.CreateOneHourBuff(session.PlayerEntity, CARD_ID, BuffFlag.BIG_AND_KEEP_ON_LOGOUT));
        }
        
        await session.RemoveItemFromInventory(inv.ItemInstance.ItemVNum);
    }
}
