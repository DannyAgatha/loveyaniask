// NosEmu
// 


using System;
using WingsEmu.Game.Items;
using WingsEmu.Game.Managers;

namespace WingsEmu.Game.Maps;

public abstract class MapItem
{
    private long _transportId;

    protected GameItemInstance ItemInstance;

    public MapItem(short x, short y, bool isQuest, IMapInstance mapInstance)
    {
        PositionX = x;
        PositionY = y;
        IsQuest = isQuest;
        MapInstance = mapInstance;
        CreatedDate = DateTime.UtcNow;
        ShowMessageEasterEgg = DateTime.UtcNow;
        TransportId = 0;
    }

    public virtual int Amount { get; set; }

    public DateTime? CreatedDate { get; set; }

    public DateTime ShowMessageEasterEgg { get; set; }

    public virtual int ItemVNum { get; set; }

    public short PositionX { get; set; }

    public short PositionY { get; set; }

    public bool IsQuest { get; }

    public IMapInstance MapInstance { get; }

    public long TransportId
    {
        get
        {
            if (_transportId == 0)
            {
                // create transportId thru factory
                // TODO: Review has some problems, aka. issue corresponding to weird/multiple/missplaced drops
                _transportId = TransportFactory.Instance.GenerateTransportId();
            }

            return _transportId;
        }

        private set
        {
            if (value != _transportId)
            {
                _transportId = value;
            }
        }
    }

    public abstract GameItemInstance GetItemInstance();
}