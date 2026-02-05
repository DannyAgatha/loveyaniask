using WingsEmu.Plugins.GameEvents.DataHolder;

namespace WingsEmu.Plugins.GameEvents.Event.InstantBattle
{
    public class InstantBattleCompleteEvent : InstantBattleEvent
    {
        public InstantBattleCompleteEvent(InstantBattleInstance instance) : base(instance)
        {
        }
    }
}