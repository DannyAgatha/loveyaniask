using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WingsAPI.Packets.Enums.Rainbow;
using WingsEmu.Game._enum;
using WingsEmu.Game.Configurations;
using WingsEmu.Game.Entities;
using WingsEmu.Game.Entities.Event;
using WingsEmu.Game.Maps;
using WingsEmu.Game.Networking;
using WingsEmu.Game.RainbowBattle;

namespace Plugin.RainbowBattle.Managers
{
    public interface IRainbowFactory
    {
        Task<RainbowBattleParty> CreateRainbowBattle(List<IClientSession> redTeam, List<IClientSession> blueTeam);
    }

    public class RainbowFactory : IRainbowFactory
    {
        private readonly IMapManager _mapManager;
        private readonly INpcEntityFactory _npcEntityFactory;
        private readonly IMonsterEntityFactory _monsterEntityFactory;
        private readonly RainbowBattleConfiguration _rainbowBattleConfiguration;

        public RainbowFactory(IMapManager mapManager, INpcEntityFactory npcEntityFactory, RainbowBattleConfiguration rainbowBattleConfiguration, 
            IMonsterEntityFactory monsterEntityFactory)
        {
            _mapManager = mapManager;
            _npcEntityFactory = npcEntityFactory;
            _rainbowBattleConfiguration = rainbowBattleConfiguration;
            _monsterEntityFactory = monsterEntityFactory;
        }

        public async Task<RainbowBattleParty> CreateRainbowBattle(List<IClientSession> redTeam, List<IClientSession> blueTeam)
        {
            IMapInstance mapInstance = _mapManager.GenerateMapInstanceByMapId(_rainbowBattleConfiguration.MapId, MapInstanceType.RainbowBattle);
            if (mapInstance == null)
            {
                return null;
            }

            foreach (FlagPosition position in _rainbowBattleConfiguration.MainFlags)
            {
                INpcEntity mainFlag = _npcEntityFactory.CreateNpc((short)MonsterVnum.BIG_FLAG, mapInstance, null, new NpcAdditionalData
                {
                    RainbowFlag = new RainBowFlag
                    {
                        FlagType = RainbowBattleFlagType.Big,
                        FlagTeamType = RainbowBattleFlagTeamType.None
                    }
                });

                await mainFlag.EmitEventAsync(new MapJoinNpcEntityEvent(mainFlag, position.X, position.Y));
            }

            foreach (FlagPosition position in _rainbowBattleConfiguration.MediumFlags)
            {
                INpcEntity mediumFlag = _npcEntityFactory.CreateNpc((short)MonsterVnum.MEDIUM_FLAG, mapInstance, null, new NpcAdditionalData
                {
                    RainbowFlag = new RainBowFlag
                    {
                        FlagType = RainbowBattleFlagType.Medium,
                        FlagTeamType = RainbowBattleFlagTeamType.None
                    }
                });

                await mediumFlag.EmitEventAsync(new MapJoinNpcEntityEvent(mediumFlag, position.X, position.Y));
            }

            foreach (FlagPosition position in _rainbowBattleConfiguration.SmallFlags)
            {
                INpcEntity smallFlag = _npcEntityFactory.CreateNpc((short)MonsterVnum.SMALL_FLAG, mapInstance, null, new NpcAdditionalData
                {
                    RainbowFlag = new RainBowFlag
                    {
                        FlagType = RainbowBattleFlagType.Small,
                        FlagTeamType = RainbowBattleFlagTeamType.None
                    }
                });

                await smallFlag.EmitEventAsync(new MapJoinNpcEntityEvent(smallFlag, position.X, position.Y));
            }

            mapInstance.Initialize(DateTime.UtcNow.AddSeconds(-1));

            var rainbowParty = new RainbowBattleParty(redTeam, blueTeam)
            {
                MapInstance = mapInstance
            };
            return rainbowParty;
        }
    }
}