using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using PhoenixLib.Events;
using WingsAPI.Communication.ServerApi.Protocol;
using WingsEmu.Game._i18n;
using WingsEmu.Game.Configurations;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.LandOfDeath;
using WingsEmu.Game.LandOfDeath.Events;
using WingsEmu.Game.Managers;
using WingsEmu.Packets.Enums.Chat;

namespace Plugin.LandOfDeath.Handlers;

public class LandOfDeathStartEventHandler : IAsyncEventProcessor<LandOfDeathStartEvent>
{
    private readonly ILandOfDeathManager _landOfDeathManager;
    private readonly ISessionManager _sessionManager;
    private readonly SerializableGameServer _serializableGameServer;

    public LandOfDeathStartEventHandler(ILandOfDeathManager landOfDeathManager, ISessionManager sessionManager, SerializableGameServer serializableGameServer)
    {
        _landOfDeathManager = landOfDeathManager;
        _sessionManager = sessionManager;
        _serializableGameServer = serializableGameServer;
    }

    public async Task HandleAsync(LandOfDeathStartEvent e, CancellationToken cancellation)
    {
        if (_landOfDeathManager.IsActive)
        {
            return;
        }

        _sessionManager.Broadcast(x => x.GenerateMsgPacket(x.GetLanguageFormat(GameDialogKey.LAND_OF_DEATH_HAS_OPENED, _serializableGameServer.ChannelId), MsgMessageType.Middle));
        _landOfDeathManager.Start = e.Start;
        _landOfDeathManager.End = e.End;
        _landOfDeathManager.IsActive = true;
        _landOfDeathManager.IsDevilActive = false;
    }
}