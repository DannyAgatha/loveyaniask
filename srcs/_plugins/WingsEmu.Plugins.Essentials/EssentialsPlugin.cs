using WingsAPI.Plugins;
using WingsEmu.Commands.Interfaces;
using WingsEmu.Commands.TypeParsers;
using WingsEmu.Game.Managers;
using WingsEmu.Game.Managers.StaticData;
using WingsEmu.Game.Maps;
using WingsEmu.Plugins.Essentials.Account;
using WingsEmu.Plugins.Essentials.Administrator;
using WingsEmu.Plugins.Essentials.Administrator.Items;
using WingsEmu.Plugins.Essentials.GameMaster;
using WingsEmu.Plugins.Essentials.God;
using WingsEmu.Plugins.Essentials.GS;
using WingsEmu.Plugins.Essentials.Help;
using WingsEmu.Plugins.Essentials.NPC;
using WingsEmu.Plugins.Essentials.Skills;
using WingsEmu.Plugins.Essentials.Teleport;

namespace WingsEmu.Plugins.Essentials;

public class EssentialsPlugin : IGamePlugin
{
    private readonly ICommandContainer _commands;
    private readonly IItemsManager _itemManager;
    private readonly IMapManager _mapManager;
    private readonly ISessionManager _sessionManager;

    public EssentialsPlugin(ICommandContainer commandContainer, IMapManager mapManager, ISessionManager sessionManager, IItemsManager itemManager)
    {
        _itemManager = itemManager;
        _sessionManager = sessionManager;
        _mapManager = mapManager;
        _commands = commandContainer;
    }

    public string Name => nameof(EssentialsPlugin);

    public void OnLoad()
    {
        _commands.AddTypeParser(new PlayerEntityTypeParser(_sessionManager));
        _commands.AddTypeParser(new MapInstanceTypeParser(_mapManager));
        _commands.AddTypeParser(new ItemTypeParser(_itemManager));
        // admin
        _commands.AddModule<AdministratorLanguageModule>();
        _commands.AddModule<GodSetRankModule>();
        _commands.AddModule<AdministratorCheatModule_Rune>();
        _commands.AddModule<AdministratorCheatModule_Shell>();
        _commands.AddModule<AdministratorModule>();
        _commands.AddModule<AdministratorMaintenanceModule>();
        _commands.AddModule<AdministratorCooldownModule>();
        _commands.AddModule<RefundModule>();
        _commands.AddModule<GsCommandModule>();

        // item management
        _commands.AddModule<AdministratorItemManagementModule>();
        _commands.AddModule<SpecialistModule>();
        _commands.AddModule<MonsterSummoningModule>();

        // bazaar
        _commands.AddModule<AdministratorBazaarModule>();

        // inventory

        // super game master
        _commands.AddModule<SuperGameMasterMonsterModule>();
        _commands.AddModule<PunishmentModule>();

        // character
        _commands.AddModule<CharacterModule>();
        _commands.AddModule<MinilandModule>();
        _commands.AddModule<TeleportModule>();
        _commands.AddModule<AccountModule>();
        _commands.AddModule<HelpModule>();
        _commands.AddModule<BetaGameTester>();
        _commands.AddModule<SearchDataModule>();
        _commands.AddModule<MateCreationModule>();
        _commands.AddModule<NPCModule>();
        _commands.AddModule<AdministratorCheatModule_Skills>();
        _commands.AddModule<AdministratorMailModule>();
        _commands.AddModule<ItemModule>();
        _commands.AddModule<SkillsModule>();
    }
}