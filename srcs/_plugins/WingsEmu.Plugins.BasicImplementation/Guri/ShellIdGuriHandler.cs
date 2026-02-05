using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WingsAPI.Game.Extensions.ItemExtension.Inventory;
using WingsAPI.Packets.Enums.Shells;
using WingsEmu.DTOs.Items;
using WingsEmu.Game._enum;
using WingsEmu.Game._Guri;
using WingsEmu.Game._Guri.Event;
using WingsEmu.Game._i18n;
using WingsEmu.Game.Algorithm;
using WingsEmu.Game.Characters.Events;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Inventory;
using WingsEmu.Game.Items;
using WingsEmu.Game.Networking;
using WingsEmu.Packets.Enums;
using WingsEmu.Packets.Enums.Chat;

namespace NosEmu.Plugins.BasicImplementations.Guri;

public class ShellIdGuriHandler : IGuriHandler
{
    private readonly IGameLanguageService _lang;
    private readonly IShellGenerationAlgorithm _shellGenerationAlgorithm;

    public ShellIdGuriHandler(IGameLanguageService gameLanguageService, IShellGenerationAlgorithm shellGenerationAlgorithm)
    {
        _lang = gameLanguageService;
        _shellGenerationAlgorithm = shellGenerationAlgorithm;
    }

    public long GuriEffectId => 204;

    public async Task ExecuteAsync(IClientSession session, GuriEvent guriPacket)
    {
        if (guriPacket.User == null)
        {
            // WRONG PACKET
            return;
        }

        var inventoryType = (InventoryType)guriPacket.Data;
        InventoryItem shell = session.PlayerEntity.GetItemBySlotAndType((short)guriPacket.User.Value, inventoryType);

        if (shell == null)
        {
            return;
        }

        if (shell.ItemInstance.Type != ItemInstanceType.WearableInstance)
        {
            return;
        }

        GameItemInstance shellItem = shell.ItemInstance;

        if (!session.PlayerEntity.HasItem((short)ItemVnums.RAINBOW_PEARL))
        {
            return;
        }

        if (shellItem.EquipmentOptions != null && shellItem.EquipmentOptions.Any())
        {
            // ALREADY IDENTIFIED
            session.SendMsg(_lang.GetLanguage(GameDialogKey.SHELLS_SHOUTMESSAGE_ALREADY_IDENTIFIED, session.UserLanguage), MsgMessageType.Middle);
            return;
        }

        ShellType shellType = shellItem.GameItem.ShellType;
        short perlsNeeded = (short)(shellItem.Upgrade / 10 + shellItem.Rarity);

        if (!session.PlayerEntity.HasItem((short)ItemVnums.RAINBOW_PEARL, perlsNeeded))
        {
            // NOT ENOUGH PEARLS
            return;
        }

        IEnumerable<EquipmentOptionDTO> shellOptions = _shellGenerationAlgorithm.GenerateShell((byte)shellType, shellItem.Rarity, shellItem.Upgrade).ToList();
        if (!shellOptions.Any())
        {
            session.SendInfo(_lang.GetLanguage(GameDialogKey.SHELLS_INFO_SHELL_CANT_BE_IDENTIFIED, session.UserLanguage));
            return;
        }

        shellItem.EquipmentOptions ??= new List<EquipmentOptionDTO>();
        shellItem.EquipmentOptions.AddRange(shellOptions);
        session.SendMsg(_lang.GetLanguage(GameDialogKey.SHELLS_SHOUTMESSAGE_IDENTIFIED, session.UserLanguage), MsgMessageType.Middle);
        session.BroadcastEffect(EffectType.UpgradeSuccess);
        await session.RemoveItemFromInventory((short)ItemVnums.RAINBOW_PEARL, perlsNeeded);
        await session.EmitEventAsync(new ShellIdentifiedEvent
        {
            Shell = shellItem
        });
    }
}