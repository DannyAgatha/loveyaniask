using System.Collections.Generic;
using WingsEmu.Packets.Enums.Character;

namespace WingsEmu.Game.Configurations.SpecialistShardExchange;

public class SpecialistShardExchangeConfiguration
{
    public SpecialistShardExchangeRoot SpecialistShardExchange { get; set; } = new();
}

public class SpecialistShardExchangeRoot
{
    public List<SpecialistExchangeDefinition> Exchanges { get; set; } = [];
}

public class SpecialistExchangeDefinition
{
    public int Argument { get; set; }
    public List<SpecialistExchangeCost> Costs { get; set; } = [];
    public List<SpecialistExchangeReward> Rewards { get; set; } = [];
}

public class SpecialistExchangeCost
{
    public int ItemVnum { get; set; }
    public int Quantity { get; set; }
}

public class SpecialistExchangeReward
{
    public ClassType Class { get; set; }
    public int SpecialistVnum { get; set; }
}