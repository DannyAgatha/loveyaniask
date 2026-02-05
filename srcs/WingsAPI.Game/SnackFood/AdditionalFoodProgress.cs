using System;

namespace WingsEmu.Game.SnackFood;

public class AdditionalFoodProgress
{
    public int HpCap { get; set; }
    public int MpCap { get; set; }

    public int FoodAdditionalHpBuffer { get; set; }
    public int FoodAdditionalMpBuffer { get; set; }

    public int FoodAdditionalHpBufferSize { get; set; }
    public int FoodAdditionalMpBufferSize { get; set; }

    public DateTime LastTick { get; set; }
}