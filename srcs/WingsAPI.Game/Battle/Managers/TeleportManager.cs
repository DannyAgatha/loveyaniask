using System.Collections.Generic;
using WingsEmu.Core.Extensions;
using WingsEmu.Game.Helpers.Damages;

namespace WingsEmu.Game.Battle;

public class TeleportManager : ITeleportManager
{
    private readonly Dictionary<long, Position> _savedPosition = new();

    public void SavePosition(long id, Position position) => _savedPosition[id] = position;
    public Position GetPosition(long id) => _savedPosition.GetOrDefault(id);

    public void RemovePosition(long id)
    {
        if (!_savedPosition.ContainsKey(id))
        {
            return;
        }

        _savedPosition.Remove(id);
    }
}