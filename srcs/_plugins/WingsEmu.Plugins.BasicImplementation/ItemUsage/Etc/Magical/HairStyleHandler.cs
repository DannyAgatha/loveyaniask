using System;
using System.Threading.Tasks;
using WingsAPI.Game.Extensions.ItemExtension.Inventory;
using WingsEmu.Game._enum;
using WingsEmu.Game._ItemUsage;
using WingsEmu.Game._ItemUsage.Event;
using WingsEmu.Game.Characters;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Items;
using WingsEmu.Game.Networking;
using WingsEmu.Packets.Enums;
using WingsEmu.Packets.Enums.Character;

namespace NosEmu.Plugins.BasicImplementations.ItemUsage.Etc.Magical;

public class HairStyleHandler : IItemHandler
{
    public ItemType ItemType => ItemType.Magical;
    public long[] Effects { get; } = { 11 };

    public async Task HandleAsync(IClientSession session, InventoryUseItemEvent e)
    {
        IPlayerEntity character = session.PlayerEntity;
        IGameItem gameItem = e.Item.ItemInstance.GameItem;

        if (character.Class == ClassType.Adventurer && (gameItem.Id == (int)ItemVnums.SUPER_HAIR_GEL || gameItem.Id == (int)ItemVnums.SUPER_HAIR_WAX))
        {
            return;
        }

        if (!Enum.TryParse(gameItem.EffectValue.ToString(), out HairStyleType hairStyle))
        {
            return;
        }

        if (character.HairStyle == hairStyle && hairStyle != HairStyleType.A)
        {
            return;
        }

        if ((hairStyle == HairStyleType.ChoppyBangs || hairStyle == HairStyleType.FrenchBraid) && character.Gender == GenderType.Male)
        {
            return;
        }

        if ((hairStyle == HairStyleType.FauxHawk || hairStyle == HairStyleType.JellyRolls) && character.Gender == GenderType.Female)
        {
            return;
        }

        if (hairStyle == HairStyleType.A)
        {
            character.HairStyle = character.HairStyle == HairStyleType.A ? HairStyleType.B : HairStyleType.A;
        }
        else
        {
            character.HairStyle = hairStyle;
        }

        session.BroadcastEq();
        await session.RemoveItemFromInventory(item: e.Item);
    }
}