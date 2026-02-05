using System.Collections.Generic;
using System.Threading.Tasks;

namespace WingsEmu.Game.Managers.ServerData;

public interface IRecipeManager
{
    Task InitializeAsync();

    IReadOnlyList<Recipe> GetRecipesByProducerItemVnum(int itemVnum);

    IReadOnlyList<Recipe> GetRecipesByNpcId(long mapNpcId);

    IReadOnlyList<Recipe> GetRecipesByNpcMonsterVnum(int npcVNum);

    IReadOnlyList<Recipe> GetRecipeByProducedItemVnum(int itemVnum);
    
    IReadOnlyList<Recipe> GetRecipesBySkillnum(int skillVNum);
}