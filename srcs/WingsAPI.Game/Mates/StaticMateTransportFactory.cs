namespace WingsEmu.Game.Mates;

public class StaticMateTransportFactory
{
    public static IMateTransportFactory Instance { get; private set; }

    public static void Initialize(IMateTransportFactory factory)
    {
        Instance = factory;
    }
}