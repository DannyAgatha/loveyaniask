using System;
using System.Threading;
using System.Threading.Tasks;
using PhoenixLib.Events;
using WingsAPI.Packets.Enums;
using WingsAPI.Packets.Enums.BattlePass;
using WingsAPI.Packets.Enums.Prestige;
using WingsEmu.DTOs.Bonus;
using WingsEmu.Game._i18n;
using WingsEmu.Game.Buffs;
using WingsEmu.Game.Characters;
using WingsEmu.Game.Characters.Events;
using WingsEmu.Game.Configurations;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Managers;
using WingsEmu.Game.Prestige;
using WingsEmu.Packets.Enums;
using WingsEmu.Packets.Enums.Character;
using WingsEmu.Packets.Enums.Chat;

namespace NosEmu.Plugins.BasicImplementations.Event.Characters;

public class GenerateReputationEventHandler : IAsyncEventProcessor<GenerateReputationEvent>
{
    private readonly IGameLanguageService _languageService;
    private readonly GameMinMaxConfiguration _minMaxConfiguration;
    private readonly IRankingManager _rankingManager;
    private readonly IReputationConfiguration _reputationConfiguration;
    private readonly IServerManager _serverManager;
    private readonly IEvtbConfiguration _evtbConfiguration;
    private readonly IAsyncEventPipeline _asyncEventPipeline;

    public GenerateReputationEventHandler(IServerManager serverManager, IGameLanguageService languageService, GameMinMaxConfiguration minMaxConfiguration,
        IReputationConfiguration reputationConfiguration, IRankingManager rankingManager, IEvtbConfiguration evtbConfiguration,  IAsyncEventPipeline asyncEventPipeline)
    {
        _serverManager = serverManager;
        _languageService = languageService;
        _minMaxConfiguration = minMaxConfiguration;
        _reputationConfiguration = reputationConfiguration;
        _rankingManager = rankingManager;
        _evtbConfiguration = evtbConfiguration;
        _asyncEventPipeline = asyncEventPipeline;
    }

    public async Task HandleAsync(GenerateReputationEvent e, CancellationToken cancellation)
    {
        IPlayerEntity character = e.Sender.PlayerEntity;
        int eventIncreaseReputation = _evtbConfiguration.GetValueForEventType(EvtbType.INCREASE_REPUTATION_EARNED);
        double reputationMultiplier = eventIncreaseReputation >= 1 ? eventIncreaseReputation : 1;
        long amount = (long)(e.Amount * _serverManager.ReputRate * reputationMultiplier);

        if (character.Reput <= 0 && amount <= 0)
        {
            return;
        }
        
        if (character.SubClass == SubClassType.ArrowLord && !character.Session.CurrentMapInstance.IsPvp)
        {
            double reputationBonusTier = character.TierLevel switch
            {
                1 => 0.10, // Increase by 10%
                2 => 0.11, // Increase by 11%
                3 => 0.12, // Increase by 12%
                4 => 0.13, // Increase by 13%
                5 => 0.14, // Increase by 14%
                _ => 0     // No increase if the tier level is not specified
            };
            
            amount += (long)(amount * reputationBonusTier);
        }

        long oldReput = character.Reput;

        double multiplier = 1;

        multiplier += amount >= 0 ? character.BCardComponent.GetAllBCardsInformation(BCardType.Reputation, (byte)AdditionalTypes.Reputation.IncreaseEarnedReputation, character.Level).firstData * 0.01 : 0;
        multiplier += amount >= 0 ? character.BCardComponent.GetAllBCardsInformation(BCardType.ReputHeroLevel, (byte)AdditionalTypes.ReputHeroLevel.ReputIncreased, character.Level).firstData * 0.01 : 0;
        multiplier += amount >= 0 ? character.HaveStaticBonus(StaticBonusType.EreniaMedal) ? 0.20 : 0 : 0;

        amount = (long)(amount * multiplier);

        character.Reput += amount;

        if (character.Reput < _minMaxConfiguration.MinReputation)
        {
            character.Reput = _minMaxConfiguration.MinReputation;
        }

        if (_minMaxConfiguration.MaxReputation < character.Reput)
        {
            character.Reput = _minMaxConfiguration.MaxReputation;
        }

        character.Session.RefreshReputation(_reputationConfiguration, _rankingManager.TopReputation);

        await e.Sender.EmitEventAsync(new IncreaseBattlePassObjectiveEvent(MissionType.ReachXReputation, character.Reput));
        await _asyncEventPipeline.ProcessEventAsync(new PrestigeProgressEvent(e.Sender, PrestigeTaskType.GAIN_FAME, amount: amount), cancellation);
        bool decrease = amount < 0;

        if (!e.SendMessage)
        {
            return;
        }
        
        character.Session.SendSayi(decrease ? ChatMessageColorType.Red : ChatMessageColorType.Green, decrease ? Game18NConstString.ReputationReduced : Game18NConstString.ReputationIncreased, 4, (int)Math.Abs(oldReput - character.Reput));
    }
}