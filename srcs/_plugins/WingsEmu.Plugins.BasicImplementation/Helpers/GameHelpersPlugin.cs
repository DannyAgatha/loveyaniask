using System;
using Microsoft.Extensions.DependencyInjection;
using WingsAPI.Plugins;
using WingsEmu.Game.Algorithm;
using WingsEmu.Game.Buffs;
using WingsEmu.Game.Configurations;

namespace NosEmu.Plugins.BasicImplementations;

public class GameHelpersPlugin : IGamePlugin
{
    private readonly IServiceProvider _services;

    public GameHelpersPlugin(IServiceProvider services) => _services = services;

    public string Name => nameof(GameHelpersPlugin);

    public void OnLoad()
    {
        StaticCharacterAlgorithmService.Initialize(_services.GetService<ICharacterAlgorithm>());
        StaticBattleEntityAlgorithmService.Initialize(_services.GetService<IBattleEntityAlgorithmService>());
        StaticBCardEffectHandlerService.Initialize(_services.GetService<IBCardEffectHandlerContainer>());
        StaticTrainerSpecialistPetSkillsLearningConfiguration.Initialize(_services.GetService<TrainerSpecialistPetSkillsLearningConfiguration>());
        StaticGeneralServerConfiguration.Initialize(_services.GetService<GeneralServerConfiguration>());
    }
}