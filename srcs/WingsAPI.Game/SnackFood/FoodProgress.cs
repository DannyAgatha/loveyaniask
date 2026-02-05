using System;

namespace WingsEmu.Game.SnackFood;

public class FoodProgress
{
    public int FoodHpBuffer { get; set; }
    public int FoodMpBuffer { get; set; }

    public int FoodHpBufferSize { get; set; }
    public int FoodMpBufferSize { get; set; }

    public int FoodSpBuffer { get; set; }
    public int FoodSpBufferSize { get; set; }

    public int FoodMateMaxHpBuffer { get; set; }
    public int FoodMateMaxHpBufferSize { get; set; }

    public DateTime LastTick { get; set; }
    public int IncreaseTick { get; set; } = 1;
}