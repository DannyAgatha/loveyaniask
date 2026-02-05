using System.Collections.Generic;
using System.Linq;

namespace WingsEmu.Game.Shops;

public interface IShopComponent
{
    IEnumerable<ShopPlayerItem> Items { get; }
    string Name { get; set; }
    long Sell { get; set; }
    ShopPlayerItem GetItem(short slot);
    void AddShop(IEnumerable<ShopPlayerItem> items);
    void RemoveShopItem(ShopPlayerItem shopPlayerItem);
    void RemoveShop();
}

public class ShopComponent : IShopComponent
{
    private ShopPlayerItem[] _items;
    public string Name { get; set; }

    public long Sell { get; set; }
    public IEnumerable<ShopPlayerItem> Items => _items;

    public ShopPlayerItem GetItem(short slot) => _items?[slot] == null ? null : _items[slot];

    public void RemoveShopItem(ShopPlayerItem shopPlayerItem)
    {
        if (_items == null)
        {
            return;
        }

        if (shopPlayerItem == null)
        {
            return;
        }

        _items[shopPlayerItem.ShopSlot] = null;
    }

    public void AddShop(IEnumerable<ShopPlayerItem> items)
    {
        _items = new ShopPlayerItem[20];
        foreach (ShopPlayerItem item in items.Where(item => item != null))
        {
            _items[item.ShopSlot] = item;
        }
    }

    public void RemoveShop()
    {
        _items = null;
        Name = null;
        Sell = 0;
    }
}