// NosEmu
// 


using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Qmmands;
using WingsAPI.Game.Extensions.ItemExtension.Inventory;
using WingsAPI.Packets.Enums.Shells;
using WingsEmu.Commands.Checks;
using WingsEmu.Commands.Entities;
using WingsEmu.DTOs.Account;
using WingsEmu.DTOs.Items;
using WingsEmu.Game.Algorithm;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Inventory;
using WingsEmu.Game.Items;
using WingsEmu.Game.Networking;
using WingsEmu.Packets.Enums;
using WingsEmu.Packets.Enums.Chat;

namespace WingsEmu.Plugins.Essentials.Administrator;

[Name("Admin-ShellCheat")]
[Description("Module related to Administrator commands for shells")]
[RequireAuthority(AuthorityType.GA)]
public class AdministratorCheatModule_Shell : SaltyModuleBase
{
    private readonly IGameItemInstanceFactory _itemInstanceFactory;
    private readonly IShellGenerationAlgorithm _shellGenerationAlgorithm;

    public AdministratorCheatModule_Shell(IShellGenerationAlgorithm shellGenerationAlgorithm, IGameItemInstanceFactory itemInstanceFactory)
    {
        _shellGenerationAlgorithm = shellGenerationAlgorithm;
        _itemInstanceFactory = itemInstanceFactory;
    }

    [Command("clearshell")]
    public async Task<SaltyCommandResult> ClearShell(short slot)
    {
        InventoryItem item = Context.Player.PlayerEntity.GetItemBySlotAndType(slot, InventoryType.Equipment);
        if (item == null)
        {
            Context.Player.SendChatMessage("Wrong slot", ChatMessageColorType.Yellow);
            return new SaltyCommandResult(false);
        }

        if (item.ItemInstance.Type != ItemInstanceType.WearableInstance)
        {
            return new SaltyCommandResult(false);
        }

        item.ItemInstance.EquipmentOptions?.RemoveAll(s => s.EquipmentOptionType == EquipmentOptionType.ARMOR_SHELL || s.EquipmentOptionType == EquipmentOptionType.WEAPON_SHELL);

        Context.Player.SendChatMessage($"[SHELL_CHEAT] Cleared the shell of the item at slot {slot}", ChatMessageColorType.Yellow);
        return new SaltyCommandResult(true);
    }

    [Command("addshell")]
    public async Task<SaltyCommandResult> AddShellAsync(short slot, int shellOptionType, byte level, int value)
    {
        InventoryItem item = Context.Player.PlayerEntity.GetItemBySlotAndType(slot, InventoryType.Equipment);
        if (item == null)
        {
            Context.Player.SendChatMessage("Wrong slot", ChatMessageColorType.Yellow);
            return new SaltyCommandResult(false);
        }

        if (item.ItemInstance.Type != ItemInstanceType.WearableInstance)
        {
            return new SaltyCommandResult(false);
        }

        item.ItemInstance.EquipmentOptions ??= new List<EquipmentOptionDTO>();
        item.ItemInstance.EquipmentOptions.Add(new EquipmentOptionDTO
        {
            EquipmentOptionType = item.ItemInstance.GameItem.EquipmentSlot switch
            {
                EquipmentType.Armor => EquipmentOptionType.ARMOR_SHELL,
                EquipmentType.SecondaryWeapon => EquipmentOptionType.WEAPON_SHELL,
                EquipmentType.MainWeapon => EquipmentOptionType.WEAPON_SHELL,
                _ => EquipmentOptionType.NONE
            },
            Type = (byte)shellOptionType,
            Level = level,
            Value = value
        });
        Context.Player.SendChatMessage("Added shell row", ChatMessageColorType.Yellow);
        return new SaltyCommandResult(true);
    }

    [Command("generateShell")]
    public async Task<SaltyCommandResult> GenerateShellAsync(byte slot)
    {
        InventoryItem item = Context.Player.PlayerEntity.GetItemBySlotAndType(slot, InventoryType.Equipment);
        if (item == null)
        {
            Context.Player.SendChatMessage("Wrong slot", ChatMessageColorType.Yellow);
            return new SaltyCommandResult(false);
        }

        if (item.ItemInstance.Type != ItemInstanceType.WearableInstance)
        {
            return new SaltyCommandResult(false);
        }

        ShellType shellType = item.ItemInstance.GameItem.ShellType;
        IEnumerable<EquipmentOptionDTO> shellOptions = _shellGenerationAlgorithm.GenerateShell((byte)shellType, item.ItemInstance.Rarity, item.ItemInstance.GameItem.LevelMinimum).ToList();

        if (!shellOptions.Any())
        {
            return new SaltyCommandResult(false);
        }

        item.ItemInstance.EquipmentOptions ??= new List<EquipmentOptionDTO>();
        item.ItemInstance.EquipmentOptions.AddRange(shellOptions);

        return new SaltyCommandResult(true);
    }

    [Command("duplicate")]
    public async Task<SaltyCommandResult> DuplicateItemAsync(byte slot)
    {
        InventoryItem item = Context.Player.PlayerEntity.GetItemBySlotAndType(slot, InventoryType.Equipment);
        if (item == null)
        {
            Context.Player.SendChatMessage("Wrong slot", ChatMessageColorType.Yellow);
            return new SaltyCommandResult(false, "Wrong slot");
        }

        if (!Context.Player.PlayerEntity.HasSpaceFor(item.ItemInstance.ItemVNum))
        {
            return new SaltyCommandResult(false, "Not enough space in inventory");
        }

        if (item.ItemInstance.Type != ItemInstanceType.WearableInstance)
        {
            return new SaltyCommandResult(false);
        }

        GameItemInstance newItem = _itemInstanceFactory.DuplicateItem(item.ItemInstance);
        await Context.Player.AddNewItemToInventory(newItem);
        return new SaltyCommandResult(true, "Item duped !");
    }

    [Command("checkoptions")]
    public async Task<SaltyCommandResult> CheckOptionsAsync(byte slot)
    {
        IClientSession session = Context.Player;
        InventoryItem item = session.PlayerEntity.GetItemBySlotAndType(slot, InventoryType.Equipment);
        if (item == null)
        {
            return new SaltyCommandResult(false, "Wrong slot");
        }

        if (item.ItemInstance.Type != ItemInstanceType.WearableInstance)
        {
            return new SaltyCommandResult(false);
        }

        if (item.ItemInstance.EquipmentOptions == null || !item.ItemInstance.EquipmentOptions.Any())
        {
            return new SaltyCommandResult(false, "Item without options");
        }

        foreach (EquipmentOptionDTO option in item.ItemInstance.EquipmentOptions)
        {
            session.SendChatMessage("============[ ITEM OPTION ]============", ChatMessageColorType.Red);
            session.SendChatMessage($"EquipmentType: {option.EquipmentOptionType}", ChatMessageColorType.Green);
            session.SendChatMessage($"EffectVnum: {option.EffectVnum}", ChatMessageColorType.Green);
            session.SendChatMessage($"Weight: {option.Weight}", ChatMessageColorType.Green);
            session.SendChatMessage($"Type: {option.Type}", ChatMessageColorType.Green);
            session.SendChatMessage($"Value {option.Value}", ChatMessageColorType.Green);
            session.SendChatMessage($"Level: {option.Level}", ChatMessageColorType.Green);
        }

        return new SaltyCommandResult(true);
    }
}