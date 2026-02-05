using System;
using Microsoft.Extensions.DependencyInjection;
using WingsAPI.Plugins;
using WingsEmu.Game;
using WingsEmu.Game._i18n;
using WingsEmu.Game.Battle;
using WingsEmu.Game.Buffs;
using WingsEmu.Game.Families;
using WingsEmu.Game.Managers;
using WingsEmu.Game.Managers.ServerData;
using WingsEmu.Game.Managers.StaticData;
using WingsEmu.Game.Maps;
using WingsEmu.Game.Mates;
using WingsEmu.Game.Miniland;

namespace NosEmu.Plugins.BasicImplementations;

public class GameManagerPlugin : IGamePlugin
{
    private readonly IServiceProvider _container;

    public GameManagerPlugin(IServiceProvider container) => _container = container;

    public string Name => nameof(GameManagerPlugin);

    public void OnLoad()
    {
        StaticMapManager.Initialize(_container.GetService<IMapManager>());
        StaticSessionManager.Initialize(_container.GetService<ISessionManager>());
        StaticCardsManager.Initialize(_container.GetService<ICardsManager>());
        StaticItemsManager.Initialize(_container.GetService<IItemsManager>());
        StaticNpcMonsterManager.Initialize(_container.GetService<INpcMonsterManager>());
        StaticDropManager.Initialize(_container.GetService<IDropManager>());
        StaticSkillsManager.Initialize(_container.GetService<ISkillsManager>());
        StaticMinilandManager.Initialize(_container.GetService<IMinilandManager>());
        StaticScriptedInstanceManager.Initialize(_container.GetService<IScriptedInstanceManager>());
        StaticRandomGenerator.Initialize(_container.GetService<IRandomGenerator>());
        StaticMateTransportFactory.Initialize(_container.GetService<IMateTransportFactory>());
        StaticGameLanguageService.Initialize(_container.GetService<IGameLanguageService>());
        StaticBuffFactory.Initialize(_container.GetRequiredService<IBuffFactory>());
        StaticSkillExecutor.Initialize(_container.GetService<ISkillExecutor>());
        StaticMeditationManager.Initialize(_container.GetService<IMeditationManager>());
        StaticFamilyManager.Initialize(_container.GetService<IFamilyManager>());
        StaticRankingManager.Initialize(_container.GetService<IRankingManager>());
    }
}