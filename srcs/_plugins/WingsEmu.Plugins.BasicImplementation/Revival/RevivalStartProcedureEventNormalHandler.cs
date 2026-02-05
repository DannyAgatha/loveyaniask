using System;
using System.Threading;
using System.Threading.Tasks;
using PhoenixLib.Events;
using WingsAPI.Packets.Enums.Chat;
using WingsEmu.Game.Battle;
using WingsEmu.Game.Characters.Events;
using WingsEmu.Game.Configurations;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Maps;
using WingsEmu.Game.Revival;
using WingsEmu.Packets.Enums.Chat;

namespace NosEmu.Plugins.BasicImplementations.Revival;

public class RevivalStartProcedureEventNormalHandler : IAsyncEventProcessor<RevivalStartProcedureEvent>
{
    private readonly PlayerRevivalConfiguration _revivalConfiguration;

    public RevivalStartProcedureEventNormalHandler(GameRevivalConfiguration gameRevivalConfiguration) => _revivalConfiguration = gameRevivalConfiguration.PlayerRevivalConfiguration;

    public async Task HandleAsync(RevivalStartProcedureEvent e, CancellationToken cancellation)
    {
        if (e.Sender.PlayerEntity.MapInstance.MapInstanceType != MapInstanceType.NormalInstance
            && e.Sender.PlayerEntity.MapInstance.MapInstanceType != MapInstanceType.EventGameInstance
            && e.Sender.PlayerEntity.MapInstance.MapInstanceType != MapInstanceType.Miniland
            && e.Sender.PlayerEntity.MapInstance.MapInstanceType != MapInstanceType.WorldBossInstance)
        {
            return;
        }

        if (e.Sender.CurrentMapInstance.MapInstanceType is MapInstanceType.Act4Instance or MapInstanceType.Act4Dungeon or MapInstanceType.Caligor or MapInstanceType.PrivateInstance or MapInstanceType.UnderWaterShowdown)
        {
            return;
        }

        if (e.Sender.PlayerEntity.IsOnVehicle)
        {
            await e.Sender.EmitEventAsync(new RemoveVehicleEvent());
        }

        await e.Sender.PlayerEntity.RemoveBuffsOnDeathAsync();
        e.Sender.RefreshStat();

        if (e.Sender.PlayerEntity.MapInstance.MapInstanceType  == MapInstanceType.WorldBossInstance)
        {
            e.Sender.PlayerEntity.UpdateRevival(DateTime.UtcNow.AddSeconds(10), RevivalType.DontPayRevival, ForcedType.Forced);
            e.Sender.SendMsgi(MessageType.Center,Game18NConstString.ReviveSoon);
        }
        else
        {
            e.Sender.PlayerEntity.UpdateRevival(DateTime.UtcNow + _revivalConfiguration.RevivalDialogDelay, RevivalType.DontPayRevival, ForcedType.Forced);
        }
    }
}