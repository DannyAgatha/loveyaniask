using System.Collections.Generic;
using WingsEmu.Game.Items;
using WingsEmu.Packets.Enums;

namespace WingsEmu.Game.Managers.StaticData;

public class StaticItemsManager
{
    public static IItemsManager Instance { get; private set; }

    public static void Initialize(IItemsManager manager)
    {
        Instance = manager;
    }
}

public interface IItemsManager
{
    /// <summary>
    ///     Loads the items into the cache
    /// </summary>
    void Initialize();

    /// <summary>
    ///     Returns an item with the corresponding VNum
    /// </summary>
    /// <param name="vnum"></param>
    /// <returns></returns>
    IGameItem GetItem(int vnum);

    /// <summary>
    ///     Get item
    /// </summary>
    /// <param name="name">key</param>
    /// <returns></returns>
    List<IGameItem> GetItem(string name);

    /// <summary>
    ///     Returns a list of items with the ItemType specified
    ///     as parameter
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    IEnumerable<IGameItem> GetItemsByType(ItemType type);

    /// <summary>
    ///     Returns a Title ID based on the VNum
    /// </summary>
    /// <param name="itemVnum"></param>
    /// <returns></returns>
    int GetTitleId(int itemVnum);
}