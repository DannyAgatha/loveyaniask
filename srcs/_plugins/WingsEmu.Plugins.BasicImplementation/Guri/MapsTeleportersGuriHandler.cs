using System.Linq;
using System.Threading.Tasks;
using WingsAPI.Game.Extensions.ItemExtension.Inventory;
using WingsAPI.Game.Extensions.ItemExtension.Item;
using WingsEmu.DTOs.ServerDatas;
using WingsEmu.Game._Guri;
using WingsEmu.Game._Guri.Event;
using WingsEmu.Game._i18n;
using WingsEmu.Game.Battle;
using WingsEmu.Game.Entities;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Managers;
using WingsEmu.Game.Managers.StaticData;
using WingsEmu.Game.Networking;
using WingsEmu.Packets.Enums;
using WingsEmu.Packets.Enums.Chat;

namespace NosEmu.Plugins.BasicImplementations.Guri;

public class MapsTeleportersGuriHandler : IGuriHandler
{
    private readonly IDelayManager _delayManager;
    private readonly IGameLanguageService _gameLanguageService;
    private readonly IItemsManager _itemsManager;
    private readonly ITeleporterManager _teleporterManager;

    public MapsTeleportersGuriHandler(IDelayManager delayManager, ITeleporterManager teleporterManager, IGameLanguageService gameLanguageService, IItemsManager itemsManager)
    {
        _delayManager = delayManager;
        _teleporterManager = teleporterManager;
        _gameLanguageService = gameLanguageService;
        _itemsManager = itemsManager;
    }

    public long GuriEffectId => 710;

    public async Task ExecuteAsync(IClientSession session, GuriEvent guriPacket)
    {
        if (guriPacket.User == null)
        {
            return;
        }

        if (guriPacket.Packet.Length < 6)
        {
            return;
        }

        if (!int.TryParse(guriPacket.Packet[5], out int npcId))
        {
            return;
        }

        INpcEntity npcEntity = session.CurrentMapInstance.GetNpcById(npcId);
        if (npcEntity == null)
        {
            return;
        }

        if (npcEntity.VNumRequired != 0 && npcEntity.AmountRequired != 0)
        {
            if (!session.PlayerEntity.HasItem(npcEntity.VNumRequired, npcEntity.AmountRequired))
            {
                string itemName = _itemsManager.GetItem(npcEntity.VNumRequired).GetItemName(_gameLanguageService, session.UserLanguage);
                session.SendMsg(_gameLanguageService.GetLanguageFormat(GameDialogKey.INVENTORY_SHOUTMESSAGE_NOT_ENOUGH_ITEMS, session.UserLanguage, npcEntity.AmountRequired, itemName),
                    MsgMessageType.Middle);
                return;
            }
        }

        TeleporterDTO tp = _teleporterManager.GetTeleportByNpcId(npcEntity.Id)?.FirstOrDefault(t => t?.Type == TeleporterType.TELEPORT_ON_MAP);
        if (tp == null)
        {
            return;
        }

        if (tp.MapX != guriPacket.Data || tp.MapY != guriPacket.User.Value)
        {
            return;
        }

        bool canTeleport = await _delayManager.CanPerformAction(session.PlayerEntity, DelayedActionType.UseTeleporter);
        if (!canTeleport)
        {
            return;
        }

        await _delayManager.CompleteAction(session.PlayerEntity, DelayedActionType.UseTeleporter);

        if (npcEntity.VNumRequired != 0 && npcEntity.TeleportRemoveFromInventory)
        {
            await session.RemoveItemFromInventory(npcEntity.VNumRequired, npcEntity.AmountRequired);
        }

        session.PlayerEntity.TeleportOnMap((short)guriPacket.Data, (short)guriPacket.User.Value, true);
    }
}