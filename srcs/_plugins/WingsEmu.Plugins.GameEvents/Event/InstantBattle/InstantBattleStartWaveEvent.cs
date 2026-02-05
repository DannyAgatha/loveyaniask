using WingsEmu.Plugins.GameEvents.DataHolder;

namespace WingsEmu.Plugins.GameEvents.Event.InstantBattle
{
    public class InstantBattleStartWaveEvent : InstantBattleEvent
    {
        public InstantBattleStartWaveEvent(InstantBattleInstance instance, InstantBattleInstanceWave wave) : base(instance) => Wave = wave;

        public InstantBattleInstanceWave Wave { get; }
    }
}