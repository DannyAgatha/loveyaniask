using System.Collections.Generic;
using System.Threading.Tasks;
using WingsEmu.DTOs.ServerDatas;
using WingsEmu.Game.Helpers.Damages;

namespace WingsEmu.Game.Battle;

public interface ITeleportManager
{
    void SavePosition(long id, Position position);
    Position GetPosition(long id);
    void RemovePosition(long id);
}

public interface ITeleporterManager
{
    Task InitializeAsync();
    IReadOnlyList<TeleporterDTO> GetTeleportByNpcId(long npcId);
    IReadOnlyList<TeleporterDTO> GetTeleportByMapId(int mapId);
}