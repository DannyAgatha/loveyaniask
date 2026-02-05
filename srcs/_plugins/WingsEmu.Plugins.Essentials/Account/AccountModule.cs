// NosEmu
// 


using System.Threading.Tasks;
using PhoenixLib.MultiLanguage;
using Qmmands;
using WingsAPI.Communication;
using WingsAPI.Communication.ServerApi;
using WingsAPI.Communication.ServerApi.Protocol;
using WingsAPI.Packets.Enums;
using WingsEmu.Commands.Checks;
using WingsEmu.Commands.Entities;
using WingsEmu.DTOs.Account;
using WingsEmu.DTOs.Bonus;
using WingsEmu.Game._i18n;
using WingsEmu.Game.Characters;
using WingsEmu.Game.Characters.Events;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Groups.Events;
using WingsEmu.Game.Managers;
using WingsEmu.Game.Networking;
using WingsEmu.Game.Relations;
using WingsEmu.Packets.ClientPackets;
using WingsEmu.Packets.Enums;
using WingsEmu.Packets.Enums.Chat;

namespace WingsEmu.Plugins.Essentials.Account;

[Name("Account")]
[Description("Module related to account commands.")]
[RequireAuthority(AuthorityType.User)]
public class AccountModule : SaltyModuleBase
{
    private readonly IGameLanguageService _language;
    private readonly IServerManager _manager;
    private readonly ISessionManager _sessionManager;
    private readonly SerializableGameServer _gameServer;
    private readonly IServerApiService _serverApiService;

    public AccountModule(IServerManager manager, IGameLanguageService language, ISessionManager sessionManager, SerializableGameServer gameServer, IServerApiService serverApiService)
    {
        _manager = manager;
        _language = language;
        _sessionManager = sessionManager;
        _gameServer = gameServer;
        _serverApiService = serverApiService;
    }

    [Command("autoloot")]
    [Description("Enable/Disable your autoloot")]
    public async Task<SaltyCommandResult> HandleAutoLoot()
    {
        IClientSession player = Context.Player;

        if (!player.PlayerEntity.HaveStaticBonus(StaticBonusType.AutoLoot))
        {
            return new SaltyCommandResult(false, "You do not have AutoLoot.");
        }
        
        if (player.PlayerEntity.HasAutoLootEnabled)
        {
            player.PlayerEntity.HasAutoLootEnabled = false;
            return new SaltyCommandResult(true, "AutoLoot is now off.");
        }

        player.PlayerEntity.HasAutoLootEnabled = true;
        return new SaltyCommandResult(true, "AutoLoot is now on.");
    }


    [Command("language", "lang", "getlang", "getlanguage")]
    [Description("Check your language")]
    public async Task<SaltyCommandResult> GetLanguage()
    {
        IClientSession player = Context.Player;
        player.SendChatMessage($"{_language.GetLanguage(GameDialogKey.COMMAND_CHATMESSAGE_LANGUAGE_CURRENT, player.UserLanguage)} {player.UserLanguage.ToString()}", ChatMessageColorType.Yellow);
        player.SendDiscordRpcPacket();
        return new SaltyCommandResult(true, "");
    }

    [Command("language", "lang", "setlang", "setlanguage")]
    [Description("Sets your language")]
    public async Task<SaltyCommandResult> SetAccountLanguage([Description("EN, FR, CZ, PL, DE, IT, ES, TR")] RegionLanguageType languageType)
    {
        IClientSession player = Context.Player;
        player.Account.ChangeLanguage(languageType);
        player.SendChatMessage($"{_language.GetLanguage(GameDialogKey.COMMAND_CHATMESSAGE_LANGUAGE_CHANGED, player.UserLanguage)} {languageType.ToString()}", ChatMessageColorType.Yellow);
        player.SendDiscordRpcPacket();
        return new SaltyCommandResult(true, "");
    }

    [Command("invite")]
    public async Task<SaltyCommandResult> InviteAsync([Remainder] string nickname)
    {
        await Context.Player.EmitEventAsync(new InviteJoinMinilandEvent(nickname, true));
        return new SaltyCommandResult(true);
    }

    [Command("fl")]
    [Description("Send friend request to the player")]
    public async Task<SaltyCommandResult> FriendRequestAsync([Remainder] [Description("Player nickname")] string nickname)
    {
        IClientSession session = Context.Player;
        IPlayerEntity target = _sessionManager.GetSessionByCharacterName(nickname)?.PlayerEntity;
        if (target == null)
        {
            session.SendMsg(_language.GetLanguage(GameDialogKey.INFORMATION_MESSAGE_USER_NOT_FOUND, session.UserLanguage), MsgMessageType.Middle);
            return new SaltyCommandResult(true);
        }

        await session.EmitEventAsync(new RelationFriendEvent
        {
            RequestType = FInsPacketType.INVITE,
            CharacterId = target.Id
        });

        return new SaltyCommandResult(true);
    }

    [Command("bl")]
    [Description("Block the player.")]
    public async Task<SaltyCommandResult> BlockRequestAsync([Remainder] [Description("Player nickname")] string nickname)
    {
        IClientSession session = Context.Player;
        IPlayerEntity target = _sessionManager.GetSessionByCharacterName(nickname)?.PlayerEntity;
        if (target == null)
        {
            session.SendMsg(_language.GetLanguage(GameDialogKey.INFORMATION_MESSAGE_USER_NOT_FOUND, session.UserLanguage), MsgMessageType.Middle);
            return new SaltyCommandResult(true);
        }

        await session.EmitEventAsync(new RelationBlockEvent
        {
            CharacterId = target.Id
        });

        return new SaltyCommandResult(true);
    }

    [Command("pinv")]
    [Description("Send group request to the player")]
    public async Task<SaltyCommandResult> GroupRequestAsync([Remainder] [Description("Player's nickname")] string nickname)
    {
        IClientSession session = Context.Player;
        IPlayerEntity target = _sessionManager.GetSessionByCharacterName(nickname)?.PlayerEntity;
        if (target == null)
        {
            session.SendMsg(_language.GetLanguage(GameDialogKey.INFORMATION_MESSAGE_USER_NOT_FOUND, session.UserLanguage), MsgMessageType.Middle);
            return new SaltyCommandResult(true);
        }

        await session.EmitEventAsync(new GroupActionEvent
        {
            RequestType = GroupRequestType.Requested,
            CharacterId = target.Id
        });

        return new SaltyCommandResult(true);
    }

    [Command("fcancel")]
    public async Task<SaltyCommandResult> CancelFightMode()
    {
        Context.Player.PlayerEntity.CancelCastingSkill();
        return new SaltyCommandResult(true);
    }
    
    [Command("marathon")]
    public async Task<SaltyCommandResult> MarathonMode()
    {
        IClientSession session = Context.Player;

        if (!session.PlayerEntity.IsInRaidParty)
        {
            session.SendMsg(_language.GetLanguage(GameDialogKey.RAID_INFO_NO_EXIST, session.UserLanguage), MsgMessageType.Middle);
            session.SendChatMessage(_language.GetLanguage(GameDialogKey.RAID_INFO_NO_EXIST, session.UserLanguage), ChatMessageColorType.Red);
            return new SaltyCommandResult(false);
        }

        if (!session.PlayerEntity.IsRaidLeader(session.PlayerEntity.Id))
        {
            session.SendMsg(_language.GetLanguage(GameDialogKey.NOT_RAID_LEADER, session.UserLanguage), MsgMessageType.Middle);
            session.SendChatMessage(_language.GetLanguage(GameDialogKey.NOT_RAID_LEADER, session.UserLanguage), ChatMessageColorType.Red);
            return new SaltyCommandResult(false);
        }

        session.PlayerEntity.Raid.IsMarathonMode = !session.PlayerEntity.Raid.IsMarathonMode;

        GameDialogKey messageKey = session.PlayerEntity.Raid.IsMarathonMode ? GameDialogKey.MARATHON_MODE_ENABLED : GameDialogKey.MARATHON_MODE_DISABLED;
        string message = _language.GetLanguageFormat(messageKey, session.UserLanguage, session.PlayerEntity.Name);

        foreach (IClientSession raidMember in session.PlayerEntity.Raid.Members)
        {
            raidMember.PlayerEntity.Raid.IsMarathonMode = session.PlayerEntity.Raid.IsMarathonMode;
            raidMember.SendMsg(message, MsgMessageType.Middle);
            raidMember.SendChatMessage(message, ChatMessageColorType.Red);
        }

        return new SaltyCommandResult(true);
    }
    
    [Command("change-channel")]
    public async Task<SaltyCommandResult> ChangeChannelUser(int channelId)
    {
        if (channelId == _gameServer.ChannelId)
        {
            return new SaltyCommandResult(false, "It's the same channel");
        }

        if (channelId == 51 || _gameServer.ChannelType == GameChannelType.ACT_4)
        {
            return new SaltyCommandResult(false, "Use $act4/$act4leave command instead");
        }

        IClientSession session = Context.Player;

        GetChannelInfoResponse response = await _serverApiService.GetChannelInfo(new GetChannelInfoRequest
        {
            WorldGroup = _gameServer.WorldGroup,
            ChannelId = channelId
        });

        if (response?.ResponseType != RpcResponseType.SUCCESS)
        {
            return new SaltyCommandResult(false, "Channel doesn't exist");
        }

        IPlayerEntity player = session.PlayerEntity;

        await session.EmitEventAsync(new PlayerChangeChannelEvent(response.GameServer, ItModeType.ToPortAlveus, player.MapId, player.MapX, player.MapY));
        return new SaltyCommandResult(true);
    }
}