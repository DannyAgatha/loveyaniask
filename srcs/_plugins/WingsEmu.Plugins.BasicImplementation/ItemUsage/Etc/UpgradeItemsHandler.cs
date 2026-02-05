// NosEmu
// 


using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PhoenixLib.Logging;
using WingsAPI.Game.Extensions.ItemExtension.Inventory;
using WingsEmu.DTOs.Items;
using WingsEmu.Game._enum;
using WingsEmu.Game._i18n;
using WingsEmu.Game._ItemUsage;
using WingsEmu.Game._ItemUsage.Event;
using WingsEmu.Game.Characters.Events;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Inventory;
using WingsEmu.Game.Managers;
using WingsEmu.Game.Networking;
using WingsEmu.Packets.Enums;
using WingsEmu.Packets.Enums.Chat;

namespace NosEmu.Plugins.BasicImplementations.ItemUsage.Etc;

public class UpgradeItemsHandler : IItemHandler
{
    private readonly IGameLanguageService _gameLanguage;
    private readonly IServerManager _serverManager;

    public UpgradeItemsHandler(IGameLanguageService gameLanguage, IServerManager serverManager)
    {
        _gameLanguage = gameLanguage;
        _serverManager = serverManager;
    }

    private int[] _scrolls => [26, 27, 28, 47, 50, 60, 61, 75, 76, 77, 79, 83, 84, 86, 89, 90, 93, 100, 107];
    
    private static IEnumerable<int> PremiumScrolls => new[] 
    { 
        (int)ItemVnums.LOWER_SP_PROTECTION_SCROLL_PREMIUM, 
        (int)ItemVnums.LOWER_SP_PROTECTION_SCROLL_PREMIUM_LIMITED,
        (int)ItemVnums.HIGHER_SP_PROTECTION_SCROLL_PREMIUM,
        (int)ItemVnums.HIGHER_SP_PROTECTION_SCROLL_PREMIUM_LIMITED,
        (int)ItemVnums.DRAGON_SP_PROTECTION_SCROLL_PREMIUM,
        (int)ItemVnums.DRAGON_SP_PROTECTION_SCROLL_PREMIUM_LIMITED,
    };

    public ItemType ItemType => ItemType.Upgrade;
    public long[] Effects => [0];

    public async Task HandleAsync(IClientSession session, InventoryUseItemEvent e)
    {
        string[] packetsplit = e.Packet;
        InventoryItem inv = e.Item;
        int effectValue = e.Item.ItemInstance.GameItem.EffectValue;
        bool isPremium = PremiumScrolls.Contains(e.Item.ItemInstance.GameItem.Id);
        
        // If it's a scroll
        if (_scrolls.Contains(effectValue))
        {
            session.SendGuriPacket(12, value: effectValue, secondValue: isPremium ? 2 : 0);
            return;
        }

        if (packetsplit == null || packetsplit.Length <= 9)
        {
            return;
        }

        if (!byte.TryParse(packetsplit[8], out byte typeEquip) ||
            !short.TryParse(packetsplit[9], out short slotEquip))
        {
            return;
        }

        if (session.PlayerEntity.IsSitting)
        {
            await session.EmitEventAsync(new PlayerRestEvent
            {
                RestTeamMemberMates = false
            });
        }


        switch (inv.ItemInstance.ItemVNum)
        {
            case 1219:
                InventoryItem equip = session.PlayerEntity.GetItemBySlotAndType(slotEquip, (InventoryType)typeEquip);

                if (equip == null)
                {
                    return;
                }

                if (equip.ItemInstance.Type != ItemInstanceType.WearableInstance)
                {
                    return;
                }

                if (!equip.ItemInstance.IsFixed)
                {
                    session.SendChatMessage(_gameLanguage.GetLanguage(GameDialogKey.GAMBLING_CHATMESSAGE_ITEM_IS_FIXED, session.UserLanguage), ChatMessageColorType.Red);
                    return;
                }

                await session.RemoveItemFromInventory(inv.ItemInstance.ItemVNum);
                equip.ItemInstance.IsFixed = false;
                session.SendPacket(session.PlayerEntity.GenerateEffectPacket(3003));
                session.SendGuriPacket(17, 1, slotEquip);
                session.SendChatMessage(_gameLanguage.GetLanguage(GameDialogKey.ITEM_CHATMESSAGE_UNFIXED, session.UserLanguage), ChatMessageColorType.Green);

                break;

            case 1365:
            case 9039:
                InventoryItem specialist = session.PlayerEntity.GetItemBySlotAndType(slotEquip, (InventoryType)typeEquip);
                if (specialist == null)
                {
                    Log.Debug("Not a SP selected.");
                    return;
                }

                if (specialist.ItemInstance.Type != ItemInstanceType.SpecialistInstance)
                {
                    return;
                }

                if (specialist.ItemInstance.Rarity != -2)
                {
                    Log.Debug("SP is not destroyed.");
                    return;
                }

                specialist.ItemInstance.Rarity = 0;
                session.SendMsg(_gameLanguage.GetLanguage(GameDialogKey.SPECIALIST_SHOUTMESSAGE_RESURRECTED, session.UserLanguage), MsgMessageType.Middle);
                session.SendGuriPacket(13, 1, 1);

                session.PlayerEntity.SpPointsBasic = _serverManager.MaxBasicSpPoints;
                if (session.PlayerEntity.SpPointsBasic > _serverManager.MaxBasicSpPoints)
                {
                    session.PlayerEntity.SpPointsBasic = _serverManager.MaxBasicSpPoints;
                }

                await session.RemoveItemFromInventory(inv.ItemInstance.ItemVNum);
                session.RefreshSpPoint();
                session.SendInventoryAddPacket(specialist);
                break;
        }
    }
}