using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WingsAPI.Game.Extensions.ItemExtension.Inventory;
using WingsAPI.Packets.Enums.Shells;
using WingsEmu.DTOs.Items;
using WingsEmu.Game;
using WingsEmu.Game._i18n;
using WingsEmu.Game._ItemUsage;
using WingsEmu.Game._ItemUsage.Event;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Inventory;
using WingsEmu.Game.Items;
using WingsEmu.Game.Networking;
using WingsEmu.Packets.Enums;
using WingsEmu.Packets.Enums.Chat;

namespace NosEmu.Plugins.BasicImplementations.ItemUsage.Etc.Magical;

public class ShellItemHandler : IItemHandler
{
    private static readonly ShellEffectType[] _mainWeaponShells =
    {
        ShellEffectType.AntiMagicDisorder,
        ShellEffectType.GainMoreXP,
        ShellEffectType.GainMoreCXP,
        ShellEffectType.GainMoreGold
    };

    private readonly IGameLanguageService _gameLanguage;
    private readonly IRandomGenerator _randomGenerator;

    public ShellItemHandler(IGameLanguageService gameLanguage, IRandomGenerator randomGenerator)
    {
        _gameLanguage = gameLanguage;
        _randomGenerator = randomGenerator;
    }

    public ItemType ItemType => ItemType.Shell;
    public long[] Effects => new long[] { 0 };

    public async Task HandleAsync(IClientSession session, InventoryUseItemEvent e)
    {
        InventoryItem inv = e.Item;
        string[] packetsplit = e.Packet;

        if (packetsplit == null)
        {
            return;
        }

        if (e?.Item == null)
        {
            return;
        }

        if (inv.ItemInstance.EquipmentOptions == null || !inv.ItemInstance.EquipmentOptions.Any())
        {
            session.SendMsg(_gameLanguage.GetLanguage(GameDialogKey.SHELLS_SHOUTMESSAGE_MUST_BE_IDENTIFIED, session.UserLanguage), MsgMessageType.Middle);
            return;
        }

        if (packetsplit.Length < 9)
        {
            // MODIFIED PACKET
            return;
        }

        if (!short.TryParse(packetsplit[9], out short eqSlot) || !Enum.TryParse(packetsplit[8], out InventoryType eqType))
        {
            return;
        }

        if (!int.TryParse(packetsplit[6], out int requestType))
        {
            return;
        }

        GameItemInstance shell = inv.ItemInstance;
        InventoryItem eq = session.PlayerEntity.GetItemBySlotAndType(eqSlot, eqType);

        if (eq == null)
        {
            // PACKET MODIFIED
            return;
        }

        if (eq.ItemInstance.Type != ItemInstanceType.WearableInstance)
        {
            return;
        }

        GameItemInstance itemInstance = eq.ItemInstance;

        if (itemInstance.GameItem.ItemType != ItemType.Armor && shell.GameItem.ItemSubType == 1)
        {
            // ARMOR SHELL ONLY APPLY ON ARMORS
            session.SendMsg(_gameLanguage.GetLanguage(GameDialogKey.SHELLS_SHOUTMESSAGE_FOR_ARMOR_ONLY, session.UserLanguage), MsgMessageType.Middle);
            return;
        }

        if (itemInstance.GameItem.ItemType != ItemType.Weapon && shell.GameItem.ItemSubType == 0)
        {
            // WEAPON SHELL ONLY APPLY ON WEAPONS
            session.SendMsg(_gameLanguage.GetLanguage(GameDialogKey.SHELLS_SHOUTMESSAGE_FOR_WEAPON_ONLY, session.UserLanguage), MsgMessageType.Middle);
            return;
        }

        switch (requestType)
        {
            case 0:
                session.SendPacket(itemInstance.EquipmentOptions?.Count > 0
                    ? $"qna #u_i^1^{session.PlayerEntity.Id}^{(short)inv.ItemInstance.GameItem.Type}^{inv.Slot}^1^1^{(short)eqType}^{eqSlot} {_gameLanguage.GetLanguage(GameDialogKey.SHELLS_DIALOG_REPLACE_OPTIONS, session.UserLanguage)}"
                    : $"qna #u_i^1^{session.PlayerEntity.Id}^{(short)inv.ItemInstance.GameItem.Type}^{inv.Slot}^1^1^{(short)eqType}^{eqSlot} {_gameLanguage.GetLanguage(GameDialogKey.SHELLS_DIALOG_ADD_OPTIONS, session.UserLanguage)}");
                break;
            case 1:
                if (shell.EquipmentOptions == null)
                {
                    // SHELL NOT IDENTIFIED
                    return;
                }

                if (itemInstance.BoundCharacterId != session.PlayerEntity.Id && itemInstance.BoundCharacterId != null)
                {
                    // NEED TO PERFUME STUFF BEFORE CHANGING SHELL
                    session.SendMsg(_gameLanguage.GetLanguage(GameDialogKey.SHELL_SHOUTMESSAGE_NEED_PERFUM_TO_CHANGE_SHELL, session.UserLanguage), MsgMessageType.Middle);
                    return;
                }

                if (itemInstance.Rarity < shell.Rarity)
                {
                    // RARITY TOO HIGH ON SHELL
                    session.SendMsg(_gameLanguage.GetLanguage(GameDialogKey.SHELLS_SHOUTMESSAGE_RARITY_TOO_HIGH, session.UserLanguage), MsgMessageType.Middle);
                    return;
                }

                // if (itemInstance.GameItem.IsHeroic)
                // {
                //     // ITEM IS HEROIC
                //     session.SendMsg(_gameLanguage.GetLanguage(GameDialogKey.GAMBLING_MESSAGE_ITEM_IS_HEROIC, session.UserLanguage), MsgMessageType.Middle);
                //     return;
                // }

                if (itemInstance.GameItem.LevelMinimum < shell.Upgrade && !itemInstance.GameItem.IsHeroic)
                {
                    // SHELL LEVEL TOO HIGH
                    session.SendMsg(_gameLanguage.GetLanguage(GameDialogKey.SHELLS_SHOUTMESSAGE_LEVEL_TOO_HIGH, session.UserLanguage), MsgMessageType.Middle);
                    return;
                }

                if (itemInstance.EquipmentOptions != null && itemInstance.EquipmentOptions.Any() && _randomGenerator.RandomNumber() > 50)
                {
                    // BREAK BECAUSE DIDN'T USE MAGIC ERASER
                    session.SendMsg(_gameLanguage.GetLanguage(GameDialogKey.SHELLS_SHOUTMESSAGE_BROKEN, session.UserLanguage), MsgMessageType.Middle);
                    await session.RemoveItemFromInventory(item: inv);
                    return;
                }

                // If it's second weapon - remove all (Main Weapon) shells
                if (itemInstance.GameItem.EquipmentSlot == EquipmentType.SecondaryWeapon)
                {
                    var toRemove = new List<EquipmentOptionDTO>();
                    foreach (EquipmentOptionDTO i in shell.EquipmentOptions)
                    {
                        if (i == null)
                        {
                            continue;
                        }

                        var type = (ShellEffectType)i.Type;
                        if (!_mainWeaponShells.Contains(type))
                        {
                            continue;
                        }

                        toRemove.Add(i);
                    }

                    foreach (EquipmentOptionDTO remove in toRemove)
                    {
                        shell.EquipmentOptions.Remove(remove);
                    }
                }

                itemInstance.EquipmentOptions?.Clear();
                itemInstance.EquipmentOptions ??= new List<EquipmentOptionDTO>();
                foreach (EquipmentOptionDTO i in shell.EquipmentOptions)
                {
                    session.SendGuriPacket(17, 1, session.PlayerEntity.Id);
                    session.SendMsg(_gameLanguage.GetLanguage(GameDialogKey.SHELLS_SHOUTMESSAGE_OPTION_SET, session.UserLanguage), MsgMessageType.Middle);
                    itemInstance.EquipmentOptions.Add(i);
                }

                itemInstance.BoundCharacterId = session.PlayerEntity.Id;
                itemInstance.ShellRarity = shell.Rarity;
                await session.RemoveItemFromInventory(item: inv);
                break;
        }
    }
}