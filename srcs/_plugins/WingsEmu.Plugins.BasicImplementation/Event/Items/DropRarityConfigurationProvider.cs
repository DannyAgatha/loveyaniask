using System.Collections.Generic;
using System.Linq;
using WingsEmu.Game;
using WingsEmu.Game.Items;
using WingsEmu.Packets.Enums;

namespace NosEmu.Plugins.BasicImplementations.Event.Items;

public class DropRarityConfigurationProvider : IDropRarityConfigurationProvider
{
    private readonly List<RarityChance> _equipment;
    private readonly IRandomGenerator _randomGenerator;
    private readonly List<RarityChance> _shells;

    public DropRarityConfigurationProvider(DropRarityConfiguration dropRarityConfiguration, IRandomGenerator randomGenerator)
    {
        _randomGenerator = randomGenerator;
        _equipment = dropRarityConfiguration.Equipment?.OrderBy(s => s.Chance).ToList();
        _shells = dropRarityConfiguration.Shells?.OrderBy(s => s.Chance).ToList();
    }

    public sbyte GetRandomRarity(ItemType itemType)
    {
        if (itemType != ItemType.Weapon && itemType != ItemType.Armor && itemType != ItemType.Shell)
        {
            return 0;
        }

        List<RarityChance> rarities = itemType == ItemType.Shell ? _shells : _equipment;
        var randomBag = new RandomBag<RarityChance>(_randomGenerator);

        foreach (RarityChance rarity in rarities)
        {
            randomBag.AddEntry(rarity, rarity.Chance);
        }

        return randomBag.GetRandom().Rarity;
    }
}