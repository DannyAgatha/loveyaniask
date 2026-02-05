using System;

namespace WingsEmu.Game.SnackFood;

public class AdditionalSnackProgress
{
    public int HpCap { get; set; }
    public int MpCap { get; set; }

    public int SnackAdditionalHpBuffer { get; set; }
    public int SnackAdditionalMpBuffer { get; set; }

    public int SnackAdditionalHpBufferSize { get; set; }
    public int SnackAdditionalMpBufferSize { get; set; }

    public DateTime LastTick { get; set; }
}