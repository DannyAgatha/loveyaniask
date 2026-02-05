using System.Threading.Tasks;
using WingsEmu.Game.Maps;

namespace WingsEmu.Game.Arena;

public interface IArenaManager
{
    public IMapInstance ArenaInstance { get; }
    public IMapInstance FamilyArenaInstance { get; }
    public Task Initialize();
}