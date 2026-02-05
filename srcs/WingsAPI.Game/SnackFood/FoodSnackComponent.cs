using System;
using WingsEmu.Game.Items;

namespace WingsEmu.Game.SnackFood;

public class FoodSnackComponent : IFoodSnackComponent
{
    private readonly SnackFoodConfiguration _configuration;

    public FoodSnackComponent(SnackFoodConfiguration configuration) => _configuration = configuration;

    public FoodProgress GetFoodProgress { get; private set; } = new();

    public SnackProgress GetSnackProgress { get; private set; } = new();

    public AdditionalSnackProgress GetAdditionalSnackProgress { get; private set; } = new();

    public AdditionalFoodProgress GetAdditionalFoodProgress { get; private set; } = new();

    public bool AddSnack(IGameItem gameItem)
    {
        SnackProgress progress = GetSnackProgress ??= new SnackProgress();

        if (progress.SnackHpBuffer >= _configuration.SnackSoftCap || progress.SnackMpBuffer >= _configuration.SnackSoftCap || progress.SnackSpBuffer >= _configuration.SnackSoftCap)
        {
            return true;
        }

        progress.SnackHpBuffer += gameItem.Hp;
        progress.SnackHpBufferSize = progress.SnackHpBuffer;
        progress.SnackMpBuffer += gameItem.Mp;
        progress.SnackMpBufferSize = progress.SnackMpBuffer;
        progress.SnackSpBuffer += gameItem.Data[4];
        progress.SnackSpBufferSize = progress.SnackSpBuffer;

        if (progress.SnackHpBuffer >= _configuration.SnackHardCap)
        {
            progress.SnackHpBuffer = _configuration.SnackHardCap;
            progress.SnackHpBufferSize = _configuration.SnackHardCap;
        }

        if (progress.SnackMpBuffer >= _configuration.SnackHardCap)
        {
            progress.SnackMpBuffer = _configuration.SnackHardCap;
            progress.SnackMpBufferSize = _configuration.SnackHardCap;
        }

        if (progress.SnackSpBuffer >= _configuration.SnackHardCap)
        {
            progress.SnackSpBuffer = _configuration.SnackHardCap;
            progress.SnackSpBufferSize = _configuration.SnackHardCap;
        }

        return false;
    }

    public void AddAdditionalSnack(int max, int amount, bool isHp, int cap = 100)
    {
        AdditionalSnackProgress additionalProgress = GetAdditionalSnackProgress ??= new AdditionalSnackProgress();

        if (isHp)
        {
            int hpCap = (int)Math.Round(max * cap / 100.0);

            if (additionalProgress.HpCap != 0 && additionalProgress.HpCap > cap)
            {
                return;
            }

            additionalProgress.SnackAdditionalHpBuffer += amount;
            additionalProgress.SnackAdditionalHpBufferSize = additionalProgress.SnackAdditionalHpBuffer;
            additionalProgress.HpCap = (int)Math.Round(hpCap * 100.0 / max);

            return;
        }

        int mpCap = (int)Math.Round(max * cap / 100.0);

        if (additionalProgress.MpCap != 0 && additionalProgress.MpCap > cap)
        {
            return;
        }

        additionalProgress.SnackAdditionalMpBuffer += amount;
        additionalProgress.SnackAdditionalMpBufferSize = additionalProgress.SnackAdditionalMpBuffer;
        additionalProgress.MpCap = (int)Math.Round(mpCap * 100.0 / max);
    }

    public bool AddFood(IGameItem gameItem)
    {
        FoodProgress progress = GetFoodProgress ??= new FoodProgress();

        if (progress.FoodHpBuffer >= _configuration.FoodSoftCap || progress.FoodMpBuffer >= _configuration.FoodSoftCap || progress.FoodSpBuffer >= _configuration.FoodSoftCap)
        {
            return true;
        }

        progress.FoodHpBuffer += gameItem.Hp;
        progress.FoodHpBufferSize = progress.FoodHpBuffer;
        progress.FoodMpBuffer += gameItem.Mp;
        progress.FoodMpBufferSize = progress.FoodMpBuffer;
        progress.FoodSpBuffer += gameItem.Data[4];
        progress.FoodSpBufferSize = progress.FoodSpBuffer;

        if (progress.FoodHpBuffer >= _configuration.FoodHardCap)
        {
            progress.FoodHpBuffer = _configuration.FoodHardCap;
            progress.FoodHpBufferSize = _configuration.FoodHardCap;
        }

        if (progress.FoodMpBuffer >= _configuration.FoodHardCap)
        {
            progress.FoodMpBuffer = _configuration.FoodHardCap;
            progress.FoodMpBufferSize = _configuration.FoodHardCap;
        }

        if (progress.FoodSpBuffer >= _configuration.FoodHardCap)
        {
            progress.FoodSpBuffer = _configuration.FoodHardCap;
            progress.FoodSpBufferSize = _configuration.FoodHardCap;
        }

        int mateProgress = gameItem.Data[5];
        if (mateProgress != 0)
        {
            progress.IncreaseTick += mateProgress;
            progress.FoodMateMaxHpBuffer = 100;
            progress.FoodMateMaxHpBufferSize = 100;
        }

        return false;
    }

    public void AddAdditionalFood(int max, int amount, bool isHp, int cap = 100)
    {
        AdditionalFoodProgress additionalProgress = GetAdditionalFoodProgress ??= new AdditionalFoodProgress();

        if (isHp)
        {
            int hpCap = (int)Math.Round(max * cap / 100.0);

            if (additionalProgress.HpCap != 0 && additionalProgress.HpCap > cap)
            {
                return;
            }

            additionalProgress.FoodAdditionalHpBuffer += amount;
            additionalProgress.FoodAdditionalHpBufferSize = additionalProgress.FoodAdditionalHpBuffer;
            additionalProgress.HpCap = (int)Math.Round(hpCap * 100.0 / max);

            return;
        }

        int mpCap = (int)Math.Round(max * cap / 100.0);

        if (additionalProgress.MpCap != 0 && additionalProgress.MpCap > cap)
        {
            return;
        }

        additionalProgress.FoodAdditionalMpBuffer += amount;
        additionalProgress.FoodAdditionalMpBufferSize = additionalProgress.FoodAdditionalMpBuffer;
        additionalProgress.MpCap = (int)Math.Round(mpCap * 100.0 / max);
    }

    public void ClearFoodBuffer()
    {
        GetFoodProgress = null;
        GetAdditionalFoodProgress = null;
    }

    public void ClearSnackBuffer()
    {
        GetSnackProgress = null;
        GetAdditionalSnackProgress = null;
    }
}