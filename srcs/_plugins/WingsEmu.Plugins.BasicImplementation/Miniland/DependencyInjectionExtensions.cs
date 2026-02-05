// NosEmu
// 


using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using PhoenixLib.Configuration;
using WingsEmu.Game._enum;
using WingsEmu.Game.Configurations.Miniland;

namespace NosEmu.Plugins.BasicImplementations.Miniland;

public static class DependencyInjectionExtensions
{
    public static void AddMinilandModule(this IServiceCollection services)
    {
        services.AddFileConfiguration(new MinilandConfiguration
        {
            new()
            {
                MapVnum = (int)MapIds.MINILAND,
                RestrictedZones = new List<RestrictedZone>
                {
                    new()
                    {
                        RestrictionTag = RestrictionType.OnlyMates,
                        Corner1 = new SerializablePosition { X = 2, Y = 7 },
                        Corner2 = new SerializablePosition { X = 17, Y = 8 }
                    }
                },
                ForcedPlacings = new List<ForcedPlacing>
                {
                    new()
                    {
                        SubType = MinilandItemSubType.HOUSE, ForcedLocation = new SerializablePosition
                        {
                            X = 24,
                            Y = 6
                        }
                    },
                    new()
                    {
                        SubType = MinilandItemSubType.SMALL_HOUSE, ForcedLocation = new SerializablePosition
                        {
                            X = 21,
                            Y = 4
                        }
                    },
                    new()
                    {
                        SubType = MinilandItemSubType.WAREHOUSE, ForcedLocation = new SerializablePosition
                        {
                            X = 31,
                            Y = 2
                        }
                    }
                }
            }
        });
        services.AddConfigurationsFromDirectory<MinigameScoresHolder>("minigame_scores");
        services.AddConfigurationsFromDirectory<Minigame>("minigames");

        services.AddFileConfiguration<GlobalMinigameConfiguration>();
        services.AddSingleton(s => new MinigameConfiguration
        {
            Minigames = s.GetRequiredService<IEnumerable<Minigame>>().ToList(),
            ScoresHolders = s.GetRequiredService<IEnumerable<MinigameScoresHolder>>().ToList(),
            Configuration = s.GetRequiredService<GlobalMinigameConfiguration>()
        });
    }
}