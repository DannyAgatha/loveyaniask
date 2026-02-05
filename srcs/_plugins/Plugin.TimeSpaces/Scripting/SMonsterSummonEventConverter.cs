using System.Linq;
using PhoenixLib.Events;
using WingsAPI.Scripting.Converter;
using WingsAPI.Scripting.Event.Common;
using WingsEmu.Game;
using WingsEmu.Game.Helpers.Damages;
using WingsEmu.Game.Maps;
using WingsEmu.Game.Monster.Event;

namespace Plugin.TimeSpaces.Scripting;

public class SMonsterSummonEventConverter : ScriptedEventConverter<SMonsterSummonEvent>
{
    private readonly IMapInstance _mapInstance;

    public SMonsterSummonEventConverter(IMapInstance mapInstance) => _mapInstance = mapInstance;

    protected override IAsyncEvent Convert(SMonsterSummonEvent e)
    {
        return new MonsterSummonEvent(_mapInstance, e.Monsters.Select(x => new ToSummon
        {
            VNum = x.Vnum,
            SpawnCell = x.IsRandomPosition ? null : new Position(x.Position.X, x.Position.Y),
            IsMoving = x.CanMove,
            IsTarget = x.IsTarget,
            IsBossOrMate = x.IsBoss,
            Direction = x.Direction,
            HpMultiplier = x.HpMultiplier,
            MpMultiplier = x.MpMultiplier,
            Level = x.CustomLevel,
            IsHostile = true
        }));
    }
}