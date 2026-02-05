using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PhoenixLib.DAL.Redis.Locks;
using WingsEmu.Game.Characters.Events;
using WingsEmu.Game.Configurations.Miniland;
using WingsEmu.Game.Miniland;
using WingsEmu.Game.Networking;

namespace NosEmu.Plugins.BasicImplementations.Managers;

public class MinigameManager : IMinigameManager
{
    private readonly IExpirableLockService _lockService;
    private readonly MinigameConfiguration _minigameConfiguration;
    private readonly Dictionary<long, MinilandInteractionInformationHolder> _minigameInteractions = new();

    public MinigameManager(MinigameConfiguration minigameConfiguration, IExpirableLockService lockService)
    {
        _minigameConfiguration = minigameConfiguration;
        _lockService = lockService;
    }

    public async Task<bool> CanRefreshMinigamesFreeProductionPoints(long characterId) =>
        await _lockService.TryAddTemporaryLockAsync($"game:locks:minigame-refresh:{characterId}", DateTime.UtcNow.Date.AddDays(1));


    public MinigameScoresHolder GetScores(int minigameVnum)
    {
        Minigame minigame = GetSpecificMinigameConfiguration(minigameVnum);

        MinigameScoresHolder minigameScoresHolder = _minigameConfiguration.ScoresHolders.FirstOrDefault(m => m.Type == minigame.Type);

        return minigameScoresHolder;
    }

    public Minigame GetSpecificMinigameConfiguration(int minigameVnum)
    {
        Minigame minigame = _minigameConfiguration.Minigames.FirstOrDefault(m => m.Vnum == minigameVnum);

        return minigame;
    }

    public void RegisterInteraction(IClientSession session, MinilandInteractionInformationHolder minilandInteraction)
    {
        if (_minigameInteractions.TryAdd(session.PlayerEntity.Id, minilandInteraction))
        {
            return;
        }

        _minigameInteractions[session.PlayerEntity.Id] = minilandInteraction;
    }

    public void ReportInteractionIncoherence(IClientSession session, MinigameInteraction lastInteraction, MapDesignObject lastMapObject, MinigameInteraction actualInteraction,
        MapDesignObject actualMapObject)
    {
        session.NotifyStrangeBehavior(StrangeBehaviorSeverity.NORMAL,
            $"Incoherence between minigame interactions detected - 'LastAction': {lastInteraction} | 'LastMapObjectSlot': {lastMapObject.InventoryItem.Slot.ToString()}" +
            $" | 'ActionSent': {actualInteraction} | 'ActualMapObjectSlot': {actualMapObject.InventoryItem.Slot.ToString()}");
    }

    public MinilandInteractionInformationHolder GetLastInteraction(IClientSession session)
        => _minigameInteractions.TryGetValue(session.PlayerEntity.Id, out MinilandInteractionInformationHolder interaction)
            ? interaction
            : new MinilandInteractionInformationHolder(MinigameInteraction.None, default);
}