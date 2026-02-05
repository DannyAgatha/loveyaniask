using System.ServiceModel;
using System.Threading.Tasks;

namespace WingsAPI.Communication.DbServer.CharacterService
{
    [ServiceContract]
    public interface ICharacterService
    {
        /*
         * CharacterSaves
         */
        [OperationContract]
        Task<DbServerSaveCharactersResponse> SaveCharacters(DbServerSaveCharactersRequest request);

        [OperationContract]
        Task<DbServerSaveCharacterResponse> SaveCharacter(DbServerSaveCharacterRequest request);

        [OperationContract]
        Task<DbServerSaveCharacterResponse> CreateCharacter(DbServerSaveCharacterRequest request);

        [OperationContract]
        Task<DbServerGetCharactersResponse> GetCharacters(DbServerGetCharactersRequest request);

        [OperationContract]
        Task<DbServerGetCharacterResponse> GetCharacterBySlot(DbServerGetCharacterFromSlotRequest fromSlotRequest);

        [OperationContract]
        Task<DbServerGetCharacterResponse> GetCharacterById(DbServerGetCharacterByIdRequest fromSlotRequest);

        [OperationContract]
        Task<DbServerGetCharacterResponse> GetCharacterByName(DbServerGetCharacterRequestByName request);

        [OperationContract]
        Task<DbServerFlushCharacterSavesResponse> FlushCharacterSaves(DbServerFlushCharacterSavesRequest request);

        [OperationContract]
        Task<DbServerDeleteCharacterResponse> DeleteCharacter(DbServerDeleteCharacterRequest request);

        [OperationContract]
        Task<DbServerGetCharacterResponse> ForceRemoveCharacterFromCache(DbServerGetCharacterRequestByName request);

        /*
         * Ranking
         */
        [OperationContract]
        ValueTask<CharacterGetTopResponse> GetTopCompliment(EmptyRpcRequest request);

        [OperationContract]
        ValueTask<CharacterGetTopResponse> GetTopPoints(EmptyRpcRequest request);

        [OperationContract]
        ValueTask<CharacterGetTopResponse> GetTopReputation(EmptyRpcRequest request);

        [OperationContract]
        ValueTask<CharacterRefreshRankingResponse> RefreshRanking(EmptyRpcRequest request);
    }
}