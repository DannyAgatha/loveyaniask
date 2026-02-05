using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using PhoenixLib.Caching;
using PhoenixLib.Logging;
using WingsAPI.Communication;
using WingsAPI.Communication.DbServer.CharacterService;
using WingsAPI.Communication.Player;
using WingsAPI.Data.Character;
using WingsEmu.Game.Managers;
using WingsEmu.Game.Networking;
using WingsEmu.Game.Networking.Broadcasting;
using WingsEmu.Packets;

namespace NosEmu.Plugins.BasicImplementations.Managers;

public class SessionManager : ISessionManager
{
    private static readonly IPacketSerializer Serializer = new PacketSerializer();

    private readonly ILongKeyCachedRepository<ClusterCharacterInfo> _characterCache;
    private readonly ConcurrentDictionary<string, ClusterCharacterInfo> _characterCacheByName = new();
    private readonly ICharacterService _characterService;
    private readonly ConcurrentDictionary<long, bool> _isOnlineByCharacterId = new();
    private readonly ConcurrentDictionary<string, bool> _isOnlineByCharacterName = new();
    private readonly ReaderWriterLockSlim _lock = new();
    private readonly ConcurrentQueue<string> _queue = new();
    private readonly IServerManager _serverManager;
    private readonly List<IClientSession> _sessionsAll = new();
    private readonly ConcurrentDictionary<long, IClientSession> _sessionsByCharacterId = new();
    private readonly ConcurrentDictionary<string, IClientSession> _sessionsByCharacterName = new();

    public SessionManager(ILongKeyCachedRepository<ClusterCharacterInfo> characterCache, IServerManager serverManager, ICharacterService characterService)
    {
        _characterCache = characterCache;
        _serverManager = serverManager;
        _characterService = characterService;
    }

    private IReadOnlyDictionary<long, IClientSession> SessionsByCharacterId => _sessionsByCharacterId;
    private IReadOnlyDictionary<string, IClientSession> SessionsByCharacterName => _sessionsByCharacterName;

    public IReadOnlyList<IClientSession> Sessions
    {
        get
        {
            _lock.EnterReadLock();
            try
            {
                return _sessionsAll.ToArray();
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }
    }

    public int SessionsCount => _sessionsByCharacterId.Count;

    public async ValueTask<ClusterCharacterInfo> GetOnlineCharacterById(long characterId)
    {
        return await _characterCache.GetOrSetAsync(characterId, () => FetchDelegate(characterId), TimeSpan.FromHours(2));
    }

    public ClusterCharacterInfo GetOnlineCharacterByName(string characterName) => _characterCacheByName.TryGetValue(characterName, out ClusterCharacterInfo characterInfo) ? characterInfo : null;

    public bool IsOnline(string charName) => _isOnlineByCharacterName.ContainsKey(charName);

    public bool IsOnline(long characterId) => _isOnlineByCharacterId.ContainsKey(characterId);

    public void AddOnline(ClusterCharacterInfo clusterCharacterInfo)
    {
        long characterId = clusterCharacterInfo.Id;
        string charName = clusterCharacterInfo.Name;
        _isOnlineByCharacterId[characterId] = true;
        _isOnlineByCharacterName[charName] = true;
        _characterCache.Set(clusterCharacterInfo.Id, clusterCharacterInfo, TimeSpan.FromHours(2));
        _characterCacheByName[charName] = clusterCharacterInfo;
    }

    public void RemoveOnline(string charName, long characterId)
    {
        _isOnlineByCharacterId.Remove(characterId, out _);
        _isOnlineByCharacterName.Remove(charName, out _);
        _characterCache.Remove(characterId);
        _characterCacheByName.Remove(charName, out _);
    }

    public async Task DisconnectAllAsync()
    {
        foreach (IClientSession session in Sessions.ToArray())
        {
            if (session == null)
            {
                continue;
            }


            if (session.HasSelectedCharacter)
            {
                if (session.PlayerEntity.Hp < 1)
                {
                    session.PlayerEntity.Hp = 1;
                }
            }

            Log.Info($"[SESSION_DISCONNECT] {session.SessionId}:{session.PlayerEntity?.Name}");
            session.ForceDisconnect();
        }
    }

    public IClientSession GetSessionByCharacterName(string name) => SessionsByCharacterName.TryGetValue(name, out IClientSession session) ? session : null;

    public async Task KickAsync(string characterName)
    {
        IClientSession session = GetSessionByCharacterName(characterName);
        session?.ForceDisconnect();
    }

    public async Task KickAsync(long accountId)
    {
        IClientSession session = Sessions.FirstOrDefault(s => s.Account != null && s.Account.Id == accountId);
        session?.ForceDisconnect();
    }

    public IClientSession GetSessionByCharacterId(long characterId) => SessionsByCharacterId.TryGetValue(characterId, out IClientSession session) ? session : null;

    public void Broadcast(string packet)
    {
        _queue.Enqueue(packet);
    }

    public void Broadcast<T>(T packet, params IBroadcastRule[] rules) where T : IServerPacket
    {
        _lock.EnterReadLock();
        try
        {
            foreach (IClientSession session in _sessionsAll)
            {
                if (session is null)
                {
                    continue;
                }

                bool all = true;
                foreach (IBroadcastRule x in rules)
                {
                    if (!x.Match(session))
                    {
                        all = false;
                        break;
                    }
                }

                if (!rules.Any() || all)
                {
                    session.SendPacket(packet);
                }
            }
        }
        finally
        {
            _lock.ExitReadLock();
        }
    }

    public void Broadcast(string packet, params IBroadcastRule[] rules)
    {
        _lock.EnterReadLock();
        try
        {
            foreach (IClientSession session in _sessionsAll)
            {
                if (session is null)
                {
                    continue;
                }

                bool all = true;
                foreach (IBroadcastRule x in rules)
                {
                    if (!x.Match(session))
                    {
                        all = false;
                        break;
                    }
                }

                if (!rules.Any() || all)
                {
                    session.SendPacket(packet);
                }
            }
        }
        finally
        {
            _lock.ExitReadLock();
        }
    }

    public void Broadcast(IEnumerable<string> packets)
    {
        foreach (string packet in packets)
        {
            _queue.Enqueue(packet);
        }
    }

    public void Broadcast(IEnumerable<string> packets, params IBroadcastRule[] rules)
    {
        _lock.EnterReadLock();
        try
        {
            foreach (IClientSession session in _sessionsAll)
            {
                if (session is null)
                {
                    continue;
                }

                bool all = true;
                foreach (IBroadcastRule x in rules)
                {
                    if (!x.Match(session))
                    {
                        all = false;
                        break;
                    }
                }

                if (!rules.Any() || all)
                {
                    session.SendPackets(packets);
                }
            }
        }
        finally
        {
            _lock.ExitReadLock();
        }
    }

    public void Broadcast(Func<IClientSession, string> generatePacketCallback)
    {
        _lock.EnterReadLock();
        try
        {
            foreach (IClientSession session in _sessionsAll)
            {
                if (session is null)
                {
                    continue;
                }

                session.SendPacket(generatePacketCallback(session));
            }
        }
        finally
        {
            _lock.ExitReadLock();
        }
    }

    public void Broadcast(Func<IClientSession, string> generatePacketCallback, params IBroadcastRule[] rules)
    {
        _lock.EnterReadLock();
        try
        {
            foreach (IClientSession session in _sessionsAll)
            {
                if (session is null)
                {
                    continue;
                }

                bool all = true;
                foreach (IBroadcastRule x in rules)
                {
                    if (x.Match(session))
                    {
                        continue;
                    }

                    all = false;
                    break;
                }

                if (!rules.Any() || all)
                {
                    session.SendPacket(generatePacketCallback(session));
                }
            }
        }
        finally
        {
            _lock.ExitReadLock();
        }
    }

    public async Task BroadcastAsync(Func<IClientSession, Task<string>> lambdaAsync)
    {
        foreach (IClientSession session in Sessions)
        {
            if (session is null)
            {
                continue;
            }

            session.SendPacket(await lambdaAsync(session));
        }
    }

    public async Task BroadcastAsync(Func<IClientSession, Task<string>> generatePacketCallback, params IBroadcastRule[] rules)
    {
        foreach (IClientSession session in Sessions)
        {
            if (session is null)
            {
                continue;
            }

            bool all = true;
            foreach (IBroadcastRule x in rules)
            {
                if (!x.Match(session))
                {
                    all = false;
                    break;
                }
            }

            if (!rules.Any() || all)
            {
                session.SendPacket(await generatePacketCallback(session));
            }
        }
    }

    public void Broadcast<T>(T packet) where T : IPacket
    {
        Broadcast(Serializer.Serialize(packet));
    }

    public virtual void RegisterSession(IClientSession session)
    {
        if (!session.HasSelectedCharacter)
        {
            return;
        }

        long id = session.PlayerEntity.Id;
        string name = session.PlayerEntity.Name;

        if (!_sessionsByCharacterId.TryGetValue(id, out _))
        {
            _sessionsByCharacterId.TryAdd(session.PlayerEntity.Id, session);
        }
        else
        {
            _sessionsByCharacterId.TryRemove(session.PlayerEntity.Id, out _);
            _sessionsByCharacterId.TryAdd(session.PlayerEntity.Id, session);
        }

        if (!_sessionsByCharacterName.TryGetValue(name, out _))
        {
            _sessionsByCharacterName.TryAdd(session.PlayerEntity.Name, session);
        }
        else
        {
            _sessionsByCharacterName.TryRemove(session.PlayerEntity.Name, out _);
            _sessionsByCharacterName.TryAdd(session.PlayerEntity.Name, session);
        }

        _lock.EnterWriteLock();
        try
        {
            _sessionsAll.Add(session);
        }
        finally
        {
            _lock.ExitWriteLock();
        }

        _serverManager.TryStart();
    }

    public virtual void UnregisterSession(IClientSession session)
    {
        _sessionsByCharacterId.TryRemove(session.PlayerEntity.Id, out _);
        _sessionsByCharacterName.TryRemove(session.PlayerEntity.Name, out _);

        _lock.EnterWriteLock();
        try
        {
            _sessionsAll.Remove(session);
        }
        finally
        {
            _lock.ExitWriteLock();
        }

        if (_sessionsByCharacterId.IsEmpty)
        {
            _serverManager.PutIdle();
        }
    }

    private async Task<ClusterCharacterInfo> FetchDelegate(long characterId)
    {
        DbServerGetCharacterResponse response = null;
        try
        {
            response = await _characterService.GetCharacterById(new DbServerGetCharacterByIdRequest
            {
                CharacterId = characterId
            });
        }
        catch (Exception e)
        {
            Log.Error("[SESSION_MANAGER] Unexpected error: ", e);
        }

        if (response?.RpcResponseType == RpcResponseType.GENERIC_SERVER_ERROR)
        {
            Log.Error("[SESSION_MANAGER] FamilyManager", new InvalidOperationException($"Database corrupted: {characterId.ToString()} seems to have been removed but still used somewhere"));
        }

        if (response?.RpcResponseType != RpcResponseType.SUCCESS)
        {
            return null;
        }

        CharacterDTO characterDto = response.CharacterDto;

        return new ClusterCharacterInfo
        {
            Id = characterId,
            Name = characterDto.Name,
            Gender = characterDto.Gender,
            Class = characterDto.Class,
            SubClass = characterDto.SubClass,
            TierLevel = characterDto.TierLevel,
            Level = characterDto.Level,
            HeroLevel = characterDto.HeroLevel,
            MorphId = null,
            ChannelId = null
        };
    }
}