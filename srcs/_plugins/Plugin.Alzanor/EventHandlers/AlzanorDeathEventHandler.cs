using PhoenixLib.Events;
using WingsEmu.Game;
using WingsEmu.Game._i18n;
using WingsEmu.Game.Alzanor;
using WingsEmu.Game.Alzanor.Configurations;
using WingsEmu.Game.Alzanor.Events;
using WingsEmu.Game.Buffs;
using WingsEmu.Game.Characters;
using WingsEmu.Game.Characters.Events;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Items;
using WingsEmu.Game.Managers;
using WingsEmu.Packets.Enums.Chat;

namespace Plugin.Alzanor.EventHandlers;

public class AlzanorDeathEventHandler : IAsyncEventProcessor<AlzanorDeathEvent>
{
    private readonly AlzanorConfiguration _alzanorConfiguration;
    private readonly IRandomGenerator _randomGenerator;
    private readonly IBuffFactory _buffFactory;
    private readonly IGameItemInstanceFactory _gameItemInstanceFactory;
    private readonly ISessionManager _sessionManager;
    private readonly IAlzanorManager _alzanorManager;
    private readonly IGameLanguageService _languageService;

    public AlzanorDeathEventHandler(AlzanorConfiguration alzanorConfiguration, IRandomGenerator randomGenerator, IBuffFactory buffFactory, IGameItemInstanceFactory gameItemInstanceFactory, ISessionManager sessionManager, IAlzanorManager alzanorManager, IGameLanguageService languageService)
    {
        _alzanorConfiguration = alzanorConfiguration;
        _randomGenerator = randomGenerator;
        _buffFactory = buffFactory;
        _gameItemInstanceFactory = gameItemInstanceFactory;
        _sessionManager = sessionManager;
        _alzanorManager = alzanorManager;
        _languageService = languageService;
    }

    public async Task HandleAsync(AlzanorDeathEvent e, CancellationToken cancellation)
    {
        var killer = e.Killer as IPlayerEntity;
        IPlayerEntity? character = e.Sender.PlayerEntity;

        if (killer == null || character == null)
        {
            return;
        }

        if (!killer.Session.IsGameMaster())
        {
            if (killer.Session.IpAddress == character.Session.IpAddress)
            {
                killer.Session.SendChatMessage(killer.Session.GetLanguage(GameDialogKey.ALZANOR_NO_BONUS_SAME_IP_DETECTED), ChatMessageColorType.Red);
                return;
            } 
        }
        
        if(_alzanorConfiguration.SendMessageOnKill)
        {
            GameDialogKey gameDialogKey = GameDialogKey.ALZANOR_MESSAGE_KILL;
            killer.AlzanorComponent.AlzanorParty.MapInstance.Broadcast(x => 
                x.GenerateMsgPacket(x.GetLanguageFormat(gameDialogKey, killer.Name, character.Name), MsgMessageType.Middle));

            string msg = $"A player {killer.Name} has killed {character.Name}";
            await BroadcastMessageAsync(msg);
        }

        killer.AlzanorComponent.Kills += 1;
        character.AlzanorComponent.Deaths += 1;
        _alzanorManager.IncreaseKillDeathStats(killer.Session, true);
        _alzanorManager.IncreaseKillDeathStats(character.Session, false);


        if (_alzanorConfiguration.GiveKillReputationReward)
        {
            long repDiff = character.Session.PlayerEntity.Reput - killer.Session.PlayerEntity.Reput;
            
            if(repDiff > 10_000_000)
                repDiff = 10_000_000;
            else if(repDiff < -10_000_000)
                repDiff = -10_000_000;

            double reward;

            if (repDiff >= 0)
            {
                reward = 5000 + repDiff * (5000.0 / 10_000_000);
            }
            else
            {
                reward = 5000 + repDiff * (4000.0 / 10_000_000);
            }
            
            reward *= _alzanorConfiguration.KillRewardReputationMultiplier;
            
            int totalReward = (int)reward;

            await killer.Session.EmitEventAsync(new GenerateReputationEvent
            {
                Amount = totalReward,
                SendMessage = true
            });
            
            await character.Session.EmitEventAsync(new GenerateReputationEvent
            {
                Amount = totalReward * -1,
                SendMessage = true
            });
        }
    }
    
    private Task BroadcastMessageAsync(string message)
    {
        return _alzanorManager.AlzanorInstance.BroadcastAsync(_ =>
            Task.FromResult($"say 1 -1 11 {message}"));
    }
}