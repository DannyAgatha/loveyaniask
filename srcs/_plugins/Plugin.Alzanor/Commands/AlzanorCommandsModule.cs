using PhoenixLib.Events;
using PhoenixLib.ServiceBus;
using Qmmands;
using WingsEmu.Commands.Checks;
using WingsEmu.Commands.Entities;
using WingsEmu.DTOs.Account;
using WingsEmu.Game.Alzanor.Communication;
using WingsEmu.Game.Alzanor.Events;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.GameEvent;
using WingsEmu.Plugins.GameEvents.Event.Global;

namespace Plugin.Alzanor.Commands;


[Name("Alzanor")]
[Group("alzanor")]
[RequireAuthority(AuthorityType.Owner)]
public class AlzanorCommandsModule : SaltyModuleBase
{
    private readonly IAsyncEventPipeline _eventPipeline;
    private readonly IGameEventInstanceManager _gameEventInstanceManager;
    private readonly IMessagePublisher<AlzanorStartMessage> _messagePublisher;

    public AlzanorCommandsModule(IAsyncEventPipeline eventPipeline, IGameEventInstanceManager gameEventInstanceManager, IMessagePublisher<AlzanorStartMessage> messagePublisher)
    {
        _eventPipeline = eventPipeline;
        _gameEventInstanceManager = gameEventInstanceManager;
        _messagePublisher = messagePublisher;
    }
    
    [Command("start")]
    [Description("Starts an Alzanor event")]
    public async Task<SaltyCommandResult> StartGameEvent(bool noDelay = true)
    {
        Context.Player.SendSuccessChatMessage("Alzanor started");
        await Context.Player.EmitEventAsync(new AlzanorStartRegisterEvent());

        return new SaltyCommandResult(true);
    }
}