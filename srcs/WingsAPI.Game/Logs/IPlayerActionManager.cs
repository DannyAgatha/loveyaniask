namespace WingsEmu.Game.Logs;

public interface IPlayerLogManager
{
    void AddLog<T>(T message) where T : IPlayerActionLog;
}