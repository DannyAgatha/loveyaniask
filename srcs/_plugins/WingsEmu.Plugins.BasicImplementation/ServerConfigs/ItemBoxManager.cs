using System.Collections.Generic;
using NosEmu.Plugins.BasicImplementations.ServerConfigs.ImportObjects.ItemBoxes;
using PhoenixLib.Caching;
using PhoenixLib.Logging;
using WingsEmu.DTOs.ServerDatas;
using WingsEmu.Game.Items;
using NosEmu.Plugins.BasicImplementations.ServerConfigs.ImportObjects;

namespace NosEmu.Plugins.BasicImplementations.ServerConfigs;

public class ItemBoxManager : IItemBoxManager
{
    private readonly IEnumerable<ItemBoxImportFile> _itemBoxConfigurations;
    private readonly IKeyValueCache<ItemBoxDto> _itemBoxesCache;
    private readonly IEnumerable<RandomBoxImportFile> _randomBoxConfigurations;

    public ItemBoxManager(IEnumerable<ItemBoxImportFile> itemBoxConfigurations, IEnumerable<RandomBoxImportFile> randomBoxConfigurations, IKeyValueCache<ItemBoxDto> itemBoxesCache)
    {
        _itemBoxConfigurations = itemBoxConfigurations;
        _randomBoxConfigurations = randomBoxConfigurations;
        _itemBoxesCache = itemBoxesCache;
    }

    public ItemBoxDto GetItemBoxByItemVnumAndDesign(int itemVnum) => _itemBoxesCache.Get(itemVnum.ToString());

    public void Initialize()
    {
        int boxesCount = 0;
        foreach (ItemBoxImportFile file in _itemBoxConfigurations)
        {
            ItemBoxDto box = file.ToDto();
            if (box == null)
            {
                continue;
            }

            // just the item box itself
            _itemBoxesCache.Set(box.Id.ToString(), box);
            boxesCount++;
        }

        foreach (RandomBoxImportFile file in _randomBoxConfigurations)
        {
            foreach (RandomBoxObject obj in file.Items)
            {
                ItemBoxDto box = obj.ToDtos();
                if (box == null)
                {
                    continue;
                }

                _itemBoxesCache.Set(box.Id.ToString(), box);
                boxesCount++;
            }
        }

        Log.Info($"[ITEMBOX_MANAGER] Loaded {boxesCount} itemBoxes");
    }
}