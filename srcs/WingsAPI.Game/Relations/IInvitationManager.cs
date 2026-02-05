using WingsEmu.Packets.Enums;

namespace WingsEmu.Game.Relations;

public interface IInvitationManager
{
    bool ContainsPendingInvitation(long invitationSender, long invitationTarget, InvitationType type);
    void AddPendingInvitation(long invitationSender, long invitationTarget, InvitationType type);
    void RemovePendingInvitation(long invitationSender, long invitationTarget, InvitationType type);

    /// <summary>
    ///     Mainly used on disconnect
    /// </summary>
    /// <param name="invitationSender"></param>
    void RemoveAllPendingInvitations(long invitationSender);
}