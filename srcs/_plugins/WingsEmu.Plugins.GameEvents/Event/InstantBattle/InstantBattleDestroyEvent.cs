using WingsEmu.Plugins.GameEvents.DataHolder;

namespace WingsEmu.Plugins.GameEvents.Event.InstantBattle
{
    public class InstantBattleDestroyEvent : InstantBattleEvent
    {
        public InstantBattleDestroyEvent(InstantBattleInstance instance) : base(instance)
        {
        }
    }
}