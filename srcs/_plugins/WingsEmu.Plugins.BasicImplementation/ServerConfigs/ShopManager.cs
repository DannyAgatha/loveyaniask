// NosEmu
// 


using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NosEmu.Plugins.BasicImplementations.ServerConfigs.ImportObjects.Npcs;
using PhoenixLib.Caching;
using PhoenixLib.Logging;
using WingsAPI.Data.Shops;
using WingsEmu.DTOs.Shops;
using WingsEmu.Game.Entities;
using WingsEmu.Game.Managers.ServerData;
using WingsEmu.Game.Shops;
using NosEmu.Plugins.BasicImplementations.ServerConfigs.ImportObjects;

namespace NosEmu.Plugins.BasicImplementations.ServerConfigs;

public class ShopManager : IShopManager
{
    private readonly IEnumerable<MapNpcImportFile> _importFile;
    private readonly IShopFactory _shopFactory;

    private readonly ILongKeyCachedRepository<ShopNpc> _shopsByNpcId;

    public ShopManager(IEnumerable<MapNpcImportFile> importFile, ILongKeyCachedRepository<ShopNpc> shopsByNpcId, IShopFactory shopFactory)
    {
        _importFile = importFile;
        _shopsByNpcId = shopsByNpcId;
        _shopFactory = shopFactory;
    }

    public async Task InitializeAsync()
    {
        IEnumerable<MapNpcObject> importedNpcs = _importFile.SelectMany(x => x.Npcs.Select(s =>
        {
            s.MapId = x.MapId;
            return s;
        })).ToList();

        int shopItemsCount = 0;
        int shopSkillsCount = 0;
        int count = 0;

        foreach (MapNpcObject npc in importedNpcs)
        {
            try
            {
                if (npc.ItemShop == null && npc.SkillShop == null)
                {
                    continue;
                }

                ShopDTO shop = npc.SkillShop?.ToDto() ?? npc.ItemShop.ToDto();

                shop.MapNpcId = npc.MapNpcId;

                if (shop.MenuType == 1)
                {
                    shop.Skills = new List<ShopSkillDTO>();
                    foreach (MapNpcShopTabObject<MapNpcShopSkillObject> tabs in npc.SkillShop.ShopTabs.Where(x => x.Items != null))
                    {
                        short index = 0;
                        shop.Skills.AddRange(tabs.Items.Select(x =>
                        {
                            ShopSkillDTO tpp = x.ToDto((byte)tabs.ShopTabId, index);
                            index++;
                            return tpp;
                        }));
                    }
                }
                else
                {
                    short i = 0;
                    shop.Items = new List<ShopItemDTO>();
                    foreach (MapNpcShopTabObject<MapNpcShopItemObject> tabs in npc.ItemShop.ShopTabs.Where(tabs => tabs.Items != null))
                    {
                        shop.Items.AddRange(tabs.Items.Select(s =>
                        {
                            ShopItemDTO tpp = s.ToDto((byte)tabs.ShopTabId, i);
                            i++;
                            return tpp;
                        }));
                    }
                }

                _shopsByNpcId.Set(shop.MapNpcId, _shopFactory.CreateShop(shop));
                shopItemsCount += shop.Items?.Count ?? 0;
                shopSkillsCount += shop.Skills?.Count ?? 0;
                count++;
            }
            catch (Exception e)
            {
                Log.Error("[MAPNPC_IMPORT] ERROR", e);
            }
        }

        Log.Info($"[SHOP_MANAGER] Loaded {count.ToString()} shops.");
        Log.Info($"[SHOP_MANAGER] Loaded {shopItemsCount.ToString()} shops items.");
        Log.Info($"[SHOP_MANAGER] Loaded {shopSkillsCount.ToString()} shops skills.");
    }

    public ShopNpc GetShopByNpcId(int npcId) => _shopsByNpcId.Get(npcId);
}