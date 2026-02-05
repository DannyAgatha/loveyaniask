using PhoenixLib.Events;
using WingsEmu.Core.Extensions;
using WingsEmu.Game;
using WingsEmu.Game._i18n;
using WingsEmu.Game.Alzanor;
using WingsEmu.Game.Alzanor.Configurations;
using WingsEmu.Game.Alzanor.Events;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Managers;
using WingsEmu.Game.Networking;
using WingsEmu.Game.RainbowBattle;
using WingsEmu.Packets.Enums.Chat;

namespace Plugin.Alzanor.EventHandlers;

public class AlzanorStartProcessRegistrationEventHandler : IAsyncEventProcessor<AlzanorStartProcessRegistrationEvent>
{
    private readonly IAsyncEventPipeline _asyncEventPipeline;
    private readonly IRandomGenerator _randomGenerator;
    private readonly ISessionManager _sessionManager;
    private readonly IAlzanorManager _alzanorManager;
    private readonly AlzanorConfiguration _alzanorConfiguration;

    public AlzanorStartProcessRegistrationEventHandler(IAsyncEventPipeline asyncEventPipeline, IRandomGenerator randomGenerator, ISessionManager sessionManager, IAlzanorManager alzanorManager, AlzanorConfiguration alzanorConfiguration)
    {
        _asyncEventPipeline = asyncEventPipeline;
        _randomGenerator = randomGenerator;
        _sessionManager = sessionManager;
        _alzanorManager = alzanorManager;
        _alzanorConfiguration = alzanorConfiguration;
    }

    public async Task HandleAsync(AlzanorStartProcessRegistrationEvent e, CancellationToken cancellation)
    {
        _alzanorManager.DisableAlzanorRegistration();
        long[] registeredPlayers = _alzanorManager.RegisteredPlayers.ToArray();
        _alzanorManager.ClearRegisteredPlayers();

        HashSet<IClientSession> sessions = [];
        foreach (long playerId in registeredPlayers)
        {
            IClientSession session = _sessionManager.GetSessionByCharacterId(playerId);
            if (session == null)
            {
                continue;
            }

            if (!session.CanJoinToAlzanorEvent())
            {
                continue;
            }

            if (!session.IsGameMaster())
            {
                var sessionByIp = sessions.Where(s => s.IpAddress == session.IpAddress).ToList();
                
                if (sessionByIp.Count != 0)
                {
                    continue;
                }
            }
            
            sessions.Add(session);
        }

        List<AlzanorTeam> teams = [];
        var notEnoughPlayers = new HashSet<IClientSession>();
        var teamsList = sessions.Split(_alzanorConfiguration.MaximumPlayers).ToList();
        if (teamsList.Count > 1)
        {
            var getLastList = teamsList[^1].ToList();
            var getPreviousList = teamsList[^2].ToList();
            if (getLastList.Count <= 14)
            {
                int x = getLastList.Count + getPreviousList.Count; // 10 + 30
                int half = x / 2; // 40 / 2 = 20
                int toRemove = half - getLastList.Count; // 20 - 10 = 10
                IClientSession[] previousSession = getPreviousList.TakeLast(toRemove).ToArray();
                getLastList.AddRange(previousSession);
                foreach (IClientSession session in previousSession)
                {
                    getPreviousList.Remove(session);
                }

                teamsList[^1] = getLastList;
                teamsList[^2] = getPreviousList;
            }
        }
        
        // split it into 30, 30, 30, 10
        // if getLastList.Count <= 14, take previous list and split in half
        // 30 + 10 = 40
        // split in half = 20/20
        foreach (IEnumerable<IClientSession> members in teamsList)
        {
            var membersList = members.ToList();

            if (membersList.Count < _alzanorConfiguration.MinimumPlayers)
            {
                foreach (IClientSession session in membersList)
                {
                    notEnoughPlayers.Add(session);
                }

                continue;
            }

            var redTeam = new List<IClientSession>(15);
            var blueTeam = new List<IClientSession>(15);
            int randomNumber = _randomGenerator.RandomNumber(0, 2);

            for (int i = 0; i < membersList.Count; i++)
            {
                IClientSession member = membersList[i];
                int modulo = (randomNumber + i) % 2;
                switch (modulo)
                {
                    case 0:
                        redTeam.Add(member);
                        break;
                    case 1:
                        blueTeam.Add(member);
                        break;
                }
            }
            teams.Add(new AlzanorTeam()
            {
                RedTeam = redTeam,
                BlueTeam = blueTeam
            });
        }
        
        foreach (IClientSession session in notEnoughPlayers)
        {
            session.SendMsg(session.GetLanguage(GameDialogKey.ALZANOR_MESSAGE_NOT_ENOUGH_PLAYERS), MsgMessageType.Middle);
            session.SendChatMessage(session.GetLanguage(GameDialogKey.ALZANOR_MESSAGE_NOT_ENOUGH_PLAYERS), ChatMessageColorType.Red);
        }
        
        foreach (AlzanorTeam team in teams)
        {
            await _asyncEventPipeline.ProcessEventAsync(new AlzanorStartEvent
            {
                RedTeam = team.RedTeam,
                BlueTeam = team.BlueTeam
            });
        }
    }
    
    private class AlzanorTeam
    {
        public List<IClientSession> RedTeam { get; init; }
        public List<IClientSession> BlueTeam { get; init; }
    }
}