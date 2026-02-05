using System;
using System.Threading;
using System.Threading.Tasks;
using PhoenixLib.Events;
using WingsEmu.Game.Maps;
using WingsEmu.Game.Maps.Event;
using WingsEmu.Game.Raids;

namespace Plugin.Raids;

public class RaidInstanceActivateRaidWaves : IAsyncEventProcessor<JoinMapEndEvent>
{
    private readonly IMapManager _mapManager;
    private readonly IRaidManager _raidManager;

    public RaidInstanceActivateRaidWaves(IRaidManager raidManager, IMapManager mapManager)
    {
        _raidManager = raidManager;
        _mapManager = mapManager;
    }

    public async Task HandleAsync(JoinMapEndEvent e, CancellationToken cancellation)
    {
        if (e.JoinedMapInstance.MapInstanceType != MapInstanceType.RaidInstance)
        {
            return;
        }

        if (!e.Sender.PlayerEntity.IsInRaidParty || !e.Sender.PlayerEntity.Raid.Started)
        {
            return;
        }

        if (!e.Sender.PlayerEntity.Raid.Instance.RaidSubInstances.TryGetValue(e.JoinedMapInstance.Id, out RaidSubInstance raidSubInstance))
        {
            return;
        }

        raidSubInstance.RaidWavesActivated = true;
        raidSubInstance.LastRaidWave = DateTime.UtcNow.AddSeconds(30); // sorry, quickwin :(
    }
}