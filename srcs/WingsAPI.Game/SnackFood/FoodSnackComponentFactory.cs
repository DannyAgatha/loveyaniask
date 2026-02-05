namespace WingsEmu.Game.SnackFood;

public class FoodSnackComponentFactory : IFoodSnackComponentFactory
{
    private readonly SnackFoodConfiguration _snackFoodConfiguration;

    public FoodSnackComponentFactory(SnackFoodConfiguration snackFoodConfiguration) => _snackFoodConfiguration = snackFoodConfiguration;

    public IFoodSnackComponent CreateFoodSnackComponent() => new FoodSnackComponent(_snackFoodConfiguration);
}