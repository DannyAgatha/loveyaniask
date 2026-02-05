// NosEmu
// 


namespace WingsEmu.Game.Networking.Broadcasting;

public interface IBroadcastRule
{
    /// <summary>
    ///     Tells whether or not the given session matches a broadcasting rule
    /// </summary>
    /// <param name="session"></param>
    /// <returns>true if the session should receive the packet</returns>
    bool Match(IClientSession session);
}