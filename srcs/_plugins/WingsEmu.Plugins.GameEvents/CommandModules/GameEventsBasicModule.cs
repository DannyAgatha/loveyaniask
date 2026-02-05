using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PhoenixLib.Events;
using PhoenixLib.ServiceBus;
using Qmmands;
using WingsAPI.Communication.InstantBattle;
using WingsEmu.Commands.Checks;
using WingsEmu.Commands.Entities;
using WingsEmu.DTOs.Account;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.GameEvent;
using WingsEmu.Game.Networking;
using WingsEmu.Plugins.GameEvents.DataHolder;
using WingsEmu.Plugins.GameEvents.Event.Global;
using WingsEmu.Plugins.GameEvents.Event.InstantBattle;

namespace WingsEmu.Plugins.GameEvents.CommandModules
{
    [Name("GameEventsBasic")]
    [Group("ic", "instant-combat", "instant-battle")]
    [Description("Module related to GameEvents commands.")]
    [RequireAuthority(AuthorityType.GM)]
    public class GameEventsBasicModule : SaltyModuleBase
    {
        private readonly IAsyncEventPipeline _eventPipeline;
        private readonly IGameEventInstanceManager _gameEventInstanceManager;
        private readonly IMessagePublisher<InstantBattleStartMessage> _messagePublisher;

        public GameEventsBasicModule(IAsyncEventPipeline eventPipeline, IMessagePublisher<InstantBattleStartMessage> messagePublisher, IGameEventInstanceManager gameEventInstanceManager)
        {
            _eventPipeline = eventPipeline;
            _messagePublisher = messagePublisher;
            _gameEventInstanceManager = gameEventInstanceManager;
        }

        [Command("start")]
        [Description("Starts an instant combat")]
        public async Task<SaltyCommandResult> StartGameEvent(bool noDelay = true)
        {
            Context.Player.SendSuccessChatMessage("Instant combat started");
            await _messagePublisher.PublishAsync(new InstantBattleStartMessage
            {
                HasNoDelay = noDelay
            });
            await _eventPipeline.ProcessEventAsync(new GameEventPrepareEvent(GameEventType.InstantBattle)
            {
                NoDelay = noDelay
            });

            return new SaltyCommandResult(true);
        }

        [Command("finish")]
        [Description("Finish instant-combat")]
        public async Task<SaltyCommandResult> Finish()
        {
            IClientSession session = Context.Player;

            IReadOnlyCollection<IGameEventInstance> instantBattles = _gameEventInstanceManager.GetGameEventsByType(GameEventType.InstantBattle);
            if (instantBattles == null || !instantBattles.Any())
            {
                return new SaltyCommandResult(false, "There is no Instant Battles now.");
            }

            IGameEventInstance mapInstance = instantBattles.FirstOrDefault(x => x?.MapInstance != null && x.MapInstance.Id == session.CurrentMapInstance.Id);
            if (mapInstance == null)
            {
                return new SaltyCommandResult(false, "Not in Instant Battle map.");
            }

            if (mapInstance is not InstantBattleInstance instance)
            {
                return new SaltyCommandResult(false, "Not in Instant Battle map.");
            }

            instance.AvailableWaves.Clear();
            await _eventPipeline.ProcessEventAsync(new InstantBattleCompleteEvent(instance));
            return new SaltyCommandResult(true);
        }
    }
}