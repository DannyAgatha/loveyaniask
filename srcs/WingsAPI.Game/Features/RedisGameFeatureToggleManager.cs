using System.Threading.Tasks;
using StackExchange.Redis;

namespace WingsEmu.Game.Features;

public class RedisGameFeatureToggleManager : IGameFeatureToggleManager
{
    private const string KeyPrefix = "game:disabled-feature";

    private readonly IDatabase _database;

    public RedisGameFeatureToggleManager(IConnectionMultiplexer multiplexer) => _database = multiplexer.GetDatabase(0);

    public async Task<bool> IsDisabled(GameFeature serviceName) => await _database.KeyExistsAsync(CreateKey(serviceName));

    public async Task Disable(GameFeature serviceName) => await _database.StringSetAsync(CreateKey(serviceName), "disabled");

    public async Task Enable(GameFeature serviceName) => await _database.KeyDeleteAsync(CreateKey(serviceName));

    private static string CreateKey(GameFeature serviceName) => $"{KeyPrefix}:{serviceName.ToString().ToLower()}";
}