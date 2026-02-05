using PhoenixLib.Events;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using WingsAPI.Packets.Enums.Chat;
using WingsEmu.Game;
using WingsEmu.Game._i18n;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Helpers.Damages;
using WingsEmu.Game.Maps;
using WingsEmu.Game.Monster.Event;
using WingsEmu.Packets.Enums.Chat;
using WingsEmu.Plugins.GameEvents.Configuration.InstantBattle;
using WingsEmu.Plugins.GameEvents.DataHolder;
using WingsEmu.Plugins.GameEvents.Event.InstantBattle;

namespace WingsEmu.Plugins.GameEvents.InstantBattle
{
    public class InstantBattleStartWaveEventHandler : IAsyncEventProcessor<InstantBattleStartWaveEvent>
    {
        private readonly IAsyncEventPipeline _eventPipeline;

        public InstantBattleStartWaveEventHandler(IAsyncEventPipeline eventPipeline) => _eventPipeline = eventPipeline;

        public async Task HandleAsync(InstantBattleStartWaveEvent e, CancellationToken cancellation)
        {
            IMapInstance map = e.Instance.MapInstance;
            InstantBattleInstanceWave wave = e.Wave;

            if (Enum.TryParse(wave.Configuration.TitleKey, out GameDialogKey key))
            {
                map.Broadcast(x => x.GenerateMsgPacket(x.GetLanguage(key), MsgMessageType.Middle));
            }

            var summons = new List<ToSummon>();
            foreach (InstantBattleMonster monster in wave.Configuration.Monsters)
            {
                for (int i = 0; i < monster.Amount; i++)
                {
                    Position position = map.GetRandomPosition();
                    var summon = new ToSummon
                    {
                        VNum = monster.MonsterVnum,
                        SpawnCell = position,
                        IsMoving = true,
                        IsHostile = true,
                        IsInstantBattle = true
                    };

                    summons.Add(summon);
                }
            }

            wave.MonsterSpawn = DateTime.UtcNow;
            await _eventPipeline.ProcessEventAsync(new MonsterSummonEvent(map, summons), cancellation);
            map.Broadcast(x => x.GenerateMsgiPacket(MessageType.Default, Game18NConstString.MonsterHaveAppeared));
        }
    }
}