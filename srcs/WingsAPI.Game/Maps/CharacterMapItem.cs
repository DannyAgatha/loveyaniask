// NosEmu
// 


using WingsEmu.Game.Items;

namespace WingsEmu.Game.Maps;

public class CharacterMapItem : MapItem
{
    public CharacterMapItem(short x, short y, GameItemInstance itemInstance, IMapInstance mapInstance, bool isQuest = false) : base(x, y, isQuest, mapInstance) => ItemInstance = itemInstance;

    public override int Amount => ItemInstance.Amount;

    public override int ItemVNum => ItemInstance.ItemVNum;

    public override GameItemInstance GetItemInstance() => ItemInstance;
}