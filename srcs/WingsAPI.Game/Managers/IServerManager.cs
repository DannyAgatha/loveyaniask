using System.Threading;

namespace WingsEmu.Game.Managers;

public class StaticServerManager
{
    public static IServerManager Instance { get; private set; }

    public static void Initialize(IServerManager instance)
    {
        Instance = instance;
    }
}

public enum GameServerState
{
    ERROR,
    STARTING,
    RUNNING,
    IDLE,
    STOPPING
}

public interface IServerManager
{
    GameServerState State { get; }
    bool IsRunning { get; }
    int ChannelId { get; }
    int MobDropRate { get; }
    int MobDropChance { get; }
    int FamilyExpRate { get; }
    bool ExpEvent { get; set; }
    int FairyXpRate { get; set; }
    int GoldDropRate { get; set; }
    int GoldRate { get; set; }
    int GoldDropChance { get; set; }
    int GenericDropRate { get; set; }
    int GenericDropChance { get; set; }
    int ReputRate { get; set; }
    int HeroicStartLevel { get; set; }
    int HeroXpRate { get; set; }
    long MaxGold { get; set; }
    long MaxBankGold { get; set; }
    short MaxHeroLevel { get; set; }
    short MaxJobLevel { get; set; }
    short MaxLevel { get; set; }
    short MaxSpLevel { get; set; }
    int MateXpRate { get; set; }
    int PartnerXpRate { get; set; }
    short MaxMateLevel { get; set; }
    short MaxNpcTalkRange { get; set; }
    int MaxBasicSpPoints { get; set; }
    int MaxAdditionalSpPoints { get; set; }
    string ServerGroup { get; }
    int MobXpRate { get; set; }
    int AccountLimit { get; }
    bool InShutdown { get; }
    int JobXpRate { get; set; }
    void InitializeAsync();
    void ListenCancellation(CancellationTokenSource stopServiceTokenSource);
    void TryStart();
    void PutIdle();
    void Shutdown();
    void Reload();
}