using System.Threading.Tasks;
using WingsAPI.Communication.Player;
using WingsEmu.Game.Networking;

namespace WingsEmu.Game.Managers;

public class StaticSessionManager
{
    public static ISessionManager Instance { get; private set; }

    public static void Initialize(ISessionManager manager)
    {
        Instance = manager;
    }
}

public interface ISessionManager : IBroadcaster
{
    int SessionsCount { get; }

    ValueTask<ClusterCharacterInfo> GetOnlineCharacterById(long characterId);
    ClusterCharacterInfo GetOnlineCharacterByName(string characterName);
    bool IsOnline(string charName);
    bool IsOnline(long characterId);
    void AddOnline(ClusterCharacterInfo clusterCharacterInfo);
    void RemoveOnline(string charName, long characterId);

    /// <summary>
    ///     Disconnects all sessions from the current channel
    /// </summary>
    /// <returns></returns>
    Task DisconnectAllAsync();

    /// <summary>
    ///     Returns the IClientSession specified
    ///     by the character name passed as parameter
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    IClientSession GetSessionByCharacterName(string name);

    /// <summary>
    ///     Kicks a player using the character name
    ///     passed as parameter
    /// </summary>
    /// <param name="characterName"></param>
    /// <returns></returns>
    Task KickAsync(string characterName);

    /// <summary>
    ///     Kicks a player using the account id
    ///     passed as parameter
    /// </summary>
    /// <param name="accountId"></param>
    /// <returns></returns>
    Task KickAsync(long accountId);

    /// <summary>
    ///     Returns the IClientSession based on the CharacterId provided as parameter
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    IClientSession GetSessionByCharacterId(long id);

    /// <summary>
    ///     Registers a new session
    /// </summary>
    /// <param name="session"></param>
    void RegisterSession(IClientSession session);

    /// <summary>
    ///     Unregisters a session
    /// </summary>
    /// <param name="session"></param>
    void UnregisterSession(IClientSession session);
}