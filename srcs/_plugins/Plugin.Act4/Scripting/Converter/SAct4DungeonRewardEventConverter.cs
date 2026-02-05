using PhoenixLib.Events;
using WingsAPI.Scripting.Converter;
using WingsAPI.Scripting.Event.Dungeon;
using WingsEmu.Game.Act4;
using WingsEmu.Game.Act4.Event;

namespace Plugin.Act4.Scripting.Converter;

public class SAct4DungeonRewardEventConverter : ScriptedEventConverter<SAct4DungeonRewardEvent>
{
    private readonly DungeonInstanceWrapper _dungeonInstanceWrapper;

    public SAct4DungeonRewardEventConverter(DungeonInstanceWrapper dungeonInstanceWrapper) => _dungeonInstanceWrapper = dungeonInstanceWrapper;

    protected override IAsyncEvent Convert(SAct4DungeonRewardEvent e) =>
        new Act4DungeonRewardEvent
        {
            DungeonInstanceWrapper = _dungeonInstanceWrapper
        };
}