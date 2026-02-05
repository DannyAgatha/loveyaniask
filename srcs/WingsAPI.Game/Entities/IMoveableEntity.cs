// NosEmu
// 


using WingsEmu.Game.Helpers.Damages;

namespace WingsEmu.Game.Entities;

public interface IMoveableEntity : IEntity
{
    public Position Position { get; set; }
    public short PositionX { get; }
    public short PositionY { get; }

    public byte Speed { get; set; }
}