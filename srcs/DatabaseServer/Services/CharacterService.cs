using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DatabaseServer.Managers;
using WingsAPI.Communication;
using WingsAPI.Communication.DbServer.CharacterService;
using WingsAPI.Data.Character;

namespace DatabaseServer.Services
{
    public class CharacterService : ICharacterService
    {
        private readonly ICharacterManager _characterManager;
        private readonly IRankingManager _rankingManager;

        public CharacterService(ICharacterManager characterManager, IRankingManager rankingManager)
        {
            _characterManager = characterManager;
            _rankingManager = rankingManager;
        }

        public async Task<DbServerSaveCharactersResponse> SaveCharacters(DbServerSaveCharactersRequest request)
        {
            await _characterManager.AddCharactersToSavingQueue(request.Characters);

            return new DbServerSaveCharactersResponse
            {
                RpcResponseType = RpcResponseType.SUCCESS
            };
        }

        // Nouvelle méthode pour sauvegarder immédiatement les personnages
        

        public async Task<DbServerSaveCharacterResponse> SaveCharacter(DbServerSaveCharacterRequest request)
        {
            await _characterManager.AddCharacterToSavingQueue(request.Character);
            return new DbServerSaveCharacterResponse
            {
                RpcResponseType = RpcResponseType.SUCCESS
            };
        }

        public async Task<DbServerSaveCharacterResponse> CreateCharacter(DbServerSaveCharacterRequest request)
        {
            CharacterDTO character = await _characterManager.CreateCharacter(request.Character, request.IgnoreSlotCheck);


            return new DbServerSaveCharacterResponse
            {
                RpcResponseType = character == null ? RpcResponseType.GENERIC_SERVER_ERROR : RpcResponseType.SUCCESS,
                Character = character
            };
        }

        public async Task<DbServerGetCharactersResponse> GetCharacters(DbServerGetCharactersRequest request)
        {
            IEnumerable<CharacterDTO> characters = await _characterManager.GetCharactersByAccountId(request.AccountId);

            return new DbServerGetCharactersResponse
            {
                RpcResponseType = characters == null ? RpcResponseType.GENERIC_SERVER_ERROR : RpcResponseType.SUCCESS,
                Characters = characters
            };
        }

        public async Task<DbServerGetCharacterResponse> GetCharacterBySlot(DbServerGetCharacterFromSlotRequest fromSlotRequest)
        {
            CharacterDTO character = await _characterManager.GetCharacterBySlot(fromSlotRequest.AccountId, fromSlotRequest.Slot);

            return new DbServerGetCharacterResponse
            {
                RpcResponseType = character == null ? RpcResponseType.GENERIC_SERVER_ERROR : RpcResponseType.SUCCESS,
                CharacterDto = character
            };
        }

        public async Task<DbServerGetCharacterResponse> GetCharacterById(DbServerGetCharacterByIdRequest request)
        {
            CharacterDTO character = await _characterManager.GetCharacterById(request.CharacterId);

            return new DbServerGetCharacterResponse
            {
                RpcResponseType = character == null ? RpcResponseType.GENERIC_SERVER_ERROR : RpcResponseType.SUCCESS,
                CharacterDto = character
            };
        }

        public async Task<DbServerGetCharacterResponse> GetCharacterByName(DbServerGetCharacterRequestByName request)
        {
            CharacterDTO character = await _characterManager.GetCharacterByName(request.CharacterName);

            return new DbServerGetCharacterResponse
            {
                RpcResponseType = character == null ? RpcResponseType.GENERIC_SERVER_ERROR : RpcResponseType.SUCCESS,
                CharacterDto = character
            };
        }

        public async Task<DbServerFlushCharacterSavesResponse> FlushCharacterSaves(DbServerFlushCharacterSavesRequest request)
        {
            await _characterManager.FlushCharacterSaves();
            return new DbServerFlushCharacterSavesResponse
            {
                RpcResponseType = RpcResponseType.SUCCESS
            };
        }

        public async Task<DbServerDeleteCharacterResponse> DeleteCharacter(DbServerDeleteCharacterRequest request)
        {
            bool success = await _characterManager.DeleteCharacter(request.CharacterDto);

            return new DbServerDeleteCharacterResponse
            {
                RpcResponseType = success ? RpcResponseType.SUCCESS : RpcResponseType.GENERIC_SERVER_ERROR
            };
        }

        public async Task<DbServerGetCharacterResponse> ForceRemoveCharacterFromCache(DbServerGetCharacterRequestByName request)
        {
            CharacterDTO character = await _characterManager.RemoveCachedCharacter(request.CharacterName);

            return new DbServerGetCharacterResponse
            {
                RpcResponseType = character == null ? RpcResponseType.GENERIC_SERVER_ERROR : RpcResponseType.SUCCESS,
                CharacterDto = character
            };
        }

        public async ValueTask<CharacterGetTopResponse> GetTopCompliment(EmptyRpcRequest request)
        {
            IReadOnlyList<CharacterDTO> top = await _rankingManager.GetTopCompliment();

            return new CharacterGetTopResponse
            {
                ResponseType = top == null ? RpcResponseType.GENERIC_SERVER_ERROR : RpcResponseType.SUCCESS,
                Top = top
            };
        }

        public async ValueTask<CharacterGetTopResponse> GetTopPoints(EmptyRpcRequest request)
        {
            IReadOnlyList<CharacterDTO> top = await _rankingManager.GetTopPoints();

            return new CharacterGetTopResponse
            {
                ResponseType = top == null ? RpcResponseType.GENERIC_SERVER_ERROR : RpcResponseType.SUCCESS,
                Top = top
            };
        }

        public async ValueTask<CharacterGetTopResponse> GetTopReputation(EmptyRpcRequest request)
        {
            IReadOnlyList<CharacterDTO> top = await _rankingManager.GetTopReputation();

            return new CharacterGetTopResponse
            {
                ResponseType = top == null ? RpcResponseType.GENERIC_SERVER_ERROR : RpcResponseType.SUCCESS,
                Top = top
            };
        }

        public async ValueTask<CharacterRefreshRankingResponse> RefreshRanking(EmptyRpcRequest request)
        {
            RefreshResponse response = await _rankingManager.TryRefreshRanking();

            return new CharacterRefreshRankingResponse
            {
                ResponseType = response.Success ? RpcResponseType.SUCCESS : RpcResponseType.GENERIC_SERVER_ERROR,
                TopCompliment = response.TopCompliment,
                TopPoints = response.TopPoints,
                TopReputation = response.TopReputation
            };
        }
    }
}