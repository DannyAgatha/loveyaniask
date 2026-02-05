// NosEmu
// 


using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using WingsEmu.Game.Networking.Broadcasting;
using WingsEmu.Packets;

namespace WingsEmu.Game.Networking;

public abstract class SessionsContainer : IBroadcaster
{
    private static readonly IPacketSerializer Serializer = new PacketSerializer();

    private readonly ReaderWriterLockSlim _lock = new(LockRecursionPolicy.SupportsRecursion);
    private readonly ConcurrentQueue<string> _queue = new();

    private readonly List<IClientSession> _sessionsAll = new();

    private bool _disposed;

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
        _lock.EnterReadLock();
        try
        {
            foreach (IClientSession session in _sessionsAll)
            {
                session.SendPacket(await lambdaAsync(session));
            }
        }
        finally
        {
            _lock.ExitReadLock();
        }
    }

    public async Task BroadcastAsync(Func<IClientSession, Task<string>> generatePacketCallback, params IBroadcastRule[] rules)
    {
        _lock.EnterReadLock();
        try
        {
            foreach (IClientSession session in _sessionsAll)
            {
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
        finally
        {
            _lock.ExitReadLock();
        }
    }

    public void Broadcast<T>(T packet) where T : IPacket
    {
        Broadcast(Serializer.Serialize(packet));
    }

    protected void FlushPackets()
    {
        var packets = new List<string>();
        while (_queue.TryDequeue(out string packet))
        {
            packets.Add(packet);
        }

        _lock.EnterReadLock();
        try
        {
            foreach (IClientSession session in _sessionsAll)
            {
                session.SendPackets(packets);
            }
        }
        finally
        {
            _lock.ExitReadLock();
        }
    }

    public virtual void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        GC.SuppressFinalize(this);
        _disposed = true;
    }

    public virtual void RegisterSession(IClientSession session)
    {
        if (!session.HasSelectedCharacter)
        {
            return;
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
    }

    public virtual void UnregisterSession(IClientSession session)
    {
        _lock.EnterWriteLock();
        try
        {
            _sessionsAll.Remove(session);
        }
        finally
        {
            _lock.ExitWriteLock();
        }
    }
}