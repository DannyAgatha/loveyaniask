using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using PhoenixLib.Events;
using PhoenixLib.Logging;
using WingsEmu.DTOs.Maps;
using WingsEmu.Game.Characters.Events;
using WingsEmu.Game.Entities.Extensions;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Managers;
using WingsEmu.Game.Networking;
using WingsEmu.Game.Networking.Broadcasting;
using WingsEmu.Game.TimeSpaces;
using WingsEmu.Packets.Enums;
using WingsEmu.Packets.Enums.Chat;

namespace NosEmu.Plugins.BasicImplementations.Event.Characters;

public class NormalChatEventHandler : IAsyncEventProcessor<NormalChatEvent>
{
    private readonly ISessionManager _sessionManager;
    public NormalChatEventHandler(ISessionManager sessionManager)
    {
        _sessionManager = sessionManager;
    }

    public async Task HandleAsync(NormalChatEvent e, CancellationToken cancellation)
    {
        IClientSession session = e.Sender;

        if (session.CurrentMapInstance.HasMapFlag(MapFlags.ACT_4))
        {
            session.CurrentMapInstance.Broadcast(session.PlayerEntity.GenerateSayPacket(e.Message.Trim(), ChatMessageColorType.White),
                new FactionBroadcast(session.PlayerEntity.Faction), new ExceptSessionBroadcast(session));

            FactionType enemyFaction = session.PlayerEntity.Faction == FactionType.Angel ? FactionType.Demon : FactionType.Angel;
            session.CurrentMapInstance.Broadcast(session.PlayerEntity.GenerateSayPacket("^$#%#&^%$@#", ChatMessageColorType.PlayerSay),
                new FactionBroadcast(enemyFaction, true), new ExceptSessionBroadcast(session));
            return;
        }

        session.CurrentMapInstance.Broadcast(session.PlayerEntity.GenerateSayPacket(e.Message.Trim(), ChatMessageColorType.White), new ExceptSessionBroadcast(session),
            new RainbowTeamBroadcast(session));

        var sessions = _sessionManager.Sessions.ToList();

        // Nosville
        if (session.CurrentMapInstance.MapId == 1)
        {
            foreach (IClientSession ses in sessions.Where(s => s != null &&
                s.CurrentMapInstance != null && s.CurrentMapInstance.MapId == 228))
            {
                string trimmedMessage = e.Message?.Trim() ?? string.Empty;
                ses.SendPacket(session.PlayerEntity.GenerateSayPacket(trimmedMessage, ChatMessageColorType.NosVilleToCylloan));
            }
        }

        // Cylloan
        if (session.CurrentMapInstance?.MapId == 228)
        {
            foreach (IClientSession ses in sessions.Where(s => s != null &&
                s.CurrentMapInstance != null && s.CurrentMapInstance.MapId == 1))
            {
                string trimmedMessage = e.Message?.Trim() ?? string.Empty;
                ses.SendPacket(session.PlayerEntity.GenerateSayPacket(trimmedMessage, ChatMessageColorType.CylloanToNosville));
            }
        }

        TimeSpaceParty timeSpace = session.PlayerEntity.TimeSpaceComponent.TimeSpace;
        if (timeSpace?.Instance == null)
        {
            return;
        }

        foreach (TimeSpaceSubInstance timeSpaceSubInstance in timeSpace.Instance.TimeSpaceSubInstances.Values)
        {
            if (session.CurrentMapInstance.Id == timeSpaceSubInstance.MapInstance.Id)
            {
                continue;
            }

            foreach (IClientSession member in timeSpaceSubInstance.MapInstance.Sessions)
            {
                session.SendSpeakToTarget(member, e.Message.Trim(), SpeakType.Normal);
            }
        }
    }
}