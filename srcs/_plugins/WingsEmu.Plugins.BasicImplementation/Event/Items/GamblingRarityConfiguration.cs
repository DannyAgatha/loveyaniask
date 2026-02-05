using System.Collections.Generic;
using System.Linq;
using WingsEmu.Game;

namespace NosEmu.Plugins.BasicImplementations.Event.Items;

public interface IGamblingRarityConfiguration
{
    RaritySuccess GetRaritySuccess(byte fromRarity);
    short GetRandomRarity();
}

public class GamblingRarityConfiguration : IGamblingRarityConfiguration
{
    private readonly List<RaritySuccess> _gamblingSuccess;
    private readonly RandomBag<RarityChance> _randomRarities;

    public GamblingRarityConfiguration(GamblingRarityInfo gamblingRarityInfo, IRandomGenerator randomGenerator)
    {
        _gamblingSuccess = gamblingRarityInfo.GamblingSuccess;
        var gamblingRarities = gamblingRarityInfo.GamblingRarities.OrderBy(s => s.Chance).ToList();
        _randomRarities = new RandomBag<RarityChance>(randomGenerator);

        foreach (RarityChance rarity in gamblingRarities)
        {
            _randomRarities.AddEntry(rarity, rarity.Chance);
        }
    }

    public RaritySuccess GetRaritySuccess(byte fromRarity) => _gamblingSuccess.FirstOrDefault(s => s.FromRarity == fromRarity);

    public short GetRandomRarity() => _randomRarities.GetRandom().Rarity;
}