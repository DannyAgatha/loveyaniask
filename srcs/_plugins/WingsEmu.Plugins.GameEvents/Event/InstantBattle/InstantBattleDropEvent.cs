using WingsEmu.Plugins.GameEvents.Configuration.InstantBattle;
using WingsEmu.Plugins.GameEvents.DataHolder;

namespace WingsEmu.Plugins.GameEvents.Event.InstantBattle
{
    public class InstantBattleDropEvent : InstantBattleEvent
    {
        public InstantBattleDropEvent(InstantBattleInstance instance, InstantBattleWave wave) : base(instance) => Wave = wave;

        public InstantBattleWave Wave { get; }
    }
}