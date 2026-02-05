using PhoenixLib.Events;
using WingsEmu.Plugins.GameEvents.DataHolder;

namespace WingsEmu.Plugins.GameEvents.Event.InstantBattle
{
    public abstract class InstantBattleEvent : IAsyncEvent
    {
        protected InstantBattleEvent(InstantBattleInstance instance) => Instance = instance;

        public InstantBattleInstance Instance { get; }
    }
}