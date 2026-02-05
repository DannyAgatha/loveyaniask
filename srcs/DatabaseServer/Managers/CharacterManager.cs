using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using PhoenixLib.Caching;
using PhoenixLib.Logging;
using WingsAPI.Data.Character;
using WingsEmu.DTOs.Enums;

namespace DatabaseServer.Managers
{
    public class CharacterManager : BackgroundService, ICharacterManager
    {
        private static readonly TimeSpan Interval = TimeSpan.FromSeconds(Convert.ToUInt32(Environment.GetEnvironmentVariable(EnvironmentConsts.DbServerCharSaveIntervalSeconds) ?? "60"));
        private static readonly TimeSpan LifeTime = TimeSpan.FromMinutes(Convert.ToUInt32(Environment.GetEnvironmentVariable(EnvironmentConsts.DbServerCharTtlMinutes) ?? "30"));

        private readonly ILongKeyCachedRepository<CharacterDTO> _characterById;

        private readonly ICharacterDAO _characterDao;
        private readonly IKeyValueCache<long> _characterIdByKey;

        private readonly HashSet<long> _characterIdsToSave = new();
        private readonly SemaphoreSlim _createCharacterSemaphore = new(1, 1);
        private readonly SemaphoreSlim _semaphoreSlim = new(1, 1);

        public CharacterManager(ICharacterDAO characterDao, ILongKeyCachedRepository<CharacterDTO> characterById, IKeyValueCache<long> characterIdByKey)
        {
            _characterDao = characterDao;
            _characterById = characterById;
            _characterIdByKey = characterIdByKey;
        }

        public async Task<IEnumerable<CharacterDTO>> GetCharactersByAccountId(long accountId) => await _characterDao.LoadByAccountAsync(accountId);

        public async Task<CharacterDTO> GetCharacterBySlot(long accountId, byte slot)
        {
            long characterId = _characterIdByKey.Get(GetKeyAccountIdSlot(accountId, slot));

            // check if the cache does have the value
            if (characterId != default)
            {
                Log.Debug($"[CHARACTER_SAVE_SYSTEM][GetCharacterBySlot] CharacterId fetched from cache via AccountId and Slot. AccountId: '{accountId.ToString()}' Slot: '{slot.ToString()}'");
                return await GetCharacterById(characterId);
            }

            CharacterDTO characterDto = await _characterDao.LoadBySlotAsync(accountId, slot);
            if (characterDto == null)
            {
                // shouldn't happen normally
                Log.Warn(
                    $"[CHARACTER_SAVE_SYSTEM][GetCharacterBySlot] The given AccountId and Slot does not pertain to any CharacterDTO in the db. AccountId: '{accountId.ToString()}' Slot: '{slot.ToString()}'");
                return null;
            }

            SetCharacter(characterDto);
            Log.Debug($"[CHARACTER_SAVE_SYSTEM][GetCharacterBySlot] fetched by AccountId and Slot. AccountId: '{accountId.ToString()}' Slot: '{slot.ToString()}'");
            return characterDto;
        }

        public async Task<CharacterDTO> GetCharacterById(long characterId)
        {
            CharacterDTO dto = _characterById.Get(characterId);
            if (dto != null)
            {
                Log.Debug($"[CHARACTER_SAVE_SYSTEM][GetCharacter] CharacterDTO fetched from cache via CharacterId: '{characterId.ToString()}'");
                return dto;
            }

            dto = await _characterDao.GetByIdAsync(characterId);

            if (dto == null)
            {
                Log.Warn($"[CHARACTER_SAVE_SYSTEM][GetCharacter] The given CharacterId does not pertain to any CharacterDTO in the db. CharacterId: '{characterId.ToString()}'");
                return null;
            }

            SetCharacter(dto);
            Log.Debug($"[CHARACTER_SAVE_SYSTEM][GetCharacter] {characterId.ToString()} fetched from DB cause was not existing in cache");
            return dto;
        }

        public async Task<CharacterDTO> GetCharacterByName(string name)
        {
            long characterId = _characterIdByKey.Get(GetKey(name));

            // check if the cache does have the value
            if (characterId != default)
            {
                Log.Debug($"[CHARACTER_SAVE_SYSTEM][GetCharacter] CharacterId fetched from cache through CharacterName. CharacterId: '{characterId.ToString()}' CharacterName: '{name}'");
                return await GetCharacterById(characterId);
            }

            CharacterDTO characterDto = await _characterDao.LoadByNameAsync(name);
            if (characterDto == null)
            {
                // shouldn't happen normally
                Log.Warn($"[CHARACTER_SAVE_SYSTEM][GetCharacter] The given CharacterName does not exist in the db. CharacterName: '{name}'");
                return null;
            }

            SetCharacter(characterDto);
            Log.Debug(
                $"[CHARACTER_SAVE_SYSTEM][GetCharacter] CharacterDTO fetched from DB through CharacterName cause it was not existing in cache. CharacterId: '{characterId.ToString()}' CharacterName: '{name}'");
            return characterDto;
        }

        public async Task<CharacterDTO> CreateCharacter(CharacterDTO characterDto, bool ignoreSlotCheck)
        {
            await _createCharacterSemaphore.WaitAsync();
            try
            {
                if (await _characterDao.LoadByNameAsync(characterDto.Name) != null)
                {
                    return null;
                }

                CharacterDTO character;
                if (ignoreSlotCheck == false)
                {
                    character = await GetCharacterBySlot(characterDto.AccountId, characterDto.Slot);
                    if (character != null)
                    {
                        Log.Warn("[CHARACTER_SAVE_SYSTEM][CreateCharacter] Found a character already in the desired slot." +
                            $"AccountId: '{character.AccountId.ToString()}' CharacterId: '{character.Id.ToString()}' Slot: '{character.Slot.ToString()}'");
                        return null;
                    }
                }

                character = await _characterDao.SaveAsync(characterDto);
                SetCharacter(character);
                Log.Debug(
                    $"[CHARACTER_SAVE_SYSTEM][CreateCharacter] Created a new character. AccountId: '{character.AccountId.ToString()}' CharacterId: '{character.Id.ToString()}' Slot: '{character.Slot.ToString()}'");
                return character;
            }
            finally
            {
                _createCharacterSemaphore.Release();
            }
        }

        public async Task AddCharacterToSavingQueue(CharacterDTO characterDto)
        {
            SetCharacter(characterDto);

            await _semaphoreSlim.WaitAsync();
            try
            {
                _characterIdsToSave.Add(characterDto.Id);
            }
            finally
            {
                _semaphoreSlim.Release();
            }

            Log.Debug($"[CHARACTER_SAVE_SYSTEM][AddCharacterToSavingQueue] Flagged CharacterId for saving. CharacterId: '{characterDto.Id.ToString()}'");
        }

        public async Task AddCharactersToSavingQueue(IEnumerable<CharacterDTO> characterDtos)
        {
            int i = 0;

            await _semaphoreSlim.WaitAsync();
            try
            {
                foreach (CharacterDTO characterDto in characterDtos)
                {
                    SetCharacter(characterDto);
                    _characterIdsToSave.Add(characterDto.Id);
                    i++;
                }
            }
            finally
            {
                _semaphoreSlim.Release();
            }

            Log.Debug($"[CHARACTER_SAVE_SYSTEM][AddCharactersToSavingQueue] Flagged {i.ToString()} characterIds for saving.");
        }

        public async Task<bool> DeleteCharacter(CharacterDTO characterDto)
        {
            DeleteResult result = await _characterDao.DeleteByPrimaryKey(characterDto.AccountId, characterDto.Slot);
            if (result != DeleteResult.Deleted)
            {
                Log.Warn(
                    $"[CHARACTER_SAVE_SYSTEM][DeleteCharacter] Tried to delete a character that doesn't exist. AccountId: '{characterDto.AccountId.ToString()}' Slot: '{characterDto.Slot.ToString()}");
                return false;
            }

            RemoveCharacter(characterDto);
            Log.Debug(
                $"[CHARACTER_SAVE_SYSTEM][DeleteCharacter] Deleted a character. AccountId: '{characterDto.AccountId.ToString()}' CharacterId: '{characterDto.Id.ToString()}' Slot: '{characterDto.Slot.ToString()}'");
            return true;
        }

        public async Task<int> FlushCharacterSaves()
        {
            if (_characterIdsToSave.Count < 1)
            {
                return 0;
            }

            List<long> unsavedCharacterIds = new();
            long[] characterIds;

            await _semaphoreSlim.WaitAsync();
            try
            {
                characterIds = new long[_characterIdsToSave.Count];
                _characterIdsToSave.CopyTo(characterIds);
                _characterIdsToSave.Clear();
            }
            finally
            {
                _semaphoreSlim.Release();
            }

            int count = 0;
            var tmp = Stopwatch.StartNew();
            var individualWatch = new Stopwatch();
            foreach (long characterId in characterIds)
            {
                CharacterDTO toSave = _characterById.Get(characterId);
                if (toSave == null)
                {
                    Log.Error($"[CHARACTER_SAVE_SYSTEM] {characterId.ToString()} could not be retrieved from cache", new DataException($"Desynchronised data for character {characterId.ToString()}"));
                    continue;
                }

                individualWatch.Restart();
                try
                {
                    CharacterDTO savedCharacter = await _characterDao.SaveAsync(toSave);
                    count++;
                    Log.Warn($"[CHARACTER_SAVE_SYSTEM] Saved a character successfully in {individualWatch.ElapsedMilliseconds.ToString()}ms. " +
                        $"CharacterName: '{savedCharacter.Name}' CharacterId: '{savedCharacter.Id.ToString()}' AccountId: '{savedCharacter.AccountId.ToString()}'");
                }
                catch (Exception e)
                {
                    unsavedCharacterIds.Add(characterId);
                    Log.Error($"[CHARACTER_SAVE_SYSTEM] Failed to save a character in {individualWatch.ElapsedMilliseconds.ToString()}ms. Re-queueing the save. " +
                        $"CharacterName: '{toSave.Name}' CharacterId: '{toSave.Id.ToString()}' AccountId: '{toSave.AccountId.ToString()}'", e);
                }

                individualWatch.Stop();
            }

            tmp.Stop();
            Log.Debug($"[CHARACTER_SAVE_SYSTEM] Saving of saves took in total {tmp.ElapsedMilliseconds.ToString()}ms");

            if (unsavedCharacterIds.Count <= 0)
            {
                return count;
            }

            await _semaphoreSlim.WaitAsync();
            try
            {
                foreach (long characterId in unsavedCharacterIds)
                {
                    _characterIdsToSave.Add(characterId);
                }
            }
            finally
            {
                _semaphoreSlim.Release();
            }

            Log.Warn($"[CHARACTER_SAVE_SYSTEM] Re-queued {unsavedCharacterIds.Count.ToString()} character saves");
            return count;
        }

        public async Task<CharacterDTO> RemoveCachedCharacter(string requestCharacterName)
        {
            CharacterDTO characterDto = await GetCharacterByName(requestCharacterName);
            if (characterDto == null)
            {
                return null;
            }

            await _characterDao.SaveAsync(characterDto);
            _characterById.Remove(characterDto.Id);
            return characterDto;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                await FlushCharacterSaves();
                await Task.Delay(Interval, stoppingToken);
            }
        }

        private static string GetKey(string name) => $"char:name:{name}";

        private static string GetKeyAccountIdSlot(long accountId, byte slot) => $"char:account-id:{accountId.ToString()}:slot:{slot.ToString()}";

        private void SetCharacter(CharacterDTO characterDto)
        {
            _characterById.Set(characterDto.Id, characterDto, LifeTime);
            _characterIdByKey.Set(GetKey(characterDto.Name), characterDto.Id, LifeTime);
            _characterIdByKey.Set(GetKeyAccountIdSlot(characterDto.AccountId, characterDto.Slot), characterDto.Id, LifeTime);
        }

        private void RemoveCharacter(CharacterDTO characterDto)
        {
            _characterById.Remove(characterDto.Id);
            _characterIdByKey.Remove(GetKey(characterDto.Name));
            _characterIdByKey.Remove(GetKeyAccountIdSlot(characterDto.AccountId, characterDto.Slot));
            _characterIdsToSave.Remove(characterDto.Id);
        }
    }
}