using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Foundatio.AsyncEx;
using PhoenixLib.Caching;
using PhoenixLib.Logging;
using PhoenixLib.ServiceBus;
using WingsEmu.Core.Extensions;
using WingsEmu.DTOs.Mails;
using WingsEmu.Plugins.DistributedGameEvents.Mails;

namespace MailServer.Managers
{
    public class MailManager
    {
        private static readonly TimeSpan LifeTime = TimeSpan.FromMinutes(Convert.ToUInt32(Environment.GetEnvironmentVariable("MAIL_SERVER_CACHE_TTL_MINUTES") ?? "1"));
        private readonly ILongKeyCachedRepository<AsyncReaderWriterLock> _characterIdLocks;
        private readonly ICharacterMailDao _characterMailDao;

        private readonly ILongKeyCachedRepository<Dictionary<long, CharacterMailDto>> _mailsByCharacterId;
        private readonly IMessagePublisher<MailReceivedMessage> _messagePublisher;

        private readonly ConcurrentQueue<(CharacterMailDto dto, bool remove)> _queue = new();

        public MailManager(ICharacterMailDao characterMailDao, IMessagePublisher<MailReceivedMessage> messagePublisher, ILongKeyCachedRepository<Dictionary<long, CharacterMailDto>> mailsByCharacterId,
            ILongKeyCachedRepository<AsyncReaderWriterLock> characterIdLocks)
        {
            _characterMailDao = characterMailDao;
            _messagePublisher = messagePublisher;
            _mailsByCharacterId = mailsByCharacterId;
            _characterIdLocks = characterIdLocks;
        }

        private AsyncReaderWriterLock GetCharacterLock(long characterId)
        {
            AsyncReaderWriterLock characterLock = _characterIdLocks.Get(characterId);
            if (characterLock == null)
            {
                return _characterIdLocks.GetOrSet(characterId, () => new AsyncReaderWriterLock(), LifeTime);
            }

            _characterIdLocks.Set(characterId, characterLock, LifeTime);
            return characterLock;
        }

        private async Task<Dictionary<long, CharacterMailDto>> GetCharacterMailsWithoutLock(long characterId)
        {
            Dictionary<long, CharacterMailDto> mails = _mailsByCharacterId.Get(characterId) ?? (await _characterMailDao.GetByCharacterIdAsync(characterId)).ToDictionary(m => m.Id);

            _mailsByCharacterId.Set(characterId, mails, LifeTime);
            return mails;
        }

        public async Task<IEnumerable<CharacterMailDto>> GetCharacterMails(long characterId)
        {
            AsyncReaderWriterLock characterLock = GetCharacterLock(characterId);
            using (await characterLock.ReaderLockAsync())
            {
                return (await GetCharacterMailsWithoutLock(characterId))?.Values;
            }
        }

        public async Task<IReadOnlyDictionary<long, CharacterMailDto>> GetCharacterMailsDictionary(long characterId)
        {
            AsyncReaderWriterLock characterLock = GetCharacterLock(characterId);
            using (await characterLock.ReaderLockAsync())
            {
                return await GetCharacterMailsWithoutLock(characterId);
            }
        }

        private async Task AddCharacterMails(long characterId, IEnumerable<CharacterMailDto> dtos)
        {
            AsyncReaderWriterLock characterLock = GetCharacterLock(characterId);
            using (await characterLock.WriterLockAsync())
            {
                Dictionary<long, CharacterMailDto> characterMails = await GetCharacterMailsWithoutLock(characterId);
                foreach (CharacterMailDto dto in dtos)
                {
                    characterMails[dto.Id] = dto;
                }
            }

            await _messagePublisher.PublishAsync(new MailReceivedMessage
            {
                CharacterId = characterId,
                MailDtos = dtos
            });
        }

        private async Task RemoveCharacterMails(long characterId, IEnumerable<CharacterMailDto> dtos)
        {
            AsyncReaderWriterLock characterLock = GetCharacterLock(characterId);
            using (await characterLock.WriterLockAsync())
            {
                Dictionary<long, CharacterMailDto> characterMails = await GetCharacterMailsWithoutLock(characterId);
                foreach (CharacterMailDto dto in dtos)
                {
                    characterMails.Remove(dto.Id);
                }
            }
        }

        private async Task RemoveCharacterMail(long characterId, CharacterMailDto dto)
        {
            AsyncReaderWriterLock characterLock = GetCharacterLock(characterId);
            using (await characterLock.WriterLockAsync())
            {
                Dictionary<long, CharacterMailDto> characterMails = await GetCharacterMailsWithoutLock(characterId);
                characterMails.Remove(dto.Id);
            }
        }

        public async Task RemoveMails(long characterId, IEnumerable<CharacterMailDto> dtos)
        {
            await RemoveCharacterMails(characterId, dtos);
            foreach (CharacterMailDto dto in dtos)
            {
                _queue.Enqueue((dto, true));
            }
        }

        public async Task RemoveMail(long characterId, CharacterMailDto dto)
        {
            await RemoveCharacterMail(characterId, dto);
            _queue.Enqueue((dto, true));
        }

        public void AddMail((CharacterMailDto dto, bool remove) valueTuple)
        {
            _queue.Enqueue(valueTuple);
        }

        public void AddMails(IEnumerable<(CharacterMailDto dto, bool remove)> mails)
        {
            foreach ((CharacterMailDto dto, bool remove) mail in mails)
            {
                _queue.Enqueue(mail);
            }
        }

        public async Task AddMailsInstantly(IEnumerable<CharacterMailDto> dtosToSave)
        {
            Dictionary<long, List<CharacterMailDto>> dtosToAddByCharacterId = new();

            foreach (CharacterMailDto dto in dtosToSave)
            {
                dtosToAddByCharacterId.GetOrSetDefault(dto.ReceiverId, new List<CharacterMailDto>()).Add(dto);
            }

            List<(CharacterMailDto dto, bool remove)> dtosFailedToSave = new();

            foreach ((long characterId, List<CharacterMailDto> dtosToAdd) in dtosToAddByCharacterId)
            {
                try
                {
                    IEnumerable<CharacterMailDto> saved = await _characterMailDao.SaveAsync(dtosToAdd);

                    await AddCharacterMails(characterId, saved);
                    Log.Warn($"[MAIL_MANAGER][BATCH_SAVING][CREATE][CharacterId: '{characterId.ToString()}'] Successfully created {dtosToAdd.Count.ToString()} mails");
                }
                catch (Exception e)
                {
                    dtosFailedToSave.AddRange(dtosToAdd.Select(s => (s, false)));
                    Log.Error($"[MAIL_MANAGER][BATCH_SAVING][CREATE][CharacterId: '{characterId.ToString()}'] Failed to create batch of created Mails", e);
                }
            }

            if (dtosFailedToSave.Count < 1)
            {
                return;
            }

            foreach ((CharacterMailDto dto, bool remove) dto in dtosFailedToSave)
            {
                _queue.Enqueue(dto);
            }
        }

        public async Task FlushAll()
        {
            Dictionary<long, List<CharacterMailDto>> dtosToAddByCharacterId = new();
            Dictionary<long, List<CharacterMailDto>> dtosToRemoveByCharacterId = new();

            while (_queue.TryDequeue(out (CharacterMailDto dto, bool remove) tuple))
            {
                Dictionary<long, List<CharacterMailDto>> dictionaryToModify = tuple.remove ? dtosToRemoveByCharacterId : dtosToAddByCharacterId;
                dictionaryToModify.GetOrSetDefault(tuple.dto.ReceiverId, new List<CharacterMailDto>()).Add(tuple.dto);
            }

            List<(CharacterMailDto dto, bool remove)> dtosFailedToSave = new();

            foreach ((long characterId, List<CharacterMailDto> dtosToAdd) in dtosToAddByCharacterId)
            {
                try
                {
                    IEnumerable<CharacterMailDto> saved = await _characterMailDao.SaveAsync(dtosToAdd);

                    await AddCharacterMails(characterId, saved);
                    Log.Warn($"[MAIL_MANAGER][BATCH_SAVING][CREATE][CharacterId: '{characterId.ToString()}'] Successfully created {dtosToAdd.Count.ToString()} mails");
                }
                catch (Exception e)
                {
                    dtosFailedToSave.AddRange(dtosToAdd.Select(s => (s, false)));
                    Log.Error($"[MAIL_MANAGER][BATCH_SAVING][CREATE][CharacterId: '{characterId.ToString()}'] Failed to create batch of created Mails", e);
                }
            }

            foreach ((long characterId, List<CharacterMailDto> dtosToRemove) in dtosToRemoveByCharacterId)
            {
                try
                {
                    var toRemoveIds = dtosToRemove.Select(s => s.Id).ToList();
                    await _characterMailDao.DeleteByIdsAsync(toRemoveIds);

                    Log.Warn($"[MAIL_MANAGER][BATCH_SAVING][REMOVE][CharacterId: '{characterId.ToString()}'] Successfully removed {toRemoveIds.Count.ToString()} mails");
                }
                catch (Exception e)
                {
                    dtosFailedToSave.AddRange(dtosToRemove.Select(s => (s, true)));
                    Log.Error($"[MAIL_MANAGER][BATCH_SAVING][REMOVE][CharacterId: '{characterId.ToString()}'] Failed to create batch of removed Mails", e);
                }
            }

            if (dtosFailedToSave.Count < 1)
            {
                return;
            }

            foreach ((CharacterMailDto dto, bool remove) dto in dtosFailedToSave)
            {
                _queue.Enqueue(dto);
            }
        }
    }
}