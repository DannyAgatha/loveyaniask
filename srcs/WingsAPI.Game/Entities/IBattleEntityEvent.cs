using PhoenixLib.Events;

namespace WingsEmu.Game.Entities;

public interface IBattleEntityEvent : IAsyncEvent
{
    IBattleEntity Entity { get; }
}