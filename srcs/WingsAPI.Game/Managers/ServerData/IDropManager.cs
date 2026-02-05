// NosEmu
// 


using System.Collections.Generic;
using System.Threading.Tasks;
using WingsAPI.Data.Drops;
using WingsEmu.Game._enum;

namespace WingsEmu.Game.Managers.ServerData;

public class StaticDropManager
{
    public static IDropManager Instance { get; private set; }

    public static void Initialize(IDropManager dropManager)
    {
        Instance = dropManager;
    }
}

public interface IDropManager
{
    IEnumerable<DropDTO> GetGeneralDrops();
    IReadOnlyList<DropDTO> GetDropsByMapId(int mapId);
    IReadOnlyList<DropDTO> GetDropsByMonsterVnum(int monsterVnum);
    IReadOnlyList<DropDTO> GetDropsByMonsterRace(MonsterRaceType monsterRaceType, byte monsterSubRaceType);
    Task InitializeAsync();
}