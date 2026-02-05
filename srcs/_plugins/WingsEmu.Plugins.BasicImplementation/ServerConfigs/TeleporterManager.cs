// NosEmu
// 


using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NosEmu.Plugins.BasicImplementations.ServerConfigs.ImportObjects.Teleporters;
using PhoenixLib.Logging;
using WingsEmu.Core.Extensions;
using WingsEmu.DTOs.ServerDatas;
using WingsEmu.Game.Battle;
using NosEmu.Plugins.BasicImplementations.ServerConfigs.ImportObjects;

namespace NosEmu.Plugins.BasicImplementations.ServerConfigs;

public class TeleporterManager : ITeleporterManager
{
    private readonly IEnumerable<TeleporterImportFile> _teleporterConfigurations;
    private readonly Dictionary<long, List<TeleporterDTO>> _teleporters = new();
    private readonly Dictionary<long, List<TeleporterDTO>> _teleportersByNpcId = new();

    public TeleporterManager(IEnumerable<TeleporterImportFile> teleporterConfigurations) => _teleporterConfigurations = teleporterConfigurations;

    public async Task InitializeAsync()
    {
        int count = 0;
        foreach (TeleporterImportFile file in _teleporterConfigurations)
        {
            var teleporters = file.Teleporters.Select(s =>
            {
                s.MapId = file.MapId;
                count++;
                return s.ToDto();
            }).ToList();
            _teleporters[file.MapId] = teleporters;
            foreach (TeleporterDTO teleporter in teleporters)
            {
                if (!_teleportersByNpcId.TryGetValue(teleporter.MapNpcId, out List<TeleporterDTO> teleporterDtos))
                {
                    teleporterDtos = new List<TeleporterDTO>();
                    _teleportersByNpcId[teleporter.MapNpcId] = teleporterDtos;
                }

                teleporterDtos.Add(teleporter);
            }
        }

        Log.Info($"[DATABASE] Loaded {count.ToString()} teleporters.");
    }

    public IReadOnlyList<TeleporterDTO> GetTeleportByNpcId(long npcId) => _teleportersByNpcId.GetOrDefault(npcId);
    public IReadOnlyList<TeleporterDTO> GetTeleportByMapId(int mapId) => _teleporters.GetOrDefault(mapId);
}