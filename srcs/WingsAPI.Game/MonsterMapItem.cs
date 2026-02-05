// NosEmu
// 


using WingsEmu.Game.Items;
using WingsEmu.Game.Maps;

namespace WingsEmu.Game;

public sealed class MonsterMapItem : MapItem
{
    #region Instantiation

    public MonsterMapItem(short x, short y, GameItemInstance itemInstance, IMapInstance mapInstance, long ownerId = -1, bool isQuest = false) : base(x, y, isQuest, mapInstance)
    {
        OwnerId = ownerId;
        ItemInstance = itemInstance;
    }

    public override int Amount => ItemInstance.Amount;

    public override int ItemVNum => ItemInstance.ItemVNum;

    public long? OwnerId { get; }

    public override GameItemInstance GetItemInstance() => ItemInstance;

    #endregion
}