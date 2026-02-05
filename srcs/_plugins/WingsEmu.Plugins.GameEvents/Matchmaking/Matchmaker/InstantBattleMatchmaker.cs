using System;
using System.Collections.Generic;
using System.Linq;
using WingsEmu.Game._i18n;
using WingsEmu.Game.GameEvent.Configuration;
using WingsEmu.Game.GameEvent.Matchmaking.Matchmaker;
using WingsEmu.Game.GameEvent.Matchmaking.Result;
using WingsEmu.Game.Networking;
using WingsEmu.Packets.Enums.Chat;
using WingsEmu.Plugins.GameEvents.Configuration.InstantBattle;
using WingsEmu.Plugins.GameEvents.Matchmaking.Result;

namespace WingsEmu.Plugins.GameEvents.Matchmaking.Matchmaker
{
    public class InstantBattleMatchmaker : IMatchmaker
    {
        private readonly IGlobalInstantBattleConfiguration _configuration;

        public InstantBattleMatchmaker(IGlobalInstantBattleConfiguration configuration) => _configuration = configuration;

        public IMatchmakingResult Matchmake(List<IClientSession> sessions)
        {
            var games = new List<Tuple<InstantBattleConfiguration, List<IClientSession>>>();
            var refusedSessions = new Dictionary<Game18NConstString, List<IClientSession>>();

            foreach (IClientSession session in sessions)
            {
                InstantBattleConfiguration eventConfiguration = _configuration.GetInternalConfiguration(session.PlayerEntity);

                if (eventConfiguration == default)
                {
                    continue;
                }

                Tuple<InstantBattleConfiguration, List<IClientSession>> game = games.FirstOrDefault(
                    x => x.Item1 == eventConfiguration && x.Item2.Count < eventConfiguration.Requirements.Players.Maximum);

                if (game == default)
                {
                    game = new Tuple<InstantBattleConfiguration, List<IClientSession>>(eventConfiguration, new List<IClientSession>());
                    games.Add(game);
                }

                game.Item2.Add(session);
            }

            var gamesToSend = new List<Tuple<IGameEventConfiguration, List<IClientSession>>>();

            foreach (Tuple<InstantBattleConfiguration, List<IClientSession>> game in games)
            {
                if (game.Item2.Count < game.Item1.Requirements.Players.Minimum)
                {
                    foreach (IClientSession session in game.Item2)
                    {
                        DictionaryAdd(refusedSessions, Game18NConstString.NotEnoughPlayersRegistered, session);
                    }

                    continue;
                }

                gamesToSend.Add(new Tuple<IGameEventConfiguration, List<IClientSession>>(game.Item1, game.Item2));
            }

            return new InstantBattleMatchmakingResult(gamesToSend, refusedSessions);
        }

        private void DictionaryAdd(Dictionary<Game18NConstString, List<IClientSession>> dictionary, Game18NConstString dialogKey, IClientSession clientSession)
        {
            if (dictionary.ContainsKey(dialogKey))
            {
                dictionary[dialogKey].Add(clientSession);
                return;
            }

            dictionary.Add(dialogKey, new List<IClientSession>
            {
                clientSession
            });
        }
    }
}