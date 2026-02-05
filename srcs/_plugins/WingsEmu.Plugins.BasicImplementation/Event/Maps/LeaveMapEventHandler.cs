using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using PhoenixLib.Events;
using WingsAPI.Game.Extensions.ItemExtension.Inventory;
using WingsAPI.Packets.Enums.LandOfLife;
using WingsEmu.Game._enum;
using WingsEmu.Game._i18n;
using WingsEmu.Game.Battle;
using WingsEmu.Game.Entities;
using WingsEmu.Game.Entities.Extensions;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Extensions.Mates;
using WingsEmu.Game.Families.Event;
using WingsEmu.Game.Maps;
using WingsEmu.Game.Maps.Event;
using WingsEmu.Game.Mates;
using WingsEmu.Game.Networking;
using WingsEmu.Game.Networking.Broadcasting;
using WingsEmu.Game.PrivateMapInstances;
using WingsEmu.Game.Shops.Event;
using WingsEmu.Game.Skills;
using WingsEmu.Game.TimeSpaces;
using WingsEmu.Packets.Enums.Battle;
using WingsEmu.Packets.Enums.Chat;

namespace NosEmu.Plugins.BasicImplementations.Event.Maps;

public class LeaveMapEventHandler : IAsyncEventProcessor<LeaveMapEvent>
{
    private readonly IAsyncEventPipeline _asyncEventPipeline;
    private readonly IGameLanguageService _languageService;
    private readonly ISacrificeManager _sacrificeManager;
    private readonly ISpyOutManager _spyOutManager;
    private readonly ITeleportManager _teleportManager;

    public LeaveMapEventHandler(ISacrificeManager sacrificeManager, IGameLanguageService languageService, ISpyOutManager spyOutManager, ITeleportManager teleportManager,
        IAsyncEventPipeline asyncEventPipeline)
    {
        _sacrificeManager = sacrificeManager;
        _languageService = languageService;
        _spyOutManager = spyOutManager;
        _teleportManager = teleportManager;
        _asyncEventPipeline = asyncEventPipeline;
    }

    public async Task HandleAsync(LeaveMapEvent e, CancellationToken cancellation)
    {
        IClientSession session = e.Sender;
        if (session.CurrentMapInstance == null)
        {
            return;
        }

        IMapInstance mapInstance = session.CurrentMapInstance;

        await session.PlayerEntity.TryDespawnBomb(_asyncEventPipeline);
        await session.TryDespawnTimeSpacePortal();
        session.CurrentMapInstance.UnregisterSession(session);
        session.PlayerEntity.HitsByMonsters.Clear();
        session.PlayerEntity.SkillComponent.ArchMageMeteorites.Clear();
        session.PlayerEntity.UnderWaterShowdownId = null;
        
        if (session.PlayerEntity.HasShopOpened)
        {
            await session.EmitEventAsync(new ShopPlayerCloseEvent());
        }

        await session.CloseExchange();

        if (session.PlayerEntity.IsSitting)
        {
            session.PlayerEntity.IsSitting = false;
        }

        if (session.PlayerEntity.IsCastingSkill)
        {
            session.SendCancelPacket(CancelType.InCombatMode);
            session.PlayerEntity.RemoveCastingSkill();
        }

        session.PlayerEntity.IsWarehouseOpen = false;
        session.PlayerEntity.IsPartnerWarehouseOpen = false;
        if (session.PlayerEntity.IsFamilyWarehouseOpen || session.PlayerEntity.IsFamilyWarehouseLogsOpen)
        {
            await session.EmitEventAsync(new FamilyWarehouseCloseEvent());
        }

        session.PlayerEntity.IsCraftingItem = false;

        //Try removing shop
        await session.EmitEventAsync(new ShopPlayerCloseEvent());

        //Remove Sacrifice
        IBattleEntity target = _sacrificeManager.GetTarget(e.Sender.PlayerEntity);
        IBattleEntity caster = _sacrificeManager.GetCaster(e.Sender.PlayerEntity);
        if (target != null)
        {
            await e.Sender.PlayerEntity.RemoveSacrifice(target, _sacrificeManager, _languageService);
        }

        if (caster != null)
        {
            await caster.RemoveSacrifice(e.Sender.PlayerEntity, _sacrificeManager, _languageService);
        }

        session.RemoveBuddha();

        //Remove Teleport
        _teleportManager.RemovePosition(session.PlayerEntity.Id);
        if (session.PlayerEntity.HasBuff(BuffVnums.MEMORIAL))
        {
            await session.PlayerEntity.RemoveBuffAsync(false, session.PlayerEntity.BuffComponent.GetBuff((short)BuffVnums.MEMORIAL));
        }

        //Remove Spy
        if (_spyOutManager.ContainsSpyOut(session.PlayerEntity.Id))
        {
            _spyOutManager.RemoveSpyOutSkill(session.PlayerEntity.Id);
            session.SendObArPacket();
        }

        if (session.CurrentMapInstance.MapInstanceType == MapInstanceType.ArenaInstance)
        {
            session.SendArenaStatistics(true);
        }

        if (session.CurrentMapInstance.MapInstanceType == MapInstanceType.TimeSpaceInstance && session.PlayerEntity.TimeSpaceComponent.IsInTimeSpaceParty)
        {
            if (session.CurrentMapInstance.Id != session.PlayerEntity.TimeSpaceComponent.TimeSpace.Instance.SpawnInstance.MapInstance.Id)
            {
                IClientSession[] timeSpaceMembers = session.PlayerEntity.TimeSpaceComponent.TimeSpace.Members.ToArray();
                foreach (IClientSession member in timeSpaceMembers)
                {
                    member.SendPacket(mapInstance.GenerateRsfn(isVisit: true));
                }
            }

            if (session.PlayerEntity.TimeSpaceComponent.Partners.Any())
            {
                foreach (INpcEntity partner in session.PlayerEntity.TimeSpaceComponent.Partners)
                {
                    session.CurrentMapInstance.RemoveNpc(partner);
                    session.Broadcast(partner.GenerateOut(), new ExceptSessionBroadcast(session));
                }
            }
        }

        PrivateMapInstanceInfo info = session.PlayerEntity.PrivateMapInstanceInfo;
        if (info is not null)
        {
            string time = (DateTime.UtcNow - info.Start).ToString(@"hh\:mm\:ss");
            session.SendChatMessage(session.GetLanguageFormat(GameDialogKey.PRIVATE_MAP_INSTANCE_STATS_TIME, time), ChatMessageColorType.Yellow);
            session.SendChatMessage(session.GetLanguageFormat(GameDialogKey.PRIVATE_MAP_INSTANCE_STATS_ITEMS, info.ItemsCollected.ToString("N0")), ChatMessageColorType.Yellow);
            session.SendChatMessage(session.GetLanguageFormat(GameDialogKey.PRIVATE_MAP_INSTANCE_STATS_GOLD, info.GoldCollected.ToString("N0")), ChatMessageColorType.Yellow);
            session.SendChatMessage(session.GetLanguageFormat(GameDialogKey.PRIVATE_MAP_INSTANCE_STATS_FLOWER_BUFFS, info.FlowerBuffsCollected.ToString()), ChatMessageColorType.Yellow);
            session.SendChatMessage(session.GetLanguageFormat(GameDialogKey.PRIVATE_MAP_INSTANCE_STATS_MONSTERS_KILLED, info.MonstersKilled.ToString("N0")), ChatMessageColorType.Yellow);
            session.SendChatMessage(session.GetLanguageFormat(GameDialogKey.PRIVATE_MAP_INSTANCE_STATS_EXPERIENCE_GAINED, info.ExperienceGained.ToString("N0")), ChatMessageColorType.Yellow);
            session.SendChatMessage(session.GetLanguageFormat(GameDialogKey.PRIVATE_MAP_INSTANCE_STATS_HERO_EXPERIENCE_GAINED, info.HeroExperienceGained.ToString("N0")), ChatMessageColorType.Yellow);
            session.SendChatMessage(session.GetLanguageFormat(GameDialogKey.PRIVATE_MAP_INSTANCE_STATS_JOB_EXPERIENCE_GAINED, info.JobExperienceGained.ToString("N0")), ChatMessageColorType.Yellow);
            session.SendChatMessage(session.GetLanguageFormat(GameDialogKey.PRIVATE_MAP_INSTANCE_STATS_LEVEL_GAINED, info.LevelGained.ToString()), ChatMessageColorType.Yellow);
            session.SendChatMessage(session.GetLanguageFormat(GameDialogKey.PRIVATE_MAP_INSTANCE_STATS_HERO_LEVEL_GAINED, info.HeroLevelGained.ToString()), ChatMessageColorType.Yellow);
            session.PlayerEntity.PrivateMapInstanceInfo = null;
        }

        session.SendCMapPacket(false);
        session.SendMapOutPacket();
        if (!session.PlayerEntity.CheatComponent.IsInvisible)
        {
            session.BroadcastOut(new ExceptSessionBroadcast(session));
        }

        foreach (IMateEntity mate in session.PlayerEntity.MateComponent.TeamMembers())
        {
            mapInstance.RemoveMate(mate);
            if (session.PlayerEntity.CheatComponent.IsInvisible)
            {
                continue;
            }

            session.Broadcast(mate.GenerateOut(), new ExceptSessionBroadcast(session));
        }
    }
}