using WingsEmu.Packets.Enums;

namespace WingsEmu.Game.Items;

public interface IDropRarityConfigurationProvider
{
    sbyte GetRandomRarity(ItemType itemType);
}