using System.Threading;
using System.Threading.Tasks;
using PhoenixLib.Events;
using PhoenixLib.Logging;
using WingsAPI.Game.Extensions.ItemExtension.Item;
using WingsAPI.Packets.Enums;
using WingsEmu.DTOs.Maps;
using WingsEmu.Game._enum;
using WingsEmu.Game._i18n;
using WingsEmu.Game.Act4;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Managers;
using WingsEmu.Game.Managers.StaticData;
using WingsEmu.Game.Networking;
using WingsEmu.Game.Raids;
using WingsEmu.Game.Raids.Enum;
using WingsEmu.Game.Raids.Events;
using WingsEmu.Game.Relations;
using WingsEmu.Packets.Enums;
using WingsEmu.Packets.Enums.Chat;

namespace Plugin.Raids;

public class RaidPartyJoinEventHandler : IAsyncEventProcessor<RaidPartyJoinEvent>
{
    private static readonly SemaphoreSlim _semaphoreSlim = new(1, 1);
    private readonly IGameLanguageService _gameLanguage;
    private readonly IInvitationManager _invitationManager;
    private readonly IItemsManager _itemsManager;
    private readonly IRaidManager _raidManager;
    private readonly ISessionManager _sessionManager;
    private readonly IAct4CaligorManager _act4CaligorManager;

    public RaidPartyJoinEventHandler(IGameLanguageService gameLanguage, ISessionManager sessionManager, IInvitationManager invitationManager, IRaidManager raidManager, IItemsManager itemsManager, IAct4CaligorManager act4CaligorManager)
    {
        _gameLanguage = gameLanguage;
        _sessionManager = sessionManager;
        _invitationManager = invitationManager;
        _raidManager = raidManager;
        _itemsManager = itemsManager;
        _act4CaligorManager = act4CaligorManager;
    }

    public async Task HandleAsync(RaidPartyJoinEvent e, CancellationToken cancellation)
    {
        IClientSession session = e.Sender;
        long raidOwnerId = e.RaidOwnerId;
        bool isByRaidList = e.IsByRaidList;

        await _semaphoreSlim.WaitAsync(cancellation);
        try
        {
            if (session.PlayerEntity.Id == raidOwnerId)
            {
                return;
            }

            if (session.IsMuted())
            {
                session.SendMsg(session.GetLanguage(GameDialogKey.MUTE_SHOUTMESSAGE_YOU_ARE_MUTED), MsgMessageType.Middle);
                return;
            }

            IClientSession raidOwner = _sessionManager.GetSessionByCharacterId(raidOwnerId);
            if (raidOwner?.PlayerEntity.Raid == null)
            {
                Log.Debug($"[ERROR] Character with ID {raidOwnerId.ToString()} was not found.");
                return;
            }

            if (session.PlayerEntity.IsBlocking(raidOwnerId))
            {
                session.SendInfo(session.GetLanguage(GameDialogKey.BLACKLIST_INFO_BLOCKING));
                return;
            }

            if (raidOwner.PlayerEntity.IsBlocking(session.PlayerEntity.Id))
            {
                session.SendInfo(session.GetLanguage(GameDialogKey.BLACKLIST_INFO_BLOCKED));
                return;
            }

            if (raidOwner.PlayerEntity.HasRaidStarted)
            {
                session.SendChatMessage(_gameLanguage.GetLanguage(GameDialogKey.RAID_CHATMESSAGE_NO_EXIST_OR_ALREADY_STARTED, session.UserLanguage), ChatMessageColorType.Yellow);
                return;
            }

            if (!_invitationManager.ContainsPendingInvitation(raidOwnerId, session.PlayerEntity.Id, InvitationType.Raid) && !isByRaidList)
            {
                Log.Debug($"[ERROR] Character with ID {session.PlayerEntity.Id.ToString()} to join a raid, but hadn't an invitation.");
                return;
            }

            if (!isByRaidList)
            {
                _invitationManager.RemovePendingInvitation(raidOwnerId, session.PlayerEntity.Id, InvitationType.Raid);
            }

            RaidParty raidParty = raidOwner.PlayerEntity.Raid;
            if (!e.IsRaidMarathonMode && isByRaidList && !_raidManager.ContainsRaidInRaidPublishList(raidParty))
            {
                session.SendInfo(_gameLanguage.GetLanguage(GameDialogKey.RAID_CHATMESSAGE_NO_EXIST_OR_ALREADY_STARTED, session.UserLanguage));
                await session.EmitEventAsync(new RaidListOpenEvent());
                return;
            }

            if (!session.CurrentMapInstance.HasMapFlag(MapFlags.IS_BASE_MAP) && (raidParty.Type != RaidType.Fernon || !_act4CaligorManager.FernonMapsActive))
            {
                session.SendMsg(_gameLanguage.GetLanguage(GameDialogKey.INFORMATION_SHOUTMESSAGE_MUST_BE_IN_CLASSIC_MAP, session.UserLanguage), MsgMessageType.Middle);
                return;
            }

            if (raidOwner.PlayerEntity.RaidTeamIsFull)
            {
                session.SendMsg(_gameLanguage.GetLanguage(GameDialogKey.RAID_SHOUTMESSAGE_RAID_TEAM_FULL, session.UserLanguage), MsgMessageType.Middle);
                return;
            }

            if (session.PlayerEntity.IsInRaidParty)
            {
                session.SendMsg(_gameLanguage.GetLanguage(GameDialogKey.RAID_SHOUTMESSAGE_ALREADY_IN_RAID, session.UserLanguage), MsgMessageType.Middle);
                return;
            }

            if (session.PlayerEntity.TimeSpaceComponent.IsInTimeSpaceParty)
            {
                return;
            }

            if (session.PlayerEntity.RainbowBattleComponent.IsInRainbowBattle)
            {
                return;
            }
            
            if (session.PlayerEntity.MapInstance.IsAct6PvpInstance)
            {
                return;
            }

            if (session.PlayerEntity.HasRaidStarted || session.PlayerEntity.HasShopOpened)
            {
                return;
            }

            if (session.PlayerEntity.IsInGroup())
            {
                session.SendMsg(_gameLanguage.GetLanguage(GameDialogKey.RAID_SHOUTMESSAGE_IN_GROUP, session.UserLanguage), MsgMessageType.Middle);
                return;
            }

            if (session.PlayerEntity.Level < raidParty.MinimumLevel)
            {
                session.SendChatMessage(_gameLanguage.GetLanguageFormat(GameDialogKey.RAID_CHATMESSAGE_LOW_LEVEL, session.UserLanguage, raidParty.MinimumLevel.ToString()), ChatMessageColorType.Red);
                return;
            }

            if (session.PlayerEntity.HeroLevel < raidParty.MinimumHeroLevel)
            {
                session.SendChatMessage(_gameLanguage.GetLanguageFormat(GameDialogKey.RAID_CHATMESSAGE_LOW_LEVEL, session.UserLanguage, raidParty.MinimumHeroLevel.ToString()),
                    ChatMessageColorType.Red);
                return;
            }

            if (session.IsRaidTypeRestricted(raidParty.Type))
            {
                if (!session.IsPlayerWearingRaidAmulet(raidParty.Type))
                {
                    string amuletName = raidParty.Type == RaidType.LordDraco
                        ? _itemsManager.GetItem((short)ItemVnums.DRACO_AMULET)?.GetItemName(_gameLanguage, session.UserLanguage)
                        : _itemsManager.GetItem((short)ItemVnums.GLACERUS_AMULET)?.GetItemName(_gameLanguage, session.UserLanguage);

                    session.SendChatMessage(session.GetLanguageFormat(GameDialogKey.RAID_CHATMESSAGE_AMULET_NEEDED, amuletName), ChatMessageColorType.Yellow);
                    return;
                }

                string getRaidName = session.GenerateRaidName(_gameLanguage, raidParty.Type);
                if (!session.CanPlayerJoinToRestrictedRaid(raidParty.Type))
                {
                    session.SendChatMessage(session.GetLanguageFormat(GameDialogKey.RAID_CHATMESSAGE_LIMIT_REACHED, getRaidName), ChatMessageColorType.Yellow);
                    return;
                }
            }

            if (session.PlayerEntity.Level > raidParty.MaximumLevel || session.PlayerEntity.HeroLevel > raidParty.MaximumHeroLevel)
            {
                session.SendChatMessage(_gameLanguage.GetLanguage(GameDialogKey.RAID_CHATMESSAGE_LEVEL_TOO_HIGH, session.UserLanguage), ChatMessageColorType.Yellow);
            }

            raidParty.AddMember(session);
            session.PlayerEntity.SetRaidParty(raidParty);
            session.PlayerEntity.Raid.IsMarathonMode = raidOwner.PlayerEntity.Raid.IsMarathonMode;
            raidOwner.SendRaidPacket(RaidPacketType.LIST_MEMBERS);
            foreach (IClientSession member in session.PlayerEntity.Raid.Members)
            {
                member.SendChatMessage(_gameLanguage.GetLanguageFormat(GameDialogKey.RAID_CHATMESSAGE_X_JOINED, member.UserLanguage, session.PlayerEntity.Name), ChatMessageColorType.Yellow);
                member.SendMsg(_gameLanguage.GetLanguageFormat(GameDialogKey.RAID_SHOUTMESSAGE_X_JOINED, member.UserLanguage, session.PlayerEntity.Name), MsgMessageType.Middle);
                member.RefreshRaidMemberList(raidParty.IsSpecialRaid());
            }

            session.SendRaidPacket(RaidPacketType.LEAVE);
            session.SendRaidPacket(RaidPacketType.LEADER_RELATED);

            await session.EmitEventAsync(new RaidJoinedEvent { JoinType = e.IsByRaidList ? RaidJoinType.RAID_LIST : RaidJoinType.INVITATION });
        }
        finally
        {
            _semaphoreSlim.Release();
        }
    }
}