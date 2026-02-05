namespace WingsEmu.Game._i18n;

public class StaticGameLanguageService
{
    public static IGameLanguageService Instance { get; private set; }

    public static void Initialize(IGameLanguageService service)
    {
        Instance = service;
    }
}