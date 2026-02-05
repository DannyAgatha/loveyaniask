// NosEmu
// 


using WingsEmu.Game.Maps;
using WingsEmu.Packets.Enums;

namespace WingsEmu.Game.Entities;

public interface IEntity
{
    public VisualType Type { get; }
    public int Id { get; }
    public IMapInstance MapInstance { get; }
}