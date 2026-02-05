// NosEmu
// 


using System.Collections.Generic;
using System.Threading.Tasks;
using WingsEmu.Game.Entities;

namespace WingsEmu.Game.Managers.StaticData;

public class StaticNpcMonsterManager
{
    public static INpcMonsterManager Instance { get; private set; }

    public static void Initialize(INpcMonsterManager manager)
    {
        Instance = manager;
    }
}

public interface INpcMonsterManager
{
    IMonsterData GetNpc(int vnum);
    List<IMonsterData> GetNpc(string name);
    Task InitializeAsync();
}