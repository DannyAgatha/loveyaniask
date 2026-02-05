namespace WingsEmu.Game;

public class StaticRandomGenerator
{
    public static IRandomGenerator Instance { get; private set; }

    public static void Initialize(IRandomGenerator generator)
    {
        Instance = generator;
    }
}

public interface IRandomGenerator
{
    /// <summary>
    ///     Generates a random number between min and max excluded
    /// </summary>
    /// <param name="min"></param>
    /// <param name="max"></param>
    /// <returns></returns>
    int RandomNumber(int min, int max);

    /// <summary>
    ///     Generates a random number between 0 and max excluded
    /// </summary>
    /// <param name="max"></param>
    /// <returns></returns>
    int RandomNumber(int max);

    /// <summary>
    ///     Generates a random number between 0 and 100
    /// </summary>
    /// <returns></returns>
    int RandomNumber();
    
    double RandomNumber(double min, double max);
}