using System.Collections.Generic;
using System.Threading.Tasks;
using WingsAPI.Data.Character;
using WingsAPI.Data.Families;

namespace DatabaseServer.Managers
{
    public interface IRankingManager
    {
        /// <summary>
        ///     Retrieves a list of the top 30 characters in compliments
        /// </summary>
        /// <returns></returns>
        Task<IReadOnlyList<CharacterDTO>> GetTopCompliment();

        /// <summary>
        ///     Retrieves a list of the top 30 characters in points
        /// </summary>
        /// <returns></returns>
        Task<IReadOnlyList<CharacterDTO>> GetTopPoints();

        /// <summary>
        ///     Retrieves a list of the top 43 characters in reputation
        /// </summary>
        /// <returns></returns>
        Task<IReadOnlyList<CharacterDTO>> GetTopReputation();

        /// <summary>
        ///     Tries to refresh the ranking, in case it fails it will return false
        /// </summary>
        /// <returns></returns>
        Task<RefreshResponse> TryRefreshRanking();

        Task<IReadOnlyList<FamilyDTO>> GetAndUpdateFamily();
    }

    public class RefreshResponse
    {
        public bool Success { get; init; }

        public IReadOnlyList<CharacterDTO> TopCompliment { get; init; }
        public IReadOnlyList<CharacterDTO> TopPoints { get; init; }
        public IReadOnlyList<CharacterDTO> TopReputation { get; init; }
        public IReadOnlyList<FamilyDTO> FamilyRank { get; init; }
    }
}