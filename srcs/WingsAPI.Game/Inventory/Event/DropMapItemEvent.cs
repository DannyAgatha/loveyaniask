using PhoenixLib.Events;
using WingsEmu.Game.Entities;
using WingsEmu.Game.Helpers.Damages;
using WingsEmu.Game.Maps;
using WingsEmu.Game.Networking;

namespace WingsEmu.Game.Inventory.Event;

public class ThrowItemEvent : IAsyncEvent
{
    public ThrowItemEvent(IBattleEntity battleEntity, int itemVnum, int quantity, int minimumDistance, int maximumDistance)
    {
        BattleEntity = battleEntity;
        ItemVnum = itemVnum;
        Quantity = quantity;
        MinimumDistance = minimumDistance;
        MaximumDistance = maximumDistance;
    }

    public IBattleEntity BattleEntity { get; }
    public int ItemVnum { get; }
    public int Quantity { get; }
    public int MinimumDistance { get; }
    public int MaximumDistance { get; }
}

public class DropMapItemEvent : IAsyncEvent
{
    public DropMapItemEvent(IClientSession session, IMapInstance map, Position position, short vnum, int amount, short design = 0, short rarity = 0, short upgrade = 0, long ownerId = -1, bool isQuest = false)
    {
        Session = session;
        Map = map;
        Position = position;
        Vnum = vnum;
        Amount = amount;
        Design = design;
        Rarity = rarity;
        Upgrade = upgrade;
        OwnerId = ownerId;
        IsQuest = isQuest;
    }

    public IClientSession Session { get; }
    public IMapInstance Map { get; }
    public Position Position { get; }
    public short Vnum { get; }
    public int Amount { get; }
    public short Design { get; }
    public short Rarity { get; }
    public short Upgrade { get; }
    public long OwnerId { get; }
    public bool IsQuest { get; }
}