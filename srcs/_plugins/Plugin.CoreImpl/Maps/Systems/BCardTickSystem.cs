// NosEmu
// 


using System;
using System.Collections.Generic;
using System.Linq;
using WingsEmu.DTOs.BCards;
using WingsEmu.Game;
using WingsEmu.Game._enum;
using WingsEmu.Game._i18n;
using WingsEmu.Game.Buffs;
using WingsEmu.Game.Buffs.Events;
using WingsEmu.Game.Characters;
using WingsEmu.Game.Entities;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Extensions.Mates;
using WingsEmu.Game.Mates;
using WingsEmu.Packets.Enums.Chat;

namespace Plugin.CoreImpl.Maps.Systems
{
    public sealed class BCardTickSystem
    {
        private readonly IBCardEffectHandlerContainer _bCardHandlers;
        private readonly IBuffFactory _buffFactory;
        private readonly IGameLanguageService _gameLanguage;

        private readonly Dictionary<(Guid bCardId, long entityId), DateTime> _lastBCardTick = new();
        private readonly IRandomGenerator _randomGenerator;
        private readonly Queue<Buff> _toAdd;
        private readonly Queue<Buff> _toRemove;

        public BCardTickSystem(IBCardEffectHandlerContainer bCardHandlers, IRandomGenerator randomGenerator, IBuffFactory buffFactory, IGameLanguageService gameLanguage)
        {
            _bCardHandlers = bCardHandlers;
            _randomGenerator = randomGenerator;
            _buffFactory = buffFactory;
            _gameLanguage = gameLanguage;
            _toRemove = new Queue<Buff>();
            _toAdd = new Queue<Buff>();
        }

        public void ProcessUpdate(IBattleEntity battleEntity, in DateTime time)
        {
            ProcessRecurrentBCards(battleEntity, time);
        }

        public void Clear()
        {
            _lastBCardTick.Clear();
        }

        private void ProcessRecurrentBCards(IBattleEntity entity, in DateTime time)
        {
            if (!entity.BuffComponent.HasAnyBuff())
            {
                return;
            }

            IReadOnlyList<Buff> buffs = entity.BuffComponent.GetAllBuffs();
            foreach (Buff buff in buffs)
            {
                if (ProcessExpiringBuff(buff, entity, time))
                {
                    continue;
                }

                foreach (BCardDTO bCard in buff.BCards)
                {
                    if (!bCard.TickPeriod.HasValue)
                    {
                        continue;
                    }

                    (Guid, int) key = (bCard.Id, entity.Id);

                    if (!_lastBCardTick.TryGetValue(key, out DateTime dateTime))
                    {
                        _lastBCardTick[key] = time;
                    }

                    if (dateTime.AddSeconds(bCard.TickPeriod.Value) >= time)
                    {
                        continue;
                    }

                    _lastBCardTick[key] = time;

                    if (bCard.ProcChance != 0 && bCard.ProcChance < _randomGenerator.RandomNumber())
                    {
                        continue;
                    }

                    _bCardHandlers.Execute(entity, buff.Caster, bCard);
                }
            }

            while (_toRemove.TryDequeue(out Buff buff))
            {
                entity.EmitEvent(new BuffRemoveEvent
                {
                    Buffs = new[] { buff },
                    Entity = entity,
                    RemovePermanentBuff = buff.IsSavingOnDisconnect()
                });
            }

            while (_toAdd.TryDequeue(out Buff buff))
            {
                entity.EmitEvent(new BuffAddEvent(entity, new[] { buff }));
            }
        }
        
        private void ExecuteBCard(BCardDTO bCard, IBattleEntity entity, in DateTime time)
        {
            (Guid, int) key = (bCard.Id, entity.Id);
            if (!_lastBCardTick.TryGetValue(key, out DateTime dateTime))
            {
                _lastBCardTick[key] = time;
            }

            if (dateTime.AddMilliseconds(1500) > time)
            {
                return;
            }

            _lastBCardTick[key] = time;
            _bCardHandlers.Execute(entity, entity, bCard);
        }

        private void CheckEverySecBCard(Buff buff, BCardDTO bCard, IBattleEntity entity, in DateTime time)
        {
            byte firstData = (byte)bCard.FirstDataValue(buff.CasterLevel);
            bool isFirst = false;

            (Guid, int) key = (bCard.Id, entity.Id);

            if (!_lastBCardTick.TryGetValue(key, out DateTime dateTime))
            {
                _lastBCardTick[key] = time;
                isFirst = true;
            }

            if (dateTime.AddSeconds(firstData) >= time)
            {
                return;
            }

            _lastBCardTick[key] = time;

            switch (entity)
            {
                case IPlayerEntity playerEntity:
                    playerEntity.Session.RefreshStatChar();
                    playerEntity.Session.RefreshStat();
                    playerEntity.Session.SendCondPacket();
                    break;
                case IMonsterEntity monsterEntity:
                    monsterEntity.RefreshStats();
                    break;
                case IMateEntity mateEntity:
                    mateEntity.Owner?.Session.SendPetInfo(mateEntity, _gameLanguage);
                    mateEntity.Owner?.Session.SendCondMate(mateEntity);
                    break;
            }
        }

        private bool ProcessExpiringBuff(Buff buff, IBattleEntity battleEntity, in DateTime time)
        {
            if (buff.SecondBCardsDelay != 0 && !buff.SecondBCardsExecuted)
            {
                bool execute = buff.Start.AddMilliseconds(buff.SecondBCardsDelay * 100) <= time;
                if (execute)
                {
                    var bCards = buff.BCards.Where(x => x.IsSecondBCardExecution != null && x.IsSecondBCardExecution.Value).ToList();
                    battleEntity.BCardComponent.AddBuffBCards(buff, bCards);
                    buff.SecondBCardsExecuted = true;

                    foreach (BCardDTO bCard in bCards)
                    {
                        _bCardHandlers.Execute(battleEntity, buff.Caster, bCard);
                    }

                    battleEntity.ShadowAppears(false, buff);
                    switch (battleEntity)
                    {
                        case IPlayerEntity character:
                            {
                                character.Session.RefreshStat();
                                character.Session.RefreshStatChar();
                                character.Session.SendCondPacket();
                                string buffName = _gameLanguage.GetLanguage(GameDataType.Card, buff.Name, character.Session.UserLanguage);
                                character.Session.SendChatMessage(character.Session.GetLanguageFormat(GameDialogKey.BUFF_CHATMESSAGE_SIDE_EFFECTS, buffName), ChatMessageColorType.Buff);
                                break;
                            }
                        case IMonsterEntity monsterEntity:
                            monsterEntity.RefreshStats();
                            break;
                        case IMateEntity mateEntity:
                            mateEntity.Owner?.Session.SendPetInfo(mateEntity, _gameLanguage);
                            mateEntity.Owner?.Session.SendCondMate(mateEntity);
                            break;
                    }
                }
            }

            if (buff.Start.AddMilliseconds(buff.Duration.TotalMilliseconds) > time)
            {
                return false;
            }

            if (buff.Duration.TotalMilliseconds < 0 || buff.CardId == (short)BuffVnums.CHARGE)
            {
                return false;
            }

            foreach (BCardDTO bCard in buff.BCards)
            {
                _lastBCardTick.Remove((bCard.Id, battleEntity.Id));
            }

            if (buff.TimeoutBuff != 0 && _randomGenerator.RandomNumber() < buff.TimeoutBuffChance)
            {
                _toRemove.Enqueue(buff);
                Buff timeoutBuff = _buffFactory.CreateBuff(buff.TimeoutBuff, buff.Caster);
                _toAdd.Enqueue(timeoutBuff);
                return true;
            }

            _toRemove.Enqueue(buff);
            return true;
        }
    }
}