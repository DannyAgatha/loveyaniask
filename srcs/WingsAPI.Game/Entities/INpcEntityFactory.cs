using WingsEmu.DTOs.Maps;
using WingsEmu.Game.Maps;

namespace WingsEmu.Game.Entities;

public interface INpcEntityFactory
{
    public INpcEntity CreateMapNpc(int monsterVNum, IMapInstance mapInstance, int? id = null, INpcAdditionalData npcAdditionalData = null);
    public INpcEntity CreateMapNpc(IMonsterData monsterDto, IMapInstance mapInstance, int? id = null, INpcAdditionalData npcAdditionalData = null);
    public INpcEntity CreateMapNpc(MapNpcDTO npcDto, IMapInstance mapInstance, int? id = null, INpcAdditionalData npcAdditionalData = null);
    public INpcEntity CreateMapNpc(IMonsterData monsterDto, MapNpcDTO npcDto, IMapInstance mapInstance, int? id = null, INpcAdditionalData npcAdditionalData = null);

    INpcEntity CreateNpc(int monsterVNum, IMapInstance mapInstance, int? id = null, INpcAdditionalData npcAdditionalData = null);
    INpcEntity CreateNpc(IMonsterData monsterDto, IMapInstance mapInstance, int? id = null, INpcAdditionalData npcAdditionalData = null);
}