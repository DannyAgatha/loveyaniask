using PhoenixLib.Scheduler;
using System;
using WingsEmu.DTOs.Maps;
using WingsEmu.Game._i18n;
using WingsEmu.Game.Battle;
using WingsEmu.Game.Buffs;
using WingsEmu.Game.Characters;
using WingsEmu.Game.Entities;
using WingsEmu.Game.Managers.StaticData;
using WingsEmu.Game.Maps.Event;
using WingsEmu.Game.RespawnReturn.Event;
using WingsEmu.Packets.Enums;

namespace NosEmu.Plugins.BasicImplementations.BCards.Handlers
{
    public class BCardFireCannoneerRangeBuffHandler : IBCardEffectAsyncHandler
    {
        private readonly IBuffFactory _buffFactory;
        private readonly IGameLanguageService _gameLanguage;
        private readonly INpcMonsterManager _npcMonsterManager;
        private readonly IMonsterEntityFactory _entity;
        private readonly IScheduler _scheduler;

        public BCardFireCannoneerRangeBuffHandler(IBuffFactory buffFactory, IGameLanguageService gameLanguage, INpcMonsterManager npcMonsterManager,
            IMonsterEntityFactory monsterEntityFactory, IScheduler scheduler)
        {
            _buffFactory = buffFactory;
            _gameLanguage = gameLanguage;
            _npcMonsterManager = npcMonsterManager;
            _entity = monsterEntityFactory;
            _scheduler = scheduler;
        }

        public BCardType HandledType => BCardType.FireCannoneerRangeBuff;

        public async void Execute(IBCardEffectContext ctx)
        {
            IBattleEntity target = ctx.Target;
            IBattleEntity sender = ctx.Sender;
            byte subType = ctx.BCard.SubType;

            switch ((AdditionalTypes.FireCannoneerRangeBuff)subType)
            {
                case AdditionalTypes.FireCannoneerRangeBuff.TeleportToFishingSpot:
                    {
                        if (sender is not IPlayerEntity player)
                        {
                            return;
                        }

                        if (!player.MapInstance.HasMapFlag(MapFlags.IS_BASE_MAP))
                        {
                            return;
                        }

                        if (player.FishSavedLocation == null)
                        {
                            player.FishSavedLocation = new Tuple<short, short, short>((short)player.MapInstance.MapId, player.PositionX, player.PositionY);
                            await player.Session.EmitEventAsync(new RespawnPlayerEvent());
                            return;
                        }

                        short mapId = player.FishSavedLocation.Item1;
                        short mapX = player.FishSavedLocation.Item2;
                        short mapY = player.FishSavedLocation.Item3;
                        await player.Session.EmitEventAsync(new JoinMapEvent(mapId, mapX, mapY));
                        await player.RemoveBuffAsync(849);
                        player.FishSavedLocation = null;
                    }
                    break;
            }
        }
    }
}