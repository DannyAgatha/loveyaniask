// NosEmu
// 



using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using PhoenixLib.Events;
using PhoenixLib.Logging;
using WingsAPI.Game.Extensions.ItemExtension.Inventory;
using WingsAPI.Game.Extensions.ItemExtension.Item;
using WingsAPI.Packets.Enums;
using WingsEmu.Game._enum;
using WingsEmu.Game._i18n;
using WingsEmu.Game.Battle;
using WingsEmu.Game.Buffs;
using WingsEmu.Game.Configurations;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Managers.StaticData;
using WingsEmu.Game.Networking;
using WingsEmu.Game.Raids;
using WingsEmu.Game.Raids.Events;
using WingsEmu.Packets.Enums.Chat;

namespace Plugin.Raids;

public class RaidInstanceStartEventHandler : IAsyncEventProcessor<RaidInstanceStartEvent>
{
    private readonly IGameLanguageService _gameLanguage;
    private readonly IItemsManager _itemsManager;
    private readonly IRaidFactory _raidFactory;
    private readonly IRaidManager _raidManager;
    private readonly IBossScalingConfiguration _raidModeConfiguration;
    
    public RaidInstanceStartEventHandler(IRaidManager raidManager, IRaidFactory raidFactory, IGameLanguageService gameLanguage, IItemsManager itemsManager, IBossScalingConfiguration raidModeConfiguration)
    {
        _raidManager = raidManager;
        _raidFactory = raidFactory;
        _gameLanguage = gameLanguage;
        _itemsManager = itemsManager;
        _raidModeConfiguration = raidModeConfiguration;
    }
    
    public DateTime RaidStartTime { get; set; }

    public async Task HandleAsync(RaidInstanceStartEvent e, CancellationToken cancellation)
    {
        IClientSession session = e.Sender;
        RaidParty raidParty = e.Sender.PlayerEntity.Raid;
        int raidMembersCount = session.PlayerEntity.Raid.Members.Count;
        
        if (session.PlayerEntity.HasRaidStarted)
        {
            return;
        }

        if (!session.PlayerEntity.IsRaidLeader(session.PlayerEntity.Id))
        {
            return;
        }
        
        if (session.PlayerEntity.LastSeal== null)
        {
            return;
        }

        if (!session.PlayerEntity.HasItem(session.PlayerEntity.LastSeal.ItemInstance.ItemVNum))
        {
            return;
        }

        _raidManager.UnregisterRaidFromRaidPublishList(session.PlayerEntity.Raid);

        RaidInstance raidInstance = _raidFactory.CreateRaid(session.PlayerEntity.Raid);

        if (raidInstance == null)
        {
            Log.Warn("Failed to create raid instance");
            return;
        }
        
        await session.RemoveItemFromInventory(session.PlayerEntity.LastSeal.ItemInstance.ItemVNum);

        if (session.PlayerEntity.Raid.Members.Count > session.PlayerEntity.Raid.MaximumMembers)
        {
            IClientSession getLastJoinedMember = session.PlayerEntity.Raid.Members[^1];
            getLastJoinedMember?.EmitEvent(new RaidPartyLeaveEvent(false, false));
        }

        RaidType raidType = session.PlayerEntity.Raid.Type;
        IReadOnlyList<Buff> buffs = session.PlayerEntity.BuffComponent.GetAllBuffs(x => x.CardId >= (int)BuffVnums.FIRE_BLESSING_CAPSULE && x.CardId <= (int)BuffVnums.OCEANS_BLESSING_CAPSULE);
        session.PlayerEntity.RemoveBuffAsync(false, buffs.ToArray()).ConfigureAwait(false).GetAwaiter().GetResult();

        if (e.ForceTeleport == false)
        {
            foreach (IClientSession raidSession in session.PlayerEntity.Raid.Members)
            {
                string getRaidName = session.GenerateRaidName(_gameLanguage, raidType);
                if (raidSession.IsRaidTypeRestricted(raidType))
                {
                    if (!raidSession.IsPlayerWearingRaidAmulet(raidType))
                    {
                        string amuletName = raidType == RaidType.LordDraco
                            ? _itemsManager.GetItem((short)ItemVnums.DRACO_AMULET)?.GetItemName(_gameLanguage, raidSession.UserLanguage)
                            : _itemsManager.GetItem((short)ItemVnums.GLACERUS_AMULET)?.GetItemName(_gameLanguage, raidSession.UserLanguage);

                        raidSession.SendChatMessage(raidSession.GetLanguageFormat(GameDialogKey.RAID_CHATMESSAGE_AMULET_NEEDED, amuletName), ChatMessageColorType.Yellow);
                        await raidSession.EmitEventAsync(new RaidPartyLeaveEvent(false, false));
                        continue;
                    }

                    if (!raidSession.CanPlayerJoinToRestrictedRaid(raidType))
                    {
                        raidSession.SendChatMessage(raidSession.GetLanguageFormat(GameDialogKey.RAID_CHATMESSAGE_LIMIT_REACHED, getRaidName), ChatMessageColorType.Yellow);
                        await raidSession.EmitEventAsync(new RaidPartyLeaveEvent(false, false));
                        continue;
                    }
                }

                if (raidSession.CurrentMapInstance.Id == session.CurrentMapInstance.Id)
                {
                    continue;
                }
                
                await raidSession.EmitEventAsync(new RaidPartyLeaveEvent(false, false));
            }
        }
        
        if (session.PlayerEntity.Raid != null)
        {
            session.PlayerEntity.Raid.StartTime = DateTime.UtcNow;
            session.PlayerEntity.Raid.StartRaid(raidInstance);
        }
        
        raidParty.ModeType = _raidModeConfiguration.GetModeTypeByRaidTypeAndPlayers(raidParty.Type, raidMembersCount);
        
        foreach (RaidSubInstance raid in raidInstance.RaidSubInstances.Values)
        {
            _raidManager.AddRaidPartyByMapInstanceId(raid.MapInstance.Id, session.PlayerEntity.Raid);
        }

        foreach (IClientSession raidSession in session.PlayerEntity.Raid.Members)
        {
            string getRaidName = raidSession.GenerateRaidName(_gameLanguage, raidType);
            if (!raidSession.PlayerEntity.IsAlive())
            {
                raidSession.PlayerEntity.Hp = 1;
                raidSession.PlayerEntity.Mp = 1;
            }
            
            if (raidType == RaidType.Fernon)
            {
                raidSession.PlayerEntity.SetPositionBeforeFernonRaidEnter(raidSession.PlayerEntity.Position);
            }

            raidSession.ChangeMap(raidInstance.SpawnInstance.MapInstance, raidInstance.SpawnPoint.X, raidInstance.SpawnPoint.Y);
            raidSession.SendRaidUiPacket(raidType, RaidWindowType.MISSION_START);
            raidSession.SendDiscordRpcPacket($"{session.GetLanguage(GameDialogKey.PARTICIPATING_RAID_RPC)} {raidType}");
            raidSession.SendChatMessage(raidSession.GetLanguageFormat(GameDialogKey.RAID_STARTED_AS_MODE, 
                raidParty.ModeType), ChatMessageColorType.Red);

            //Sending this last two seems useless, but sending them just to be sure
            raidSession.SendRaidPacket(RaidPacketType.INSTANCE_START);
            raidSession.SendRaidPacket(RaidPacketType.AFTER_INSTANCE_START_BUT_BEFORE_REFRESH_MEMBERS);

            switch (raidType)
            {
                case RaidType.LordDraco:
                    raidSession.PlayerEntity.RaidRestrictionDto.LordDraco--;

                    raidSession.SendChatMessage(raidSession.GetLanguageFormat(GameDialogKey.RAID_CHATMESSAGE_LIMIT_LEFT,
                        getRaidName, raidSession.PlayerEntity.RaidRestrictionDto.LordDraco), ChatMessageColorType.Yellow);
                    break;
                case RaidType.Glacerus:
                    raidSession.PlayerEntity.RaidRestrictionDto.Glacerus--;

                    raidSession.SendChatMessage(raidSession.GetLanguageFormat(GameDialogKey.RAID_CHATMESSAGE_LIMIT_LEFT,
                        getRaidName, raidSession.PlayerEntity.RaidRestrictionDto.Glacerus), ChatMessageColorType.Yellow);
                    break;
            }
        }

        await session.EmitEventAsync(new RaidStartedEvent());
    }
}