using System;

namespace WingsEmu.Game.Managers;

public interface IRevivalManager
{
    Guid RegisterRevival(long id);

    bool UnregisterRevival(long id, Guid guid);

    bool UnregisterRevival(long id);

    void TryUnregisterRevival(long id);
}