// NosEmu
// 


using WingsAPI.Data.Bazaar;
using WingsEmu.Game.Items;

namespace WingsEmu.Game.Bazaar;

public class BazaarItem
{
    /// <summary>
    ///     Should not be used
    /// </summary>
    public BazaarItem()
    {
    }

    public BazaarItem(BazaarItemDTO bazaarItemDto, GameItemInstance item, string ownerName)
    {
        BazaarItemDto = bazaarItemDto;
        Item = item;
        OwnerName = ownerName;
    }

    public BazaarItemDTO BazaarItemDto { get; set; }

    public GameItemInstance Item { get; set; }

    public string OwnerName { get; set; }
}