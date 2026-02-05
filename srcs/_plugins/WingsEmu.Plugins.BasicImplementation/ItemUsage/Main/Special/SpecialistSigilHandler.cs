using System.Threading.Tasks;
using NosEmu.Plugins.BasicImplementations.Event.Characters;
using WingsAPI.Game.Extensions.ItemExtension.Inventory;
using WingsEmu.DTOs.Items;
using WingsEmu.Game._i18n;
using WingsEmu.Game._ItemUsage;
using WingsEmu.Game._ItemUsage.Event;
using WingsEmu.Game.Characters.Events;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Inventory;
using WingsEmu.Game.Networking;
using WingsEmu.Packets.Enums;
using WingsEmu.Packets.Enums.Chat;

namespace NosEmu.Plugins.BasicImplementations.ItemUsage.Main.Special;

public class SpecialistSigilHandler : IItemHandler
{
    private readonly IGameLanguageService _gameLanguage;

    public SpecialistSigilHandler(IGameLanguageService gameLanguage) => _gameLanguage = gameLanguage;

    public ItemType ItemType => ItemType.Upgrade;
    public long[] Effects => new long[] { 10000 };

    public async Task HandleAsync(IClientSession session, InventoryUseItemEvent e)
    {
        string[] packetsplit = e.Packet;

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

        InventoryItem equip = session.PlayerEntity.GetItemBySlotAndType(slotEquip, (InventoryType)typeEquip);
        if (equip == null)
        {
            session.SendMsg(_gameLanguage.GetLanguage(GameDialogKey.ITEM_MESSAGE_CANT_USE, session.UserLanguage), MsgMessageType.Middle);
            return;
        }

        if (equip.ItemInstance.Type != ItemInstanceType.SpecialistInstance)
        {
            session.SendMsg(_gameLanguage.GetLanguage(GameDialogKey.ITEM_MESSAGE_CANT_USE, session.UserLanguage), MsgMessageType.Middle);
            return;
        }

        if (equip.ItemInstance.Upgrade >= 15)
        {
            session.SendMsg(_gameLanguage.GetLanguage(GameDialogKey.ITEM_MESSAGE_CANT_USE, session.UserLanguage), MsgMessageType.Middle);
            return;
        }

        await session.EmitEventAsync(new SpUpgradeEvent(UpgradeProtection.Protected, equip, true));
        await session.RemoveItemFromInventory(item: e.Item);
    }
}