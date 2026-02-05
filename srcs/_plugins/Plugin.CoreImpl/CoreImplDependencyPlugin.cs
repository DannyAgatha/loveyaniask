using Microsoft.Extensions.DependencyInjection;
using PhoenixLib.Configuration;
using Plugin.CoreImpl.Configs;
using Plugin.CoreImpl.Entities;
using Plugin.CoreImpl.Maps;
using Plugin.CoreImpl.Skills;
using WingsAPI.Plugins;
using WingsEmu.Game._enum;
using WingsEmu.Game.Entities;
using WingsEmu.Game.Maps;
using WingsEmu.Game.Portals;
using WingsEmu.Game.Skills;

namespace Plugin.CoreImpl
{
    public class CoreImplDependencyPlugin : IGameServerPlugin
    {
        public string Name => nameof(CoreImplDependencyPlugin);


        public void AddDependencies(IServiceCollection services, GameServerLoader gameServer)
        {
            services.AddSingleton<IEntitySkillFactory, SkillEntityFactory>();
            services.AddSingleton<IMonsterEntityFactory, MonsterEntityFactory>();
            services.AddSingleton<INpcEntityFactory, NpcEntityFactory>();
            services.AddTransient<IMapInstanceFactory, MapInstanceFactory>();
            services.AddSingleton<IPortalFactory, PortalFactory>();
            services.AddSingleton<ITimeSpacePortalFactory, TimeSpacePortalFactory>();

            services.AddFileConfiguration(new HardcodedDialogsByNpcVnumFileConfig
            {
                new() { NpcVnum = (int)MonsterVnum.BIG_MINILAND_SIGN, DialogId = (int)DialogVnums.MINILAND_SIGN },
                new() { NpcVnum = (int)MonsterVnum.SMALL_MINILAND_SIGN, DialogId = (int)DialogVnums.MINILAND_SIGN },
                new() { NpcVnum = (int)MonsterVnum.HALLOWEEN_MINILAND_SIGN, DialogId = (int)DialogVnums.MINILAND_SIGN },
                new() { NpcVnum = (int)MonsterVnum.EASTER_MINILAND_SIGN, DialogId = (int)DialogVnums.MINILAND_SIGN },
                new() { NpcVnum = (int)MonsterVnum.PIRATE_MINILAND_SIGN, DialogId = (int)DialogVnums.MINILAND_SIGN },
                new() { NpcVnum = (int)MonsterVnum.CHRISTMAS_MINILAND_SIGN, DialogId = (int)DialogVnums.MINILAND_SIGN },
                new() { NpcVnum = (int)MonsterVnum.BIG_FLAG, DialogId = (int)DialogVnums.NPC_REQ },
                new() { NpcVnum = (int)MonsterVnum.MEDIUM_FLAG, DialogId = (int)DialogVnums.NPC_REQ },
                new() { NpcVnum = (int)MonsterVnum.SMALL_FLAG, DialogId = (int)DialogVnums.NPC_REQ },

                new() { NpcVnum = (short)MonsterVnum.FERNON_LEFT_BLADE, DialogId = (short)DialogVnums.IDK },
                new() { NpcVnum = (short)MonsterVnum.FERNON_RIGHT_BLADE, DialogId = (short)DialogVnums.IDK },

                new() { NpcVnum = (short)MonsterVnum.GRASS_CLUMP, DialogId = (short)DialogVnums.IDK },
                new() { NpcVnum = (short)MonsterVnum.CHEESE_CHUNK, DialogId = (short)DialogVnums.IDK },

                new() { NpcVnum = (int)MonsterVnum.SMALL_CAMPFIRE, DialogId = (int)DialogVnums.SMALL_CAMPFIRE },

                new() { NpcVnum = (int)MonsterVnum.ICE_MACHINE, DialogId = (int)DialogVnums.ICE_MACHINE },

                new() { NpcVnum = (int)MonsterVnum.GIANT_CAMPFIRE, DialogId = (int)DialogVnums.BIG_CAMPFIRE },
                
                new() { NpcVnum = (int)MonsterVnum.ILLUSORY_ALTAR, DialogId = (int)DialogVnums.IDK }
            });
        }
    }
}