using System;
using System.Threading;
using System.Threading.Tasks;
using PhoenixLib.Events;
using WingsAPI.Packets.Enums.BattlePass;
using WingsEmu.Game._enum;
using WingsEmu.Game._i18n;
using WingsEmu.Game.Characters;
using WingsEmu.Game.Characters.Events;
using WingsEmu.Game.Configurations.Miniland;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Miniland;
using WingsEmu.Game.Miniland.Events;

namespace NosEmu.Plugins.BasicImplementations.Event.Miniland;

public class MinigamePlayEventHandler : IAsyncEventProcessor<MinigamePlayEvent>
{
    private const MinigameInteraction ThisAction = MinigameInteraction.DeclaratePlay;
    private readonly IGameLanguageService _languageService;
    private readonly MinigameConfiguration _minigameConfiguration;
    private readonly IMinigameManager _minigameManager;

    public MinigamePlayEventHandler(IMinigameManager minigameManager, IGameLanguageService languageService,
        MinigameConfiguration minigameConfiguration)
    {
        _minigameManager = minigameManager;
        _languageService = languageService;
        _minigameConfiguration = minigameConfiguration;
    }

    public Task HandleAsync(MinigamePlayEvent e, CancellationToken cancellation)
    {
        MinilandInteractionInformationHolder lastMinilandInteraction = _minigameManager.GetLastInteraction(e.Sender);

        IPlayerEntity character = e.Sender.PlayerEntity;

        if (lastMinilandInteraction.Interaction != MinigameInteraction.GetReward
            && lastMinilandInteraction.Interaction != MinigameInteraction.GetMinigameInformation
            && lastMinilandInteraction.Interaction != ThisAction
            && lastMinilandInteraction.MapObject != e.MinigameObject)
        {
            _minigameManager.ReportInteractionIncoherence(e.Sender, lastMinilandInteraction.Interaction, lastMinilandInteraction.MapObject, ThisAction, e.MinigameObject);
            return Task.CompletedTask;
        }

        if (!e.IsForFun && character.MinilandPoint < _minigameConfiguration.Configuration.MinigamePointsCostPerMinigame)
        {
            e.Sender.SendQnaPacket($"mg 10 {e.MinigameObject.InventoryItem.Slot.ToString()} {e.MinigameObject.InventoryItem.ItemInstance.ItemVNum.ToString()}",
                _languageService.GetLanguage(GameDialogKey.MINILAND_DIALOG_ASK_MINIGAME_FOR_FUN, e.Sender.UserLanguage));
            return Task.CompletedTask;
        }

        Minigame minigameConfiguration = _minigameManager.GetSpecificMinigameConfiguration(e.MinigameObject.InventoryItem.ItemInstance.ItemVNum);
        if (minigameConfiguration == default)
        {
            return Task.CompletedTask;
        }

        EffectType minigameEffect = minigameConfiguration.Type switch
        {
            MinigameType.Quarry => EffectType.MinigameQuarry,
            MinigameType.Sawmill => EffectType.MinigameSawmill,
            MinigameType.Shooting => EffectType.MinigameShooting,
            MinigameType.Fishing => EffectType.MinigameFishing,
            MinigameType.Typewriter => EffectType.MinigameTypewritter,
            MinigameType.Memory => EffectType.MinigameMemory,
            _ => throw new ArgumentOutOfRangeException()
        };

        character.CurrentMinigame = (int)minigameEffect;
        e.Sender.BroadcastGuri(2, 1);
        e.Sender.SendMinigameStart(minigameConfiguration.Type);

        _minigameManager.RegisterInteraction(e.Sender, new MinilandInteractionInformationHolder(ThisAction, e.MinigameObject));
        
        e.Sender.EmitEventAsync(new IncreaseBattlePassObjectiveEvent(MissionType.PlayXMinilandGame));
        e.Sender.EmitEventAsync(new IncreaseBattlePassObjectiveEvent(MissionType.PlayMiniGameLevel, firstData: e.Sender.PlayerEntity.Level));
        return Task.CompletedTask;
    }
}