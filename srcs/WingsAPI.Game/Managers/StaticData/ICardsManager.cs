using System.Collections.Generic;
using WingsEmu.Game.Buffs;

namespace WingsEmu.Game.Managers.StaticData;

public class StaticCardsManager
{
    public static ICardsManager Instance { get; private set; }

    public static void Initialize(ICardsManager manager)
    {
        Instance = manager;
    }
}

public interface ICardsManager
{
    /// <summary>
    ///     Loads the cards into the cache
    /// </summary>
    void Initialize();

    /// <summary>
    ///     Returns a card with the specified
    ///     card Id
    /// </summary>
    /// <param name="cardId"></param>
    /// <returns></returns>
    Card GetCardByCardId(int cardId);

    /// <summary>
    ///     Returns the card with the specified name
    /// </summary>
    /// <param name="name">i18n key</param>
    /// <returns></returns>
    List<Card> GetCardByName(string name);
}