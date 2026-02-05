using PhoenixLib.DAL.Redis.Locks;
using PhoenixLib.Events;
using Plugin.FamilyImpl.Achievements;
using WingsAPI.Game.Extensions.ItemExtension.Inventory;
using WingsEmu.Game._i18n;
using WingsEmu.Game.Alzanor;
using WingsEmu.Game.Alzanor.Configurations;
using WingsEmu.Game.Alzanor.Events;
using WingsEmu.Game.Characters.Events;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Items;
using WingsEmu.Game.Managers;
using WingsEmu.Game.Maps;
using WingsEmu.Game.Networking;
using WingsEmu.Game.Raids;
using WingsEmu.Packets.Enums.Chat;

namespace Plugin.Alzanor.EventHandlers;

public class AlzanorEndEventHandler : IAsyncEventProcessor<AlzanorEndEvent>
{
    private readonly AlzanorConfiguration _alzanorConfiguration;
    private readonly IExpirableLockService _expirableLockService;
    private readonly IFamilyAchievementManager _familyAchievementManager;
    private readonly IFamilyMissionManager _familyMissionManager;
    private readonly IGameItemInstanceFactory _gameItemInstanceFactory;
    private readonly IAlzanorManager _alzanorManager;
    private readonly ISessionManager _sessionManager;
    
    public AlzanorEndEventHandler(AlzanorConfiguration alzanorConfiguration, IExpirableLockService expirableLockService, IFamilyAchievementManager familyAchievementManager, IFamilyMissionManager familyMissionManager, IGameItemInstanceFactory gameItemInstanceFactory, IAlzanorManager alzanorManager, ISessionManager sessionManager)
    {
        _alzanorConfiguration = alzanorConfiguration;
        _expirableLockService = expirableLockService;
        _familyAchievementManager = familyAchievementManager;
        _familyMissionManager = familyMissionManager;
        _gameItemInstanceFactory = gameItemInstanceFactory;
        _alzanorManager = alzanorManager;
        _sessionManager = sessionManager;
    }

    public async Task HandleAsync(AlzanorEndEvent e, CancellationToken cancellation)
    {
        _sessionManager.Broadcast(x => x.GenerateMsgPacket(x.GetLanguageFormat(GameDialogKey.ALZANOR_END), MsgMessageType.Middle));
        
        AlzanorParty alzanorParty = e.AlzanorParty;
        if (alzanorParty == null)
        {
            return;
        }

        if (alzanorParty.Winner.HasValue)
        {
            return;
        }

        alzanorParty.FinishTime = DateTime.UtcNow.AddSeconds(15);
        
        AlzanorTeamType winnerTeam = _alzanorManager.RedDamage > _alzanorManager.BlueDamage ? AlzanorTeamType.Red : AlzanorTeamType.Blue;

        alzanorParty.Winner = winnerTeam;
        await ProcessTopPlayers();
        
        switch (winnerTeam)
        {
            case AlzanorTeamType.Red:
                Console.WriteLine($"Winner [Red]: {winnerTeam.ToString()}");
                await ProcessWin(alzanorParty, AlzanorTeamType.Red);
                await ProcessLose(alzanorParty, AlzanorTeamType.Blue);
                IClientSession redDummy = alzanorParty.RedTeam.FirstOrDefault();
                if (redDummy != null)
                {
                    await redDummy.EmitEventAsync(new AlzanorWonEvent
                    {
                        Id = alzanorParty.Id,
                        Players = alzanorParty.RedTeam.Select(x => x.PlayerEntity.Id).ToArray()
                    });
                }

                IClientSession blueDummy = alzanorParty.BlueTeam.FirstOrDefault();
                if (blueDummy != null)
                {
                    await blueDummy.EmitEventAsync(new AlzanorLoseEvent
                    {
                        Id = alzanorParty.Id,
                        Players = alzanorParty.BlueTeam.Select(x => x.PlayerEntity.Id).ToArray()
                    });
                }

                return;
            case AlzanorTeamType.Blue:
                Console.WriteLine($"Winner [Blue]: {winnerTeam.ToString()}");
                await ProcessWin(alzanorParty, AlzanorTeamType.Blue);
                await ProcessLose(alzanorParty, AlzanorTeamType.Red);
                IClientSession redDummy1 = alzanorParty.RedTeam.FirstOrDefault();
                if (redDummy1 != null)
                {
                    await redDummy1.EmitEventAsync(new AlzanorLoseEvent
                    {
                        Id = alzanorParty.Id,
                        Players = alzanorParty.RedTeam.Select(x => x.PlayerEntity.Id).ToArray()
                    });
                }

                IClientSession blueDummy1 = alzanorParty.BlueTeam.FirstOrDefault();
                if (blueDummy1 != null)
                {
                    await blueDummy1.EmitEventAsync(new AlzanorWonEvent
                    {
                        Id = alzanorParty.Id,
                        Players = alzanorParty.BlueTeam.Select(x => x.PlayerEntity.Id).ToArray()
                    });
                }

                return;
        }
    }

    private async Task ProcessTopPlayers()
    {
        List<AlzanorEventStats>? topPlayers = _alzanorManager.GetTopPlayers();
        if (topPlayers == null)
        {
            return;
        }
        
        for (int i = 0; i < topPlayers.Count; i++)
        {
            int position = i + 1;
            AlzanorEventStats playerStats = topPlayers[i];
            
            TopPlayerRewards? reward = _alzanorConfiguration.TopPlayerRewards
                .FirstOrDefault(r => r.Position == position);
            
            if (reward != null)
            {
                await AddReward(playerStats.Player, reward.ItemId, reward.Amount);
                
                if (reward.Reputation > 0)
                {
                    await playerStats.Player.EmitEventAsync(new GenerateReputationEvent
                    {
                        Amount = reward.Reputation,
                        SendMessage = true
                    });
                }
            }
        }
    }


    private async Task ProcessLose(AlzanorParty alzanorParty, AlzanorTeamType team)
    {
        IReadOnlyList<IClientSession> members = team == AlzanorTeamType.Red ? alzanorParty.RedTeam : alzanorParty.BlueTeam;

        var processedPlayers = new HashSet<int>();
        foreach (IClientSession member in members)
        {
            member.SendMsg(member.GetLanguage(GameDialogKey.ALZANOR_MESSAGE_YOU_LOSE), MsgMessageType.Middle);
            member.SendChatMessage(member.GetLanguage(GameDialogKey.ALZANOR_MESSAGE_YOU_LOSE), ChatMessageColorType.Yellow);
            member.SendEmptyRaidBoss();

            if (!processedPlayers.Add(member.PlayerEntity.Id))
            {
                continue;
            }

            await ProcessRewards(member, false);
        }
    }

    private async Task ProcessWin(AlzanorParty alzanorParty, AlzanorTeamType team)
    {
        IReadOnlyList<IClientSession> members = team == AlzanorTeamType.Red ? alzanorParty.RedTeam : alzanorParty.BlueTeam;

        var processedPlayers = new HashSet<int>();
        foreach (IClientSession member in members)
        {
            member.SendMsg(member.GetLanguage(GameDialogKey.ALZANOR_MESSAGE_YOU_WON), MsgMessageType.Middle);
            member.SendChatMessage(member.GetLanguage(GameDialogKey.ALZANOR_MESSAGE_YOU_WON), ChatMessageColorType.Yellow);

            if (!processedPlayers.Add(member.PlayerEntity.Id))
            {
                continue;
            }

            await ProcessRewards(member, true);
        }
    }
    
    private async Task ProcessRewards(IClientSession session, bool isWinner)
    {
        Rewards[]? rewards = isWinner ? _alzanorConfiguration.WinRewards : _alzanorConfiguration.LoseRewards;
        
        foreach (Rewards reward in rewards)
        {
            await AddReward(session, reward.ItemId, reward.Amount);

            if (reward.Reputation > 0)
            {
                await session.EmitEventAsync(new GenerateReputationEvent
                {
                    Amount = reward.Reputation,
                    SendMessage = true
                });
            }
        }
    }

    private async Task AddReward(IClientSession session, int itemVnum, int quantity)
    {
        GameItemInstance rewards = _gameItemInstanceFactory.CreateItem(itemVnum, quantity);
        await session.AddNewItemToInventory(rewards, true, sendGiftIsFull: true);
    }
}