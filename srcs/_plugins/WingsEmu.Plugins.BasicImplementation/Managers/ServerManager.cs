// NosEmu
// 


using System.Threading;
using WingsAPI.Communication.ServerApi.Protocol;
using WingsAPI.Packets.Enums;
using WingsAPI.Scripting.ScriptManager;
using WingsEmu.Game._ECS;
using WingsEmu.Game._i18n;
using WingsEmu.Game.Arena;
using WingsEmu.Game.Battle;
using WingsEmu.Game.Configurations;
using WingsEmu.Game.Fish;
using WingsEmu.Game.Groups;
using WingsEmu.Game.Items;
using WingsEmu.Game.Managers;
using WingsEmu.Game.Managers.ServerData;
using WingsEmu.Game.Managers.StaticData;
using WingsEmu.Game.Maps;
using WingsEmu.Game.Quests;

namespace NosEmu.Plugins.BasicImplementations.Managers;

public class ServerManager : IServerManager
{
    private readonly IFishManager _fishManager;
    private readonly IGroupManager _groupManager;
    private readonly ITickManager _tickManager;
    private readonly ITeleporterManager _teleporterManager;
    private readonly IMapManager _mapManager;
    private readonly IItemsManager _itemManager;
    private readonly ICardsManager _cardManager;
    private readonly IDropManager _dropManager;
    private readonly INpcMonsterManager _npcMonsterManager;
    private readonly IRecipeManager _recipeManager;
    private readonly IShopManager _shopManager;
    private readonly ISkillsManager _skillManager;
    private readonly IMapNpcManager _mapNpcManager;
    private readonly IQuestManager _questManager;
    private readonly SerializableGameServer _gameServerInfos;
    private readonly GameRateConfiguration _rateConfiguration;
    private readonly IEvtbConfiguration _evtbConfiguration;
    private readonly GameMinMaxConfiguration _gameMinMaxConfiguration;
    private readonly IItemBoxManager _itemBoxManager;
    private readonly IRaidScriptManager _raidScriptManager;
    private readonly IDungeonScriptManager _dungeonScriptManager;
    private readonly IArenaManager _arenaManager;
    private readonly IMapMonsterManager _mapMonsterManager;
    private readonly ITimeSpaceScriptManager _timeSpaceScriptManager;
    private readonly IGameLanguageService _gameLanguageService;
    private readonly IRankingManager _rankingManager;
    private readonly IForbiddenNamesManager _forbiddenNamesManager;

    private CancellationTokenSource _shutdownTokenSource;

    public ServerManager(ITeleporterManager teleporterManager, IMapManager mapManager, IItemsManager itemManager, ICardsManager cardManager, IDropManager dropManager,
        INpcMonsterManager npcMonsterManager, IRecipeManager recipeManager, IShopManager shopManager, ISkillsManager skillManager, IMapNpcManager mapNpcManager, IQuestManager questManager,
        SerializableGameServer gameServerInfos, GameRateConfiguration rateConfiguration, GameMinMaxConfiguration gameMinMaxConfiguration,
        IGroupManager groupManager, ITickManager tickManager, IItemBoxManager itemBoxManager, IRaidScriptManager raidScriptManager, IDungeonScriptManager dungeonScriptManager,
        IArenaManager arenaManager, IMapMonsterManager mapMonsterManager, ITimeSpaceScriptManager timeSpaceScriptManager, IGameLanguageService gameLanguageService, IRankingManager rankingManager,
        IForbiddenNamesManager forbiddenNamesManager, IEvtbConfiguration evtbConfiguration, IFishManager fishManager)
    {
        _teleporterManager = teleporterManager;
        _mapManager = mapManager;
        _itemManager = itemManager;
        _cardManager = cardManager;
        _dropManager = dropManager;
        _npcMonsterManager = npcMonsterManager;
        _recipeManager = recipeManager;
        _shopManager = shopManager;
        _skillManager = skillManager;
        _mapNpcManager = mapNpcManager;
        _questManager = questManager;
        _gameServerInfos = gameServerInfos;
        _rateConfiguration = rateConfiguration;
        _evtbConfiguration = evtbConfiguration;
        _gameMinMaxConfiguration = gameMinMaxConfiguration;
        _groupManager = groupManager;
        _tickManager = tickManager;
        _itemBoxManager = itemBoxManager;
        _raidScriptManager = raidScriptManager;
        _dungeonScriptManager = dungeonScriptManager;
        _arenaManager = arenaManager;
        _mapMonsterManager = mapMonsterManager;
        _timeSpaceScriptManager = timeSpaceScriptManager;
        _gameLanguageService = gameLanguageService;
        _rankingManager = rankingManager;
        _forbiddenNamesManager = forbiddenNamesManager;
        _fishManager = fishManager;
    }
    public void InitializeAsync()
    {
        State = GameServerState.STARTING;
        InitializeConfigurations();
        _itemManager.Initialize();
        _skillManager.Initialize();
        _questManager.InitializeAsync().ConfigureAwait(false).GetAwaiter().GetResult();
        _dropManager.InitializeAsync().ConfigureAwait(false).GetAwaiter().GetResult();
        _npcMonsterManager.InitializeAsync().ConfigureAwait(false).GetAwaiter().GetResult();
        _mapMonsterManager.InitializeAsync().ConfigureAwait(false).GetAwaiter().GetResult();
        _shopManager.InitializeAsync().ConfigureAwait(false).GetAwaiter().GetResult();
        _recipeManager.InitializeAsync().ConfigureAwait(false).GetAwaiter().GetResult();
        _teleporterManager.InitializeAsync().ConfigureAwait(false).GetAwaiter().GetResult();
        _cardManager.Initialize();
        _mapNpcManager.InitializeAsync().ConfigureAwait(false).GetAwaiter().GetResult();
        _itemBoxManager.Initialize();
        _rankingManager.TryRefreshRanking().ConfigureAwait(false).GetAwaiter().GetResult();
        _mapManager.Initialize().ConfigureAwait(false).GetAwaiter().GetResult();
        _fishManager.Initialize().ConfigureAwait(false).GetAwaiter().GetResult();
        _arenaManager.Initialize();
        _raidScriptManager.Load();
        _dungeonScriptManager.Load();
        _timeSpaceScriptManager.Load();
        _tickManager.AddProcessable(_groupManager);
        _gameLanguageService.Reload(true).ConfigureAwait(false).GetAwaiter().GetResult();
        _forbiddenNamesManager.Reload().ConfigureAwait(false).GetAwaiter().GetResult();
    }
    
    public void Reload()
    {
        InitializeConfigurations();
        _itemBoxManager.Initialize();
        _raidScriptManager.Load();
        _dungeonScriptManager.Load();
    }

    public void TryStart()
    {
        if (IsRunning)
        {
            return;
        }

        State = GameServerState.RUNNING;
        IsRunning = true;
        _tickManager.Start();
    }

    public void PutIdle()
    {
        if (IsRunning == false)
        {
            State = GameServerState.IDLE;
            return;
        }

        State = GameServerState.IDLE;
        IsRunning = false;
        _tickManager.Stop();
    }

    public void Shutdown()
    {
        InShutdown = true;
        State = GameServerState.STOPPING;
        _shutdownTokenSource?.Cancel();
    }


    public void ListenCancellation(CancellationTokenSource stopServiceTokenSource)
    {
        _shutdownTokenSource = stopServiceTokenSource;
    }

    private void InitializeConfigurations()
    {
        MobXpRate = _rateConfiguration.MobXpRate;
        JobXpRate = _rateConfiguration.JobXpRate;
        HeroXpRate = _rateConfiguration.HeroXpRate;
        FairyXpRate = _rateConfiguration.FairyXpRate;
        MateXpRate = _rateConfiguration.MateXpRate;
        PartnerXpRate = _rateConfiguration.PartnerXpRate;
        ReputRate = _rateConfiguration.ReputRate;
        MobDropRate = _rateConfiguration.MobDropRate;
        MobDropChance = _rateConfiguration.MobDropChance;
        FamilyExpRate = _rateConfiguration.FamilyXpRate;
        GoldDropRate = _rateConfiguration.GoldDropRate;
        GoldRate = _rateConfiguration.GoldRate;
        GoldDropChance = _rateConfiguration.GoldDropChance;
        GenericDropRate = _rateConfiguration.GenericDropRate;
        GenericDropChance = _rateConfiguration.GenericDropChance;

        /*
         * Min Max Configurations
         */
        MaxLevel = _gameMinMaxConfiguration.MaxLevel;
        MaxMateLevel = _gameMinMaxConfiguration.MaxMateLevel;
        MaxJobLevel = _gameMinMaxConfiguration.MaxJobLevel;
        MaxSpLevel = _gameMinMaxConfiguration.MaxSpLevel;
        MaxHeroLevel = _gameMinMaxConfiguration.MaxHeroLevel;
        HeroicStartLevel = _gameMinMaxConfiguration.HeroMinLevel;
        MaxGold = _gameMinMaxConfiguration.MaxGold;
        MaxBankGold = _gameMinMaxConfiguration.MaxBankGold;
        MaxNpcTalkRange = _gameMinMaxConfiguration.MaxNpcTalkRange;
        MaxBasicSpPoints = _gameMinMaxConfiguration.MaxSpBasePoints;
        MaxAdditionalSpPoints = _gameMinMaxConfiguration.MaxSpAdditionalPoints;
    }

    public GameServerState State { get; private set; }

    public bool IsRunning { get; private set; }

    public int ChannelId => _gameServerInfos.ChannelId;

    public int MobDropRate { get; set; }

    public int MobDropChance { get; set; }

    public int FamilyExpRate { get; set; }

    public int JobXpRate { get; set; }

    public bool ExpEvent { get; set; }

    public int FairyXpRate { get; set; }

    public int GoldDropRate { get; set; }

    public int GoldRate { get; set; }

    public int GoldDropChance { get; set; }

    public int GenericDropRate { get; set; }

    public int GenericDropChance { get; set; }

    public int ReputRate { get; set; }

    public int HeroicStartLevel { get; set; }

    public int HeroXpRate { get; set; }

    public long MaxGold { get; set; }

    public long MaxBankGold { get; set; }

    public short MaxHeroLevel { get; set; }

    public short MaxJobLevel { get; set; }

    public short MaxLevel { get; set; }

    public short MaxSpLevel { get; set; }

    public int MateXpRate { get; set; }

    public int PartnerXpRate { get; set; }

    public short MaxMateLevel { get; set; }

    public short MaxNpcTalkRange { get; set; }
    public int MaxBasicSpPoints { get; set; }

    public int MaxAdditionalSpPoints { get; set; }

    public string ServerGroup => _gameServerInfos.WorldGroup;

    public int MobXpRate { get; set; }

    public int AccountLimit => _gameServerInfos.AccountLimit;

    public bool InShutdown { get; set; }
}