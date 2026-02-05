using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NosEmu.Plugins.BasicImplementations.Vehicles;
using PhoenixLib.Events;
using WingsEmu.DTOs.Maps;
using WingsEmu.Game.Characters.Events;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Maps;
using WingsEmu.Game.Networking;

namespace NosEmu.Plugins.BasicImplementations.Event.Characters;

public class VehicleCheckMapSpeedEventHandler : IAsyncEventProcessor<VehicleCheckMapSpeedEvent>
{
    private readonly IMapManager _mapManager;
    private readonly IVehicleConfigurationProvider _vehicleConfiguration;

    public VehicleCheckMapSpeedEventHandler(IVehicleConfigurationProvider vehicleConfiguration, IMapManager mapManager)
    {
        _vehicleConfiguration = vehicleConfiguration;
        _mapManager = mapManager;
    }

    public async Task HandleAsync(VehicleCheckMapSpeedEvent e, CancellationToken cancellation)
    {
        IClientSession session = e.Sender;

        if (!session.PlayerEntity.IsOnVehicle)
        {
            return;
        }

        VehicleConfiguration vehicle = _vehicleConfiguration.GetByMorph(session.PlayerEntity.Morph, session.PlayerEntity.Gender);
        if (vehicle?.VehicleMapSpeeds == null)
        {
            RefreshSpeed(session, 0);
            return;
        }

        IReadOnlyList<MapFlags> flags = _mapManager.GetMapFlagByMapId(session.CurrentMapInstance.MapId);
        VehicleMapSpeed vehicleFlags = vehicle.VehicleMapSpeeds.FirstOrDefault(x => flags.Contains(x.MapFlag));

        if (vehicleFlags == null)
        {
            RefreshSpeed(session, 0);
            return;
        }

        RefreshSpeed(session, (byte)vehicleFlags.SpeedBonus);
    }

    private void RefreshSpeed(IClientSession session, byte vehicleSpeed)
    {
        session.PlayerEntity.VehicleMapSpeed = vehicleSpeed;
        session.PlayerEntity.RefreshCharacterStats();
        session.SendCondPacket();
    }
}