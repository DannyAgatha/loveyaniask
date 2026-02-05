using System.Collections.Generic;

namespace WingsEmu.Game.Configurations.SetEffect;

public class SetEffect
{
    public List<int> ItemVnums { get; set; }
    public List<int> BuffIds { get; set; }
}

public class SetEffectConfiguration
{
    public List<SetEffect> SetEffects { get; set; } = [];
}
