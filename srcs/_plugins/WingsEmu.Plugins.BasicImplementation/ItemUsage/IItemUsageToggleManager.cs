// NosEmu
// 


using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using StackExchange.Redis;
using WingsEmu.Game.Inventory;

namespace NosEmu.Plugins.BasicImplementations.ItemUsage;

public class RedisItemUsageToggleManager : IItemUsageToggleManager
{
    private const string ITEM_TOGGLE_FEATURE_PREFIX = "game:item-toggle:";
    private readonly IDatabase _database;

    private readonly IConnectionMultiplexer _multiplexer;

    public RedisItemUsageToggleManager(IConnectionMultiplexer multiplexer)
    {
        _multiplexer = multiplexer;
        _database = _multiplexer.GetDatabase(0);
    }

    public async Task<bool> IsItemBlocked(int vnum) => await _database.KeyExistsAsync(ITEM_TOGGLE_FEATURE_PREFIX + vnum);

    public async Task BlockItemUsage(int vnum)
    {
        await _database.StringSetAsync(ITEM_TOGGLE_FEATURE_PREFIX + vnum, "blocked");
    }

    public async Task UnblockItemUsage(int vnum)
    {
        await _database.KeyDeleteAsync(ITEM_TOGGLE_FEATURE_PREFIX + vnum);
    }

    public async Task<IEnumerable<int>> GetBlockedItemUsages()
    {
        var disabledItems = new List<int>();
        foreach (EndPoint ep in _multiplexer.GetEndPoints())
        {
            IAsyncEnumerable<RedisKey> keys = _multiplexer.GetServer(ep).KeysAsync(0, ITEM_TOGGLE_FEATURE_PREFIX + "*");
            await foreach (RedisKey redisKey in keys)
            {
                string integerKey = redisKey.ToString().Split(':')[2];
                disabledItems.Add(Convert.ToInt32(integerKey));
            }
        }

        return disabledItems;
    }
}