using System.Collections.Generic;
using WingsEmu.Packets.Enums;

namespace WingsEmu.Game.Relations;

public class InvitationManager : IInvitationManager
{
    // sender <--> type/targets
    private readonly Dictionary<long, HashSet<(long, InvitationType)>> _senderInvitations = new();

    // target <--> type/senders
    private readonly Dictionary<long, HashSet<(long, InvitationType)>> _targetInvitations = new();

    public bool ContainsPendingInvitation(long invitationSender, long invitationTarget, InvitationType type)
    {
        if (_senderInvitations.TryGetValue(invitationSender, out HashSet<(long, InvitationType)> senderDictionary))
        {
            return senderDictionary != null && senderDictionary.Contains((invitationTarget, type));
        }

        if (!_targetInvitations.TryGetValue(invitationTarget, out HashSet<(long, InvitationType)> targetDictionary))
        {
            return false;
        }

        return targetDictionary != null && targetDictionary.Contains((invitationTarget, type));
    }

    public void AddPendingInvitation(long invitationSender, long invitationTarget, InvitationType type)
    {
        AddSenderInvitation(invitationSender, invitationTarget, type);
        AddTargetInvitation(invitationSender, invitationTarget, type);
    }

    public void RemovePendingInvitation(long invitationSender, long invitationTarget, InvitationType type)
    {
        RemoveSenderInvitation(invitationSender, invitationTarget, type);
        RemoveTargetInvitation(invitationSender, invitationTarget, type);
    }

    public void RemoveAllPendingInvitations(long invitationSender)
    {
        if (!_senderInvitations.TryGetValue(invitationSender, out HashSet<(long, InvitationType)> dictionary))
        {
            return;
        }

        foreach ((long target, InvitationType key) in dictionary)
        {
            RemoveTargetInvitation(invitationSender, target, key);
        }

        dictionary.Clear();
    }

    private void AddSenderInvitation(long invitationSender, long invitationTarget, InvitationType type)
    {
        if (!_senderInvitations.TryGetValue(invitationSender, out HashSet<(long, InvitationType)> senderDictionary))
        {
            senderDictionary = new HashSet<(long, InvitationType)>();
            _senderInvitations[invitationSender] = senderDictionary;
        }

        if (senderDictionary.Contains((invitationTarget, type)))
        {
            return;
        }

        senderDictionary.Add((invitationTarget, type));
    }

    private void AddTargetInvitation(long invitationSender, long invitationTarget, InvitationType type)
    {
        if (!_targetInvitations.TryGetValue(invitationTarget, out HashSet<(long, InvitationType)> targetDictionary))
        {
            targetDictionary = new HashSet<(long, InvitationType)>();
            _targetInvitations[invitationTarget] = targetDictionary;
        }

        if (targetDictionary.Contains((invitationSender, type)))
        {
            return;
        }

        targetDictionary.Add((invitationSender, type));
    }

    private void RemoveSenderInvitation(long invitationSender, long invitationTarget, InvitationType type)
    {
        if (!_senderInvitations.TryGetValue(invitationSender, out HashSet<(long, InvitationType)> dictionary))
        {
            return;
        }

        dictionary.Remove((invitationTarget, type));
    }

    private void RemoveTargetInvitation(long invitationSender, long invitationTarget, InvitationType type)
    {
        if (!_targetInvitations.TryGetValue(invitationTarget, out HashSet<(long, InvitationType)> dictionary))
        {
            return;
        }

        dictionary.Remove((invitationSender, type));
    }
}