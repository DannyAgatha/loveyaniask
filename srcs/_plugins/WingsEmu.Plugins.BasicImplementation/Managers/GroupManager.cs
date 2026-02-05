using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using PhoenixLib.Logging;
using WingsAPI.Game.Extensions.Groups;
using WingsEmu.Game._ItemUsage.Configuration;
using WingsEmu.Game.Characters;
using WingsEmu.Game.Groups;

namespace NosEmu.Plugins.BasicImplementations.Managers;

public sealed class GroupManager : IGroupManager
{
    private readonly List<PlayerGroup> _groups;
    private readonly ConcurrentQueue<(IPlayerEntity, PlayerGroup)> _groupsToAdd = new();
    private readonly ConcurrentQueue<(IPlayerEntity, PlayerGroup)> _groupsToRemove = new();
    private readonly ConcurrentQueue<(long, PlayerGroup)> _leaderToChange = new();
    private readonly ConcurrentQueue<(IPlayerEntity, PlayerGroup)> _playersToAdd = new();
    private readonly ConcurrentQueue<(IPlayerEntity, PlayerGroup)> _playersToRemove = new();

    private readonly ISpPartnerConfiguration _spPartnerConfiguration;
    private int _lastGroupId;
    private DateTime _lastGroupUiRefresh;

    public GroupManager(ISpPartnerConfiguration spPartnerConfiguration)
    {
        _spPartnerConfiguration = spPartnerConfiguration;
        _groups = new List<PlayerGroup>();
        _lastGroupUiRefresh = DateTime.MinValue;
        Id = Guid.NewGuid();
    }

    public Guid Id { get; }
    public string Name => nameof(GroupManager);

    public void ProcessTick(DateTime date)
    {
        RemoveGroup();
        AddNewGroup();

        PlayerRemoveFromGroup();
        PlayerAddToGroup();
        ChangeLeader();

        if (_lastGroupUiRefresh.AddSeconds(1) > date)
        {
            return;
        }

        _lastGroupUiRefresh = DateTime.UtcNow;
        var stopWatch = Stopwatch.StartNew();

        ProcessGroupRefresh();

        stopWatch.Stop();
    }

    public int GetNextGroupId()
    {
        Interlocked.Increment(ref _lastGroupId);
        return _lastGroupId;
    }

    public void JoinGroup(PlayerGroup group, IPlayerEntity character)
    {
        _groupsToAdd.Enqueue((character, group));
    }

    public void RemoveGroup(PlayerGroup group, IPlayerEntity character)
    {
        _groupsToRemove.Enqueue((character, group));
    }

    public void AddMemberGroup(PlayerGroup group, IPlayerEntity character)
    {
        _playersToAdd.Enqueue((character, group));
    }

    public void RemoveMemberGroup(PlayerGroup group, IPlayerEntity character)
    {
        _playersToRemove.Enqueue((character, group));
    }

    public void ChangeLeader(PlayerGroup group, long newLeaderId)
    {
        _leaderToChange.Enqueue((newLeaderId, group));
    }

    private void ProcessGroupRefresh()
    {
        foreach (PlayerGroup group in _groups)
        {
            try
            {
                IReadOnlyList<IPlayerEntity> groupMembers = group.Members;
                foreach (IPlayerEntity member in groupMembers)
                {
                    member.Session.RefreshPartyUi();
                }
            }
            catch (Exception e)
            {
                Log.Error("Group.RefreshMembers", e);
            }
        }
    }

    private void RemoveGroup()
    {
        while (_groupsToRemove.TryDequeue(out (IPlayerEntity, PlayerGroup) playerGroup))
        {
            PlayerGroup group = playerGroup.Item2;
            IPlayerEntity character = playerGroup.Item1;
            character.RemoveGroup();

            foreach (IPlayerEntity member in group.Members)
            {
                member.Session.RefreshParty(_spPartnerConfiguration);
                member.Session.BroadcastPidx();
            }

            _groups.Remove(group);
        }
    }

    private void AddNewGroup()
    {
        while (_groupsToAdd.TryDequeue(out (IPlayerEntity, PlayerGroup) playerGroup))
        {
            IPlayerEntity character = playerGroup.Item1;
            PlayerGroup group = playerGroup.Item2;
            _groups.Add(group);
            character.SetGroup(group);

            foreach (IPlayerEntity member in group.Members)
            {
                member.Session.RefreshParty(_spPartnerConfiguration);
                if (!member.IsLeaderOfGroup(member.Id))
                {
                    continue;
                }

                member.Session.BroadcastPidx();
            }
        }
    }

    private void PlayerRemoveFromGroup()
    {
        while (_playersToRemove.TryDequeue(out (IPlayerEntity, PlayerGroup) playerGroup))
        {
            IPlayerEntity character = playerGroup.Item1;
            PlayerGroup group = playerGroup.Item2;

            group.RemoveMember(character);
            character.RemoveGroup();

            character.Session.RefreshParty(_spPartnerConfiguration);
            character.Session.BroadcastPidx();
            foreach (IPlayerEntity member in group.Members)
            {
                member.Session.RefreshParty(_spPartnerConfiguration);
                member.Session.BroadcastPidx();
            }
        }
    }

    private void PlayerAddToGroup()
    {
        while (_playersToAdd.TryDequeue(out (IPlayerEntity character, PlayerGroup) playerGroup))
        {
            IPlayerEntity character = playerGroup.Item1;
            PlayerGroup group = playerGroup.Item2;

            character.SetGroup(group);
            group.AddMember(character);
            foreach (IPlayerEntity member in group.Members)
            {
                member.Session.RefreshParty(_spPartnerConfiguration);
                if (character.Id != member.Id)
                {
                    continue;
                }

                member.Session.BroadcastPidx();
            }
        }
    }

    private void ChangeLeader()
    {
        while (_leaderToChange.TryDequeue(out (long, PlayerGroup) playerGroup))
        {
            playerGroup.Item2.OwnerId = playerGroup.Item1;
        }
    }
}