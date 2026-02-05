using System.Collections.Generic;
using PhoenixLib.ServiceBus;
using PhoenixLib.ServiceBus.Routing;
using WingsAPI.Data.Character;
using WingsAPI.Data.Families;

namespace WingsAPI.Communication.Player
{
    [MessageType("ranking.refresh")]
    public class RankingRefreshMessage : IMessage
    {
        public IReadOnlyList<CharacterDTO> TopReputation { get; init; }
        public IReadOnlyList<CharacterDTO> TopCompliment { get; init; }
        public IReadOnlyList<CharacterDTO> TopPoints { get; init; }
        public IReadOnlyList<FamilyDTO> GlobalFamily { get; init; }
    }
}