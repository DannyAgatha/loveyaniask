using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using PhoenixLib.Events;
using WingsAPI.Packets.Enums.Chat;
using WingsEmu.Core;
using WingsEmu.Core.Extensions;
using WingsEmu.Game;
using WingsEmu.Game._i18n;
using WingsEmu.Game.Configurations;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Managers;
using WingsEmu.Game.Networking;
using WingsEmu.Game.RainbowBattle;
using WingsEmu.Game.RainbowBattle.Event;
using WingsEmu.Packets.Enums;
using WingsEmu.Packets.Enums.Chat;

namespace Plugin.RainbowBattle.EventHandlers
{
    public class RainbowBattleStartProcessRegistrationEventHandler : IAsyncEventProcessor<RainbowBattleStartProcessRegistrationEvent>
    {
        private readonly IAsyncEventPipeline _asyncEventPipeline;
        private readonly RainbowBattleConfiguration _rainbowBattleConfiguration;
        private readonly IRainbowBattleManager _rainbowBattleManager;
        private readonly IRandomGenerator _randomGenerator;
        private readonly ISessionManager _sessionManager;

        public RainbowBattleStartProcessRegistrationEventHandler(ISessionManager sessionManager, IRainbowBattleManager rainbowBattleManager, RainbowBattleConfiguration rainbowBattleConfiguration,
            IAsyncEventPipeline asyncEventPipeline, IRandomGenerator randomGenerator)
        {
            _sessionManager = sessionManager;
            _rainbowBattleManager = rainbowBattleManager;
            _rainbowBattleConfiguration = rainbowBattleConfiguration;
            _asyncEventPipeline = asyncEventPipeline;
            _randomGenerator = randomGenerator;
        }

        public async Task HandleAsync(RainbowBattleStartProcessRegistrationEvent e, CancellationToken cancellation)
        {
            _rainbowBattleManager.DisableBattleRainbowRegistration();
            IEnumerable<long> registeredPlayers = _rainbowBattleManager.RegisteredPlayers.ToArray();
            _rainbowBattleManager.ClearRegisteredPlayers();

            _sessionManager.Broadcast(x => x.GenerateEsfPacket(0));

            // Check if player can join to Rainbow Battle
            HashSet<IClientSession> sessions = new();
            foreach (long playerId in registeredPlayers)
            {
                IClientSession session = _sessionManager.GetSessionByCharacterId(playerId);
                if (session == null)
                {
                    continue;
                }

                session.SendBsInfoPacket(BsInfoType.CloseWindow, GameType.RainbowBattle, 0, QueueWindow.WaitForEntry);
                if (!session.CanJoinToRainbowBattle())
                {
                    continue;
                }

                sessions.Add(session);
            }

            List<Range<byte>> levelRanges = _rainbowBattleConfiguration.LevelRange;

            var dictionary = new Dictionary<Range<byte>, HashSet<IClientSession>>();
            var refusedSessions = new HashSet<IClientSession>();

            // Check if player is in level range
            foreach (IClientSession session in sessions)
            {
                byte level = session.PlayerEntity.Level;
                Range<byte>? levelRange = levelRanges.FirstOrDefault(x => level >= x.Minimum && level <= x.Maximum);
                if (levelRange == null)
                {
                    refusedSessions.Add(session);
                    continue;
                }

                if (!dictionary.TryGetValue(levelRange, out HashSet<IClientSession> list))
                {
                    list = new HashSet<IClientSession>();
                    dictionary[levelRange] = list;
                }

                list.Add(session);
            }

            foreach (IClientSession refusedSession in refusedSessions)
            {
                refusedSession.SendMsg(refusedSession.GetLanguage(GameDialogKey.RAINBOW_BATTLE_MESSAGE_LOW_LEVEL), MsgMessageType.Middle);
                refusedSession.SendChatMessage(refusedSession.GetLanguage(GameDialogKey.RAINBOW_BATTLE_MESSAGE_LOW_LEVEL), ChatMessageColorType.Red);
            }

            List<RainbowTeam> teams = new();
            var notEnoughPlayers = new HashSet<IClientSession>();

            foreach (HashSet<IClientSession> sessionList in dictionary.Values) // 100 players
            {
                if (sessionList.Count < _rainbowBattleConfiguration.MinimumPlayers)
                {
                    foreach (IClientSession session in sessionList)
                    {
                        notEnoughPlayers.Add(session);
                    }

                    continue;
                }

                var teamsList = sessionList.Split(_rainbowBattleConfiguration.MaximumPlayers).ToList();

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

                    var redTeam = new List<IClientSession>(3);
                    var blueTeam = new List<IClientSession>(3);
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

                    teams.Add(new RainbowTeam
                    {
                        RedTeam = redTeam,
                        BlueTeam = blueTeam
                    });
                }
            }

            foreach (IClientSession session in notEnoughPlayers)
            {
                session.SendMsgi(MessageType.Default, Game18NConstString.NotEnoughPlayerRainbowBattle);
                session.SendSayi(ChatMessageColorType.Red, Game18NConstString.NotEnoughPlayerRainbowBattle);
            }

            foreach (RainbowTeam team in teams)
            {
                await _asyncEventPipeline.ProcessEventAsync(new RainbowBattleStartEvent
                {
                    RedTeam = team.RedTeam,
                    BlueTeam = team.BlueTeam
                });
            }
        }

        private class RainbowTeam
        {
            public List<IClientSession> RedTeam { get; init; }
            public List<IClientSession> BlueTeam { get; init; }
        }
    }
}