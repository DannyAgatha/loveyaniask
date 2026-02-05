using System;
using System.Threading.Tasks;
using WingsAPI.Game.Extensions.ItemExtension.Inventory;
using WingsEmu.Game;
using WingsEmu.Game._enum;
using WingsEmu.Game._ItemUsage;
using WingsEmu.Game._ItemUsage.Event;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Items;
using WingsEmu.Game.Networking;
using WingsEmu.Packets.Enums;
using WingsEmu.Packets.Enums.Character;

namespace NosEmu.Plugins.BasicImplementations.ItemUsage.Etc.Magical;

public class HairDyeHandler : IItemUsageByVnumHandler
{
    private readonly IRandomGenerator _randomGenerator;

    public HairDyeHandler(IRandomGenerator randomGenerator) => _randomGenerator = randomGenerator;

    public long[] Vnums => new[]
    {
        (long)ItemVnums.MYSTERIOUS_HAIR_DYE, (long)ItemVnums.MYSTERIOUS_HAIR_DYE_LIMITED, (long)ItemVnums.SAVAGE_DYES,
        (long)ItemVnums.HAIR_DYE_11,
        (long)ItemVnums.HAIR_DYE_12,
        (long)ItemVnums.HAIR_DYE_13,
        (long)ItemVnums.HAIR_DYE_14,
        (long)ItemVnums.HAIR_DYE_15,
        (long)ItemVnums.HAIR_DYE_16,
        (long)ItemVnums.HAIR_DYE_17,
        (long)ItemVnums.HAIR_DYE_18,
        (long)ItemVnums.HAIR_DYE_19,
        (long)ItemVnums.HAIR_DYE_20,
        (long)ItemVnums.RED_DYE,
        (long)ItemVnums.DARK_DYE,
        (long)ItemVnums.BLUE_DYE,
        (long)ItemVnums.GREY_DYE,
        (long)ItemVnums.PINK_DYE,
        (long)ItemVnums.BLACK_DYE,
        (long)ItemVnums.BROWN_DYE,
        (long)ItemVnums.GREEN_DYE,
        (long)ItemVnums.WHITE_DYE,
        (long)ItemVnums.ORANGE_DYE,
        (long)ItemVnums.PURPLE_DYE,
        (long)ItemVnums.YELLOW_DYE,
        (long)ItemVnums.LIGHT_BLUE_DYE,
        (long)ItemVnums.LIGHT_GREEN_DYE
    };

    public async Task HandleAsync(IClientSession session, InventoryUseItemEvent e)
    {
        IGameItem gameItem = e.Item.ItemInstance.GameItem;
        int itemVnum = e.Item.ItemInstance.ItemVNum;

        if (itemVnum == (short)ItemVnums.SAVAGE_DYES)
        {
            HairStyleType hairStyle = session.PlayerEntity.HairStyle;
            if (hairStyle is HairStyleType.FemaleSpecialHair or HairStyleType.MaleSpecialHair)
            {
                return;
            }

            if (hairStyle != HairStyleType.ChoppyBangs && hairStyle != HairStyleType.FrenchBraid && hairStyle != HairStyleType.FauxHawk && hairStyle != HairStyleType.JellyRolls)
            {
                return;
            }

            session.PlayerEntity.HairStyle = session.PlayerEntity.Gender == GenderType.Female ? HairStyleType.FemaleSpecialHair : HairStyleType.MaleSpecialHair;

            session.BroadcastEq();
            await session.RemoveItemFromInventory(item: e.Item);

            return;
        }

        if (itemVnum == (int)ItemVnums.MYSTERIOUS_HAIR_DYE || itemVnum == (int)ItemVnums.MYSTERIOUS_HAIR_DYE_LIMITED)
        {
            short nextValue = (short)_randomGenerator.RandomNumber(0, 127);
            session.PlayerEntity.HairColor = Enum.TryParse(nextValue.ToString(), out HairColorType hairColorType) ? hairColorType : 0;
        }
        else
        {
            session.PlayerEntity.HairColor = Enum.TryParse(gameItem.EffectValue.ToString(), out HairColorType hairColorType) ? hairColorType : 0;
        }

        session.BroadcastEq();
        await session.RemoveItemFromInventory(item: e.Item);
    }
}