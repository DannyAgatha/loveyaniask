using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Mapster;
using PhoenixLib.Events;
using WingsAPI.Game.Extensions.MinilandExtensions;
using WingsEmu.Game._i18n;
using WingsEmu.Game.Characters.Events;
using WingsEmu.Game.Configurations.Miniland;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Miniland;
using WingsEmu.Game.Miniland.Events;
using WingsEmu.Game.Networking;

namespace NosEmu.Plugins.BasicImplementations.Event.Miniland;

public class MinigameScoreEventHandler : IAsyncEventProcessor<MinigameScoreEvent>
{
    private readonly IGameLanguageService _languageService;
    private readonly MinigameConfiguration _minigameConfiguration;
    private readonly IMinigameManager _minigameManager;
    private readonly Dictionary<long, List<MinigameDone>> _minigamesDone = new();
    private readonly MinigameInteraction ThisAction = MinigameInteraction.DeclarateScore;

    public MinigameScoreEventHandler(MinigameConfiguration minigameConfiguration, IMinigameManager minigameManager, IGameLanguageService languageService)
    {
        _minigameConfiguration = minigameConfiguration;
        _minigameManager = minigameManager;
        _languageService = languageService;
    }

    public async Task HandleAsync(MinigameScoreEvent e, CancellationToken cancellation)
    {
        MinigameScoresHolder scores = _minigameManager.GetScores(e.MapObject.InventoryItem.ItemInstance.ItemVNum);

        ScoreHolder scoreHolder = GetScoreHolder(e, scores);

        MinilandInteractionInformationHolder minilandInteraction = _minigameManager.GetLastInteraction(e.Sender);

        if (minilandInteraction.Interaction != MinigameInteraction.DeclaratePlay && minilandInteraction.MapObject != e.MapObject)
        {
            _minigameManager.ReportInteractionIncoherence(e.Sender, minilandInteraction.Interaction, minilandInteraction.MapObject, ThisAction, e.MapObject);
            return;
        }

        bool noReward = false;

        ScoreValidity scoreValidity = CheckScoreValidity(e, scoreHolder, scores, minilandInteraction);
        switch (scoreValidity)
        {
            case ScoreValidity.Valid:
                break;
            case ScoreValidity.NotValid:
                if (!_minigameConfiguration.Configuration.AntiExploitConfiguration.GiveRewardsToPossibleFalsePositives)
                {
                    noReward = true;
                }

                break;
            case ScoreValidity.Abusive:
                noReward = true;
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        bool isProbableBot = IsProbableBot(e, minilandInteraction);
        if (isProbableBot)
        {
            if (!_minigameConfiguration.Configuration.AntiExploitConfiguration.GiveRewardsToPossibleFalsePositives)
            {
                noReward = true;
            }
        }

        Minigame minigameConfiguration = _minigameManager.GetSpecificMinigameConfiguration(e.MapObject.InventoryItem.ItemInstance.ItemVNum);
        await e.Sender.EmitEventAsync(new MinigameScoreLogEvent
        {
            OwnerId = e.MapObject.CharacterId,
            CompletionTime = DateTime.UtcNow.Subtract(minilandInteraction.TimeOfInteraction),
            MinigameVnum = e.MapObject.InventoryItem.ItemInstance.ItemVNum,
            MinigameType = minigameConfiguration.Type,
            Score1 = e.Score1,
            Score2 = e.Score2
        });

        if (e.Sender.PlayerEntity.MinilandPoint < _minigameConfiguration.Configuration.MinigamePointsCostPerMinigame)
        {
            e.Sender.SendInfo(_languageService.GetLanguage(GameDialogKey.MINILAND_INFO_NOT_ENOUGH_PRODUCTION_POINTS, e.Sender.UserLanguage));
            return;
        }

        if (noReward)
        {
            scoreHolder.RewardLevel = RewardLevel.NoReward;
        }

        if (scoreHolder.RewardLevel == RewardLevel.NoReward)
        {
            e.Sender.SendInfo(_languageService.GetLanguage(GameDialogKey.MINIGAME_INFO_NO_REWARD, e.Sender.UserLanguage));
            return;
        }

        List<MinigameRewards> rewards = _minigameManager.GetSpecificMinigameConfiguration(e.MapObject.InventoryItem.ItemInstance.ItemVNum).Rewards;
        MinigameRewards reward = rewards.FirstOrDefault(x => x.RewardLevel == scoreHolder.RewardLevel);
        if (reward != null && e.MapObject.InventoryItem.ItemInstance.DurabilityPoint < reward.DurabilityCost)
        {
            e.Sender.SendInfo(_languageService.GetLanguage(GameDialogKey.MINILAND_INFO_NOT_ENOUGH_DURABILITY_POINT, e.Sender.UserLanguage));
            return;
        }

        _minigameManager.RegisterInteraction(e.Sender, new MinilandInteractionInformationHolder(ThisAction, e.MapObject, (scoreHolder.RewardLevel, rewards)));
        e.Sender.SendMinigameRewardLevel(scoreHolder.RewardLevel);
    }

    /// <summary>
    ///     Returns true if it is valid.
    /// </summary>
    /// <returns></returns>
    private ScoreValidity CheckScoreValidity(MinigameScoreEvent e, ScoreHolder scoreHolder, MinigameScoresHolder minigameScoresHolder, MinilandInteractionInformationHolder minilandInteractionInfo)
    {
        long number1 = Math.Max(e.Score1, e.Score2);
        long number2 = Math.Min(e.Score1, e.Score2);
        long result = Math.Abs(number1 - number2);

        if (result > 10)
        {
            e.Sender.NotifyStrangeBehavior(StrangeBehaviorSeverity.ABUSING, $"Minigame score incoherence -> Score1: {e.Score1.ToString()} | Score2: {e.Score2.ToString()}");
            return ScoreValidity.Abusive;
        }

        TimeSpan timeInWhichTheMinigameWasDone = DateTime.UtcNow.Subtract(minilandInteractionInfo.TimeOfInteraction);

        if (scoreHolder.MinimumTimeOfCompletion <= timeInWhichTheMinigameWasDone)
        {
            return ScoreValidity.Valid;
        }

        if (timeInWhichTheMinigameWasDone <= scoreHolder.MinimumTimeOfCompletion * _minigameConfiguration.Configuration.AntiExploitConfiguration.MinigameAbuseDetectionThreshold)
        {
            e.Sender.NotifyStrangeBehavior(StrangeBehaviorSeverity.SEVERE_ABUSE,
                $"Minigame was done under the Abuse Detection Threshold -> MinigameType: {minigameScoresHolder.Type} | RewardLevel: {scoreHolder.RewardLevel}" +
                $" | TimeSpan of minigame's completion: {timeInWhichTheMinigameWasDone.ToString()}");
            return ScoreValidity.Abusive;
        }

        e.Sender.NotifyStrangeBehavior(StrangeBehaviorSeverity.ABUSING,
            $"Minigame was done under the Expected Minimum Time Of Completion -> MinigameType: {minigameScoresHolder.Type} | RewardLevel: {scoreHolder.RewardLevel}" +
            $" | TimeSpan of minigame's completion: {timeInWhichTheMinigameWasDone.ToString()}");
        return ScoreValidity.NotValid;
    }

    /// <summary>
    ///     Returns true if it is probably a bot.
    /// </summary>
    /// <returns></returns>
    private bool IsProbableBot(MinigameScoreEvent e, MinilandInteractionInformationHolder minilandInteractionInfo)
    {
        TimeSpan timeOfAllMinigameCompletion = DateTime.UtcNow.Subtract(minilandInteractionInfo.TimeOfInteraction);
        AddMinigameDone(e.Sender, e.Score1, timeOfAllMinigameCompletion);

        MinigameDone[] minigamesDone = GetMinigamesDone(e.Sender).ToArray();

        bool isBot = false;

        minigamesDone.Aggregate(timeOfAllMinigameCompletion, (current, minigameDone) => current.Add(minigameDone.TimeSpan));

        if (timeOfAllMinigameCompletion >= _minigameConfiguration.Configuration.AntiExploitConfiguration.CommonTimeExpendedInMinigamesPerDay)
        {
            e.Sender.NotifyStrangeBehavior(StrangeBehaviorSeverity.ABUSING,
                $"In 1 day {timeOfAllMinigameCompletion.ToString()} has been expended playing minigames have been played, possible bot.");
            isBot = true;
        }

        if (minigamesDone.Length < _minigameConfiguration.Configuration.AntiExploitConfiguration.UseSameScoreCheckAtXMinigames)
        {
            return isBot;
        }

        MinigameDone[] minigamesWithUniqueScore = minigamesDone.Distinct().ToArray();

        foreach (MinigameDone minigameDone in minigamesWithUniqueScore)
        {
            int count = minigamesDone.Count(m => m.Score == minigameDone.Score);

            if (count / Convert.ToDouble(minigamesDone.Length) < _minigameConfiguration.Configuration.AntiExploitConfiguration.PercentageForSameScoreCheck)
            {
                continue;
            }

            isBot = true;
            e.Sender.NotifyStrangeBehavior(StrangeBehaviorSeverity.ABUSING, $"In 1 day {count.ToString()} minigames have had the same score, possible bot.");
        }

        return isBot;
    }

    private ScoreHolder GetScoreHolder(MinigameScoreEvent e, MinigameScoresHolder scoresHolder)
    {
        ScoreHolder scores = scoresHolder.Scores.FirstOrDefault(x => x.ScoreRange.Minimum <= e.Score1 && e.Score1 <= x.ScoreRange.Maximum);
        return scores?.Adapt<ScoreHolder>();
    }

    private void AddMinigameDone(IClientSession session, long score, TimeSpan timeSpan)
    {
        var minigameDone = new MinigameDone
        {
            DateTime = DateTime.UtcNow,
            TimeSpan = timeSpan,
            Score = score
        };

        if (_minigamesDone.TryGetValue(session.PlayerEntity.Id, out List<MinigameDone> list))
        {
            list.Add(minigameDone);
            _minigamesDone[session.PlayerEntity.Id] = list;
            return;
        }

        _minigamesDone.TryAdd(session.PlayerEntity.Id, new List<MinigameDone>
        {
            minigameDone
        });
    }

    private IEnumerable<MinigameDone> GetMinigamesDone(IClientSession session)
    {
        if (!_minigamesDone.TryGetValue(session.PlayerEntity.Id, out List<MinigameDone> list))
        {
            return new List<MinigameDone>();
        }

        var modifiedList = list.Where(x => x.DateTime.Day < 1).ToList();
        _minigamesDone[session.PlayerEntity.Id] = modifiedList;
        return modifiedList;
    }
}

public class MinigameDone
{
    public DateTime DateTime { get; set; }

    public TimeSpan TimeSpan { get; set; }

    public long Score { get; set; }
    public static bool operator ==(MinigameDone a, MinigameDone b) => b != null && a != null && a.Score == b.Score;

    public static bool operator !=(MinigameDone a, MinigameDone b) => !(a == b);
}

public enum ScoreValidity
{
    Valid,
    NotValid,
    Abusive
}