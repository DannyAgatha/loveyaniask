using System.Threading.Tasks;
using WingsEmu.Game._i18n;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Groups;
using WingsEmu.Game.Managers;
using WingsEmu.Game.Networking;
using WingsEmu.Game.Networking.Broadcasting;
using WingsEmu.Packets.ClientPackets;
using WingsEmu.Packets.Enums;
using WingsEmu.Packets.Enums.Character;
using WingsEmu.Packets.Enums.Chat;

namespace WingsEmu.Plugins.PacketHandling.Game.Basic;

public class CharacterOptionPacketHandler : GenericGamePacketHandlerBase<CharacterOptionPacket>
{
    private readonly IGameLanguageService _language;
    private readonly ISessionManager _sessionManager;

    public CharacterOptionPacketHandler(IGameLanguageService language, ISessionManager sessionManager)
    {
        _language = language;
        _sessionManager = sessionManager;
    }

    protected override async Task HandlePacketAsync(IClientSession session, CharacterOptionPacket characterOptionPacket)
    {
        if (characterOptionPacket == null)
        {
            return;
        }

        switch (characterOptionPacket.Option)
        {
            case CharacterOption.BuffBlocked:
                session.PlayerEntity.BuffBlocked = characterOptionPacket.IsActive;
                session.SendMsg(_language.GetLanguage(
                        session.PlayerEntity.BuffBlocked ? GameDialogKey.OPTIONS_SHOUTMESSAGE_BUFF_DISABLED : GameDialogKey.OPTIONS_SHOUTMESSAGE_BUFF_ENABLED, session.UserLanguage),
                    MsgMessageType.Middle);
                break;

            case CharacterOption.EmoticonsBlocked:
                session.PlayerEntity.EmoticonsBlocked = characterOptionPacket.IsActive;
                session.SendMsg(_language.GetLanguage(
                        session.PlayerEntity.EmoticonsBlocked ? GameDialogKey.OPTIONS_SHOUTMESSAGE_EMOTE_DISABLED : GameDialogKey.OPTIONS_SHOUTMESSAGE_EMOTE_ENABLED, session.UserLanguage),
                    MsgMessageType.Middle);
                break;

            case CharacterOption.ExchangeBlocked:
                session.PlayerEntity.ExchangeBlocked = !characterOptionPacket.IsActive;
                session.SendMsg(_language.GetLanguage(
                        session.PlayerEntity.ExchangeBlocked ? GameDialogKey.OPTIONS_SHOUTMESSAGE_TRADE_DISABLED : GameDialogKey.OPTIONS_SHOUTMESSAGE_TRADE_ENABLED, session.UserLanguage),
                    MsgMessageType.Middle);
                break;

            case CharacterOption.FriendRequestBlocked:
                session.PlayerEntity.FriendRequestBlocked = !characterOptionPacket.IsActive;
                session.SendMsg(_language.GetLanguage(
                    session.PlayerEntity.FriendRequestBlocked ? GameDialogKey.OPTIONS_SHOUTMESSAGE_FRIEND_REQUESTS_DISABLED : GameDialogKey.OPTIONS_SHOUTMESSAGE_FRIEND_REQUESTS_ENABLED,
                    session.UserLanguage), MsgMessageType.Middle);
                break;

            case CharacterOption.GroupRequestBlocked:
                session.PlayerEntity.GroupRequestBlocked = !characterOptionPacket.IsActive;
                session.SendMsg(_language.GetLanguage(
                        session.PlayerEntity.GroupRequestBlocked ? GameDialogKey.OPTIONS_SHOUTMESSAGE_GROUP_REQUESTS_DISABLED : GameDialogKey.OPTIONS_SHOUTMESSAGE_GROUP_REQUESTS_ENABLED,
                        session.UserLanguage),
                    MsgMessageType.Middle);
                break;

            case CharacterOption.PetAutoRelive:
                session.PlayerEntity.IsPetAutoRelive = characterOptionPacket.IsActive;
                session.SendMsg(_language.GetLanguage(
                    session.PlayerEntity.IsPetAutoRelive ? GameDialogKey.OPTIONS_SHOUTMESSAGE_PET_AUTO_RELIVE_ENABLED : GameDialogKey.OPTIONS_SHOUTMESSAGE_PET_AUTO_RELIVE_DISABLED,
                    session.UserLanguage), MsgMessageType.Middle);
                break;

            case CharacterOption.PartnerAutoRelive:
                session.PlayerEntity.IsPartnerAutoRelive = characterOptionPacket.IsActive;
                session.SendMsg(_language.GetLanguage(
                    session.PlayerEntity.IsPartnerAutoRelive ? GameDialogKey.OPTIONS_SHOUTMESSAGE_PARTNER_AUTO_RELIVE_ENABLED : GameDialogKey.OPTIONS_SHOUTMESSAGE_PARTNER_AUTO_RELIVE_DISABLED,
                    session.UserLanguage), MsgMessageType.Middle);
                break;

            case CharacterOption.HeroChatBlocked:
                session.PlayerEntity.HeroChatBlocked = characterOptionPacket.IsActive;
                session.SendMsg(_language.GetLanguage(
                        session.PlayerEntity.HeroChatBlocked ? GameDialogKey.OPTIONS_SHOUTMESSAGE_SPEAKERS_DISABLED : GameDialogKey.OPTIONS_SHOUTMESSAGE_SPEAKERS_ENABLED, session.UserLanguage),
                    MsgMessageType.Middle);
                break;

            case CharacterOption.HpBlocked:
                session.PlayerEntity.HpBlocked = characterOptionPacket.IsActive;
                session.SendMsg(_language.GetLanguage(
                    session.PlayerEntity.HpBlocked ? GameDialogKey.OPTIONS_SHOUTMESSAGE_HP_DISABLED : GameDialogKey.OPTIONS_SHOUTMESSAGE_HP_ENABLED, session.UserLanguage), MsgMessageType.Middle);
                break;

            case CharacterOption.MinilandInviteBlocked:
                session.PlayerEntity.MinilandInviteBlocked = characterOptionPacket.IsActive;
                session.SendMsg(_language.GetLanguage(
                        session.PlayerEntity.MinilandInviteBlocked ? GameDialogKey.OPTIONS_SHOUTMESSAGE_MINILAND_INVITES_DISABLED : GameDialogKey.OPTIONS_SHOUTMESSAGE_MINILAND_INVITES_ENABLED,
                        session.UserLanguage),
                    MsgMessageType.Middle);
                break;

            case CharacterOption.MouseAimLock:
                session.PlayerEntity.MouseAimLock = characterOptionPacket.IsActive;
                session.SendMsg(_language.GetLanguage(
                        session.PlayerEntity.MouseAimLock ? GameDialogKey.OPTIONS_SHOUTMESSAGE_MOUSE_ENABLED : GameDialogKey.OPTIONS_SHOUTMESSAGE_MOUSE_DISABLED, session.UserLanguage),
                    MsgMessageType.Middle);
                break;

            case CharacterOption.QuickGetUp:
                session.PlayerEntity.QuickGetUp = characterOptionPacket.IsActive;
                session.SendMsg(_language.GetLanguage(
                        session.PlayerEntity.QuickGetUp ? GameDialogKey.OPTIONS_SHOUTMESSAGE_QUICK_GET_UP_ENABLED : GameDialogKey.OPTIONS_SHOUTMESSAGE_QUICK_GET_UP_DISABLED, session.UserLanguage),
                    MsgMessageType.Middle);
                break;

            case CharacterOption.WhisperBlocked:
                session.PlayerEntity.WhisperBlocked = !characterOptionPacket.IsActive;
                session.SendMsg(_language.GetLanguage(
                        session.PlayerEntity.WhisperBlocked ? GameDialogKey.OPTIONS_SHOUTMESSAGE_WHISPER_DISABLED : GameDialogKey.OPTIONS_SHOUTMESSAGE_WHISPER_ENABLED, session.UserLanguage),
                    MsgMessageType.Middle);
                break;

            case CharacterOption.FamilyRequestBlocked:
                session.PlayerEntity.FamilyRequestBlocked = !characterOptionPacket.IsActive;
                session.SendMsg(_language.GetLanguage(
                    session.PlayerEntity.FamilyRequestBlocked ? GameDialogKey.OPTIONS_SHOUTMESSAGE_FAMILY_REQUESTS_DISABLED : GameDialogKey.OPTIONS_SHOUTMESSAGE_FAMILY_REQUESTS_ENABLED,
                    session.UserLanguage), MsgMessageType.Middle);
                break;

            case CharacterOption.HideHat:
                session.PlayerEntity.HideHat = !characterOptionPacket.IsActive;
                session.SendMsg(
                    _language.GetLanguage(session.PlayerEntity.HideHat ? GameDialogKey.OPTIONS_SHOUTMESSAGE_HIDE_HAT_ENABLED : GameDialogKey.OPTIONS_SHOUTMESSAGE_HIDE_HAT_DISABLED,
                        session.UserLanguage), MsgMessageType.Middle);
                session.BroadcastEq();
                break;

            case CharacterOption.UiBlocked:
                session.PlayerEntity.UiBlocked = !characterOptionPacket.IsActive;
                session.SendMsg(
                    _language.GetLanguage(session.PlayerEntity.UiBlocked ? GameDialogKey.OPTIONS_SHOUTMESSAGE_UI_DISABLED : GameDialogKey.OPTIONS_SHOUTMESSAGE_UI_ENABLED, session.UserLanguage),
                    MsgMessageType.Middle);
                break;

            case CharacterOption.GroupSharing:
                if (!session.PlayerEntity.IsInGroup())
                {
                    return;
                }

                PlayerGroup grp = session.PlayerEntity.GetGroup();

                if (!session.PlayerEntity.IsLeaderOfGroup(session.PlayerEntity.Id))
                {
                    session.SendMsg(_language.GetLanguage(GameDialogKey.GROUP_SHOUTMESSAGE_YOU_ARE_NOT_LEADER, session.UserLanguage), MsgMessageType.Middle);
                    return;
                }

                if (characterOptionPacket.IsActive == false)
                {
                    grp.SharingMode = GroupSharingType.Everyone;

                    await _sessionManager.BroadcastAsync(async g =>
                    {
                        string message = _language.GetLanguage(GameDialogKey.GROUP_SHOUTMESSAGE_SHARING, g.UserLanguage);
                        return g.GenerateMsgPacket(message, MsgMessageType.Middle);
                    }, new GroupBroadcast(grp));
                }
                else
                {
                    grp.SharingMode = GroupSharingType.ByOrder;

                    await _sessionManager.BroadcastAsync(async g =>
                    {
                        string message = _language.GetLanguage(GameDialogKey.GROUP_SHOUTMESSAGE_SHARING_BY_ORDER, g.UserLanguage);
                        return g.GenerateMsgPacket(message, MsgMessageType.Middle);
                    }, new GroupBroadcast(grp));
                }

                break;

            case CharacterOption.HideCD:
                session.PlayerEntity.HideCD = !characterOptionPacket.IsActive;
                session.SendMsg(
                    _language.GetLanguage(session.PlayerEntity.HideCD ? GameDialogKey.OPTIONS_SHOUTMESSAGE_HIDE_CD_ENABLED : GameDialogKey.OPTIONS_SHOUTMESSAGE_HIDE_CD_DISABLED,
                        session.UserLanguage), MsgMessageType.Middle);
                break;

            case CharacterOption.HideHPMP:
                session.PlayerEntity.HideHPMP = !characterOptionPacket.IsActive;
                session.SendMsg(
                    _language.GetLanguage(session.PlayerEntity.HideHPMP ? GameDialogKey.OPTIONS_SHOUTMESSAGE_HIDE_HPMP_ENABLED : GameDialogKey.OPTIONS_SHOUTMESSAGE_HIDE_HPMP_DISABLED,
                        session.UserLanguage), MsgMessageType.Middle);
                break;
        }

        session.RefreshStat();
    }
}