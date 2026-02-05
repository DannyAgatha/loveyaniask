// NosEmu
// 


using System.Threading.Tasks;
using WingsAPI.Game.Extensions.ItemExtension.Inventory;
using WingsAPI.Packets.Enums.Chat;
using WingsEmu.Game._enum;
using WingsEmu.Game._i18n;
using WingsEmu.Game.Characters.Events;
using WingsEmu.Game.Configurations;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Managers;
using WingsEmu.Game.Networking;
using WingsEmu.Packets.ClientPackets;
using WingsEmu.Packets.Enums;
using WingsEmu.Packets.Enums.Chat;
using WingsEmu.Packets.Enums.Language;

namespace WingsEmu.Plugins.PacketHandling.Game.Banks;

public class BankManagementPacketHandler : GenericGamePacketHandlerBase<GboxPacket>
{
    private readonly IBankReputationConfiguration _bankReputationConfiguration;
    private readonly IGameLanguageService _gameLanguage;
    private readonly IRankingManager _rankingManager;
    private readonly IReputationConfiguration _reputationConfiguration;
    private readonly IServerManager _serverManager;

    public BankManagementPacketHandler(IGameLanguageService gameLanguage, IServerManager serverManager, IReputationConfiguration reputationConfiguration,
        IBankReputationConfiguration bankReputationConfiguration, IRankingManager rankingManager)
    {
        _gameLanguage = gameLanguage;
        _serverManager = serverManager;
        _reputationConfiguration = reputationConfiguration;
        _bankReputationConfiguration = bankReputationConfiguration;
        _rankingManager = rankingManager;
    }

    protected override async Task HandlePacketAsync(IClientSession session, GboxPacket packet)
    {
        if (session.PlayerEntity.IsInExchange())
        {
            return;
        }

        if (session.PlayerEntity.HasShopOpened)
        {
            return;
        }

        if (session.PlayerEntity.HasNosBazaarOpen)
        {
            return;
        }

        if (!session.PlayerEntity.IsBankOpen)
        {
            await session.NotifyStrangeBehavior(StrangeBehaviorSeverity.ABUSING, "Tried to withdraw/deposit gold without opened bank.");
            return;
        }
        
        if (packet.Amount >= 100000000)
        {
            return;
        }

        if (packet.Amount <= 0)
        {
            return;
        }

        switch (packet.Type)
        {
            case BankActionType.Deposit:
                if (packet.Option == 0)
                {
                    session.SendQnai2Packet($"gbox 1 {packet.Amount} 1", Game18NConstString.AskDeposit, 1, packet.Amount);
                    return;
                }

                if (packet.Option == 1)
                {
                    if (session.Account.BankMoney + packet.Amount * 1000 > _serverManager.MaxBankGold)
                    {
                        session.SendInfoi(Game18NConstString.MaxGoldReached);
                        session.SendSMemoI2(SmemoType.BankError, Game18NConstString.MaxGoldReached, 0);
                        return;
                    }

                    if (session.PlayerEntity.Gold < packet.Amount * 1000)
                    {
                        session.SendInfoi(Game18NConstString.NotEnoughFounds);
                        session.SendSMemoI2(SmemoType.BankError, Game18NConstString.NotEnoughFounds, 0);
                        return;
                    }

                    session.PlayerEntity.Gold -= packet.Amount * 1000;
                    session.Account.BankMoney += packet.Amount * 1000;
                    session.RefreshGold();
                    session.SendGbPacket(BankType.Deposit, _reputationConfiguration, _bankReputationConfiguration, _rankingManager.TopReputation);
                    session.SendSMemoI2(SmemoType.BankInfo, Game18NConstString.DepositBank, 2, packet.Amount);
                    session.SendSayi2(EntityType.Mate, ChatMessageColorType.Green, Game18NConstString.BalanceBank, I18NArgumentType.Player, session.Account.BankMoney / 1000, session.PlayerEntity.Gold);
                    session.SendSMemoI2(SmemoType.BankBalance, Game18NConstString.BalanceBank, 3, session.Account.BankMoney / 1000, session.PlayerEntity.Gold, 1);
                }

                break;
            case BankActionType.Withdraw:
                if (packet.Option == 0)
                {
                    session.SendQnai2Packet($"gbox 2 {packet.Amount} 1", Game18NConstString.AskWithdraw, 2, packet.Amount, session.PlayerEntity.GetBankPenalty(_reputationConfiguration, _bankReputationConfiguration, _rankingManager.TopReputation));
                    return;
                }

                if (packet.Option == 1)
                {
                    if (session.PlayerEntity.Gold + packet.Amount * 1000 > _serverManager.MaxGold)
                    {
                        session.SendInfoi(Game18NConstString.MaxGoldReached);
                        session.SendSMemoI2(SmemoType.BankError, Game18NConstString.MaxGoldReached, 0);
                        return;
                    }

                    if (session.Account.BankMoney < packet.Amount * 1000)
                    {
                        session.SendInfoi(Game18NConstString.NotEnoughFounds);
                        session.SendSMemoI2(SmemoType.BankError, Game18NConstString.NotEnoughFounds, 0);
                        return;
                    }

                    if (!session.HasEnoughGold(session.PlayerEntity.GetBankPenalty(_reputationConfiguration, _bankReputationConfiguration, _rankingManager.TopReputation)))
                    {
                        session.SendInfoi(Game18NConstString.YouDontHaveAnyGoldToPay);
                        session.SendSMemoI2(SmemoType.BankError, Game18NConstString.YouDontHaveAnyGoldToPay, 0);
                        return;
                    }

                    session.PlayerEntity.Gold -= session.PlayerEntity.GetBankPenalty(_reputationConfiguration, _bankReputationConfiguration, _rankingManager.TopReputation);
                    session.Account.BankMoney -= packet.Amount * 1000;
                    session.PlayerEntity.Gold += packet.Amount * 1000;
                    session.RefreshGold();
                    session.SendGbPacket(BankType.Withdraw, _reputationConfiguration, _bankReputationConfiguration, _rankingManager.TopReputation);
                    session.SendSMemoI2(SmemoType.BankInfo, Game18NConstString.WithdrawBank, 3, packet.Amount, session.PlayerEntity.GetBankPenalty(_reputationConfiguration, _bankReputationConfiguration, _rankingManager.TopReputation));
                    session.SendSayi2(EntityType.Mate, ChatMessageColorType.Green, Game18NConstString.BalanceBank, I18NArgumentType.Player, session.Account.BankMoney / 1000, session.PlayerEntity.Gold);
                    session.SendSMemoI2(SmemoType.BankBalance, Game18NConstString.BalanceBank, 3, session.Account.BankMoney / 1000, session.PlayerEntity.Gold, 1);
                }

                break;
        }
    }
}