using System;

namespace WingsEmu.Game.SnackFood;

public class SnackProgress
{
    public int SnackHpBuffer { get; set; }
    public int SnackMpBuffer { get; set; }

    public int SnackHpBufferSize { get; set; }
    public int SnackMpBufferSize { get; set; }

    public int SnackSpBuffer { get; set; }
    public int SnackSpBufferSize { get; set; }

    public DateTime LastTick { get; set; }
}