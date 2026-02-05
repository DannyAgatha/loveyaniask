using WingsEmu.Game.Items;

namespace WingsEmu.Game.SnackFood;

public interface IFoodSnackComponent
{
    public FoodProgress GetFoodProgress { get; }
    public SnackProgress GetSnackProgress { get; }
    public AdditionalSnackProgress GetAdditionalSnackProgress { get; }
    public AdditionalFoodProgress GetAdditionalFoodProgress { get; }

    public bool AddSnack(IGameItem gameItem);
    public void AddAdditionalSnack(int max, int amount, bool isHp, int cap = 100);
    public bool AddFood(IGameItem gameItem);
    public void AddAdditionalFood(int max, int amount, bool isHp, int cap = 100);

    public void ClearFoodBuffer();
    public void ClearSnackBuffer();
}