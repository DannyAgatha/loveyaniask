using System.Collections.Generic;
using System.Collections.Immutable;
using WingsEmu.Core.Extensions;
using WingsEmu.Game._enum;

namespace WingsEmu.Game.Configurations;

public interface IBankReputationConfiguration
{
    public BankRankInfo GetBankRankInfo(ReputationType reputationType);
    public BankPenaltyInfo GetBankPenaltyInfo(ReputationType reputationType);
}

public class BankReputationConfiguration : IBankReputationConfiguration
{
    private readonly ImmutableDictionary<ReputationType, BankPenaltyInfo> _penaltiesByReputation;
    private readonly ImmutableDictionary<ReputationType, BankRankInfo> _ranksByReputation;

    public BankReputationConfiguration(BankReputationInfo bankReputationInfo)
    {
        var bankRankTypes = new Dictionary<ReputationType, BankRankInfo>();
        foreach (BankRankInfo bankRankInfo in bankReputationInfo.BankRanks)
        {
            foreach (ReputationType reputation in bankRankInfo.Reputations)
            {
                bankRankTypes.TryAdd(reputation, bankRankInfo);
            }
        }

        _ranksByReputation = bankRankTypes.ToImmutableDictionary();
        _penaltiesByReputation = bankReputationInfo.BankPenalties.ToImmutableDictionary(s => s.Reputation);
    }

    public BankRankInfo GetBankRankInfo(ReputationType reputationType) => _ranksByReputation.GetOrDefault(reputationType);
    public BankPenaltyInfo GetBankPenaltyInfo(ReputationType reputationType) => _penaltiesByReputation.GetOrDefault(reputationType);
}

public class BankReputationInfo
{
    public List<BankRankInfo> BankRanks { get; set; }
    public List<BankPenaltyInfo> BankPenalties { get; set; }
}

public class BankRankInfo
{
    public BankRankType BankRank { get; set; }
    public List<ReputationType> Reputations { get; set; }
}

public class BankPenaltyInfo
{
    public ReputationType Reputation { get; set; }
    public int GoldCost { get; set; }
}