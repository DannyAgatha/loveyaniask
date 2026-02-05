using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using WingsEmu.Core.Extensions;
using WingsEmu.Core.Generics;
using WingsEmu.DTOs.BCards;
using WingsEmu.Packets.Enums;

namespace WingsEmu.Game.Buffs;

public class BCardComponent : IBCardComponent
{
    private readonly List<BCardDTO> _allBCards = new();
    private readonly ConcurrentDictionary<(BCardType, byte), ThreadSafeHashSet<BCardDTO>> _bCards = new();
    private readonly ConcurrentDictionary<EquipmentType, List<BCardDTO>> _bCardsByEquipmentType = new();

    private readonly ConcurrentDictionary<Guid, HashSet<BCardDTO>> _buffBCards = new();
    private readonly List<(int casterLevel, BCardDTO bCard)> _buffBCardsCasterLevel = new();
    private readonly ConcurrentDictionary<(BCardType, byte), Dictionary<Guid, (int casterLevel, BCardDTO bCard)>> _buffInformation = new();

    private readonly ThreadSafeHashSet<BCardDTO> _chargeBCards = new();

    private readonly ConcurrentDictionary<(BCardType, byte), byte> _hasBCardCache = new();

    private readonly ReaderWriterLockSlim _lock = new(LockRecursionPolicy.SupportsRecursion);

    private readonly List<BCardDTO> _onAttack = new();
    private readonly List<BCardDTO> _onDefense = new();
    private readonly IRandomGenerator _randomGenerator;

    private readonly ConcurrentDictionary<bool, List<BCardDTO>> _shellTriggerOnAttack = new();

    public BCardComponent(IRandomGenerator randomGenerator) => _randomGenerator = randomGenerator;

    public void AddEquipmentBCards(EquipmentType equipmentType, IEnumerable<BCardDTO> bCards)
    {
        _lock.EnterWriteLock();
        try
        {
            List<BCardDTO> existingBCards = _bCardsByEquipmentType.GetOrDefault(equipmentType);
            if (existingBCards == null)
            {
                existingBCards = new List<BCardDTO>();
                _bCardsByEquipmentType[equipmentType] = existingBCards;
            }

            foreach (BCardDTO bCard in bCards)
            {
                existingBCards.Add(bCard);
                AddBCard(bCard);
            }
        }
        finally
        {
            _lock.ExitWriteLock();
        }
    }

    public void ClearEquipmentBCards(EquipmentType equipmentType)
    {
        _lock.EnterWriteLock();
        try
        {
            _bCardsByEquipmentType.TryRemove(equipmentType, out List<BCardDTO> bCards);
            if (bCards == null)
            {
                return;
            }

            foreach (BCardDTO bCard in bCards)
            {
                RemoveBCard(bCard);
            }
        }
        finally
        {
            _lock.ExitWriteLock();
        }
    }

    public IReadOnlyList<BCardDTO> GetEquipmentBCards(EquipmentType equipmentType)
    {
        List<BCardDTO> eqBCards = _bCardsByEquipmentType.GetOrDefault(equipmentType);
        if (eqBCards == null)
        {
            return Array.Empty<BCardDTO>();
        }

        return eqBCards;
    }

    public IReadOnlyDictionary<EquipmentType, List<BCardDTO>> GetEquipmentBCards() => _bCardsByEquipmentType;

    public (int firstData, int secondData) GetAllBCardsInformation(BCardType type, byte subType, int level)
    {
        int firstData = 0;
        int secondData = 0;
        double firstDataMultiply = 1;
        double secondDataMultiply = 1;

        if (_bCards.TryGetValue((type, subType), out ThreadSafeHashSet<BCardDTO> list))
        {
            foreach (BCardDTO bCard in list)
            {
                int firstDataValue = bCard.FirstDataValue(level);
                int secondDataValue = bCard.SecondDataValue(level);

                switch (type)
                {
                    case BCardType.Morale:
                        switch (subType)
                        {
                            case (byte)AdditionalTypes.Morale.SkillCooldownIncreased:
                                firstDataMultiply *= 1 + 0.01 * firstDataValue;
                                secondDataMultiply *= 1 + 0.01 * secondDataValue;
                                break;
                            case (byte)AdditionalTypes.Morale.SkillCooldownDecreased:
                                firstDataMultiply *= 0.01 * (100 - firstDataValue);
                                secondDataMultiply *= 0.01 * (100 - secondDataValue);
                                break;
                        }

                        continue;
                    case BCardType.AbsorbedSpirit:
                        switch (subType)
                        {
                            case (byte)AdditionalTypes.AbsorbedSpirit.MagicCooldownIncreased:
                                firstDataMultiply *= 1 + 0.01 * firstDataValue;
                                secondDataMultiply *= 1 + 0.01 * secondDataValue;
                                break;
                            case (byte)AdditionalTypes.AbsorbedSpirit.MagicCooldownDecreased:
                                firstDataMultiply *= 0.01 * (100 - firstDataValue);
                                secondDataMultiply *= 0.01 * (100 - secondDataValue);
                                break;
                        }
                        
                        continue;
                    default:
                        firstData += firstDataValue;
                        secondData += secondDataValue;
                        break;
                }
            }
        }

        if (_buffInformation.TryGetValue((type, subType), out Dictionary<Guid, (int casterLevel, BCardDTO bCard)> buff))
        {
            foreach ((int casterLevel, BCardDTO bCard) in buff.Values)
            {
                int firstDataValue = bCard.FirstDataValue(casterLevel);
                int secondDataValue = bCard.SecondDataValue(casterLevel);

                switch (type)
                {
                    case BCardType.Morale:
                        switch (subType)
                        {
                            case (byte)AdditionalTypes.Morale.SkillCooldownIncreased:
                                firstDataMultiply *= 1 + 0.01 * firstDataValue;
                                secondDataMultiply *= 1 + 0.01 * secondDataValue;
                                break;
                            case (byte)AdditionalTypes.Morale.SkillCooldownDecreased:
                                firstDataMultiply *= 0.01 * (100 - firstDataValue);
                                secondDataMultiply *= 0.01 * (100 - secondDataValue);
                                break;
                        }

                        continue;
                    case BCardType.AbsorbedSpirit:
                        switch (subType)
                        {
                            case (byte)AdditionalTypes.AbsorbedSpirit.MagicCooldownIncreased:
                                firstDataMultiply *= 1 + 0.01 * firstDataValue;
                                secondDataMultiply *= 1 + 0.01 * secondDataValue;
                                break;
                            case (byte)AdditionalTypes.AbsorbedSpirit.MagicCooldownDecreased:
                                firstDataMultiply *= 0.01 * (100 - firstDataValue);
                                secondDataMultiply *= 0.01 * (100 - secondDataValue);
                                break;
                        }
                        
                        continue;
                    default:
                        firstData += firstDataValue;
                        secondData += secondDataValue;
                        break;
                }
            }
        }

        if ((type == BCardType.Morale && subType is (byte)AdditionalTypes.Morale.SkillCooldownIncreased or (byte)AdditionalTypes.Morale.SkillCooldownDecreased) 
            || (type == BCardType.AbsorbedSpirit && subType is (byte)AdditionalTypes.AbsorbedSpirit.MagicCooldownIncreased or (byte)AdditionalTypes.AbsorbedSpirit.MagicCooldownDecreased))
        {
            return ((int)(firstDataMultiply * 100), (int)(secondDataMultiply * 100));
        }

        return (firstData, secondData);
    }

    public IReadOnlyList<BCardDTO> GetAllBCards()
    {
        _lock.EnterReadLock();
        try
        {
            return _allBCards.ToArray();
        }
        finally
        {
            _lock.ExitReadLock();
        }
    }

    public IReadOnlyList<(int casterLevel, BCardDTO bCard)> GetBuffBCards(Func<(int, BCardDTO), bool> predicate = null)
    {
        _lock.EnterReadLock();
        try
        {
            return _buffBCardsCasterLevel.FindAll(x => predicate == null || predicate(x));
        }
        finally
        {
            _lock.ExitReadLock();
        }
    }

    public bool HasBCard(BCardType bCardType, byte subType) => _hasBCardCache.TryGetValue((bCardType, subType), out byte amount) && amount > 0;

    public bool HasEquipmentsBCard(BCardType bCardType, byte subType)
    {
        return _bCardsByEquipmentType.Values.Any(x => x.Any(s => s.Type == (short)bCardType && s.SubType == subType));
    }

    public void AddBCard(BCardDTO bCard)
    {
        (BCardType, byte) key = ((BCardType)bCard.Type, bCard.SubType);
        ThreadSafeHashSet<BCardDTO> existingBCards = _bCards.GetOrAdd(key, new ThreadSafeHashSet<BCardDTO>());
        existingBCards.Add(bCard);

        if (!_hasBCardCache.TryGetValue(key, out _))
        {
            _hasBCardCache.TryAdd(key, 1);
        }
        else
        {
            _hasBCardCache[key]++;
        }
        
        if (bCard.IsOnAttack())
        {
            _onAttack.Add(bCard);
        }

        if (bCard.IsOnDefense())
        {
            _onDefense.Add(bCard);
        }

        _allBCards.Add(bCard);
    }

    public void RemoveBCard(BCardDTO bCard)
    {
        (BCardType, byte) key = ((BCardType)bCard.Type, bCard.SubType);
        ThreadSafeHashSet<BCardDTO> existingBCards = _bCards.GetOrDefault(key);

        existingBCards?.Remove(bCard);
        if (_hasBCardCache.TryGetValue(key, out byte amount))
        {
            amount -= 1;
            if (amount > 0)
            {
                _hasBCardCache[key] = amount;
            }
            else
            {
                _hasBCardCache.TryRemove(key, out _);
            }
        }
        
        if (bCard.IsOnAttack())
        {
            _onAttack.Remove(bCard);
        }

        if (bCard.IsOnDefense())
        {
            _onDefense.Remove(bCard);
        }

        _allBCards.Remove(bCard);
    }

    public void AddBuffBCards(Buff buff, IEnumerable<BCardDTO> bCards = null)
    {
        _lock.EnterWriteLock();
        try
        {
            if (_buffBCards.ContainsKey(buff.BuffId) && bCards == null)
            {
                return;
            }

            if (bCards != null) // Second BCards execution
            {
                foreach (BCardDTO bCard in bCards)
                {
                    (BCardType, byte SubType) key = ((BCardType)bCard.Type, bCard.SubType);
                    if (bCard.ProcChance != 0)
                    {
                        int randomNumber = _randomGenerator.RandomNumber();
                        if (randomNumber > bCard.ProcChance)
                        {
                            continue;
                        }
                    }

                    if (!_buffBCards.TryGetValue(buff.BuffId, out HashSet<BCardDTO> hashSet))
                    {
                        hashSet = new HashSet<BCardDTO>();
                        _buffBCards[buff.BuffId] = hashSet;
                    }

                    hashSet.Add(bCard);
                    _buffBCardsCasterLevel.Add((buff.CasterLevel, bCard));
                    if (!_hasBCardCache.TryGetValue(key, out _))
                    {
                        _hasBCardCache.TryAdd(key, 1);
                    }
                    else
                    {
                        _hasBCardCache[key]++;
                    }
                    
                    if (bCard.IsOnAttack())
                    {
                        _onAttack.Add(bCard);
                    }

                    if (bCard.IsOnDefense())
                    {
                        _onDefense.Add(bCard);
                    }

                    if (!_buffInformation.TryGetValue(key, out Dictionary<Guid, (int casterLevel, BCardDTO bCard)> newDictionary))
                    {
                        newDictionary = new Dictionary<Guid, (int casterLevel, BCardDTO bCard)>();
                        _buffInformation[((BCardType)bCard.Type, bCard.SubType)] = newDictionary;
                    }

                    newDictionary[bCard.Id] = (buff.CasterLevel, bCard);
                }
            }
            else
            {
                foreach (BCardDTO bCard in buff.BCards.Where(x => !x.IsSecondBCardExecution.HasValue || !x.IsSecondBCardExecution.Value))
                {
                    (BCardType, byte SubType) key = ((BCardType)bCard.Type, bCard.SubType);
                    if (bCard.ProcChance != 0)
                    {
                        int randomNumber = _randomGenerator.RandomNumber();
                        if (randomNumber > bCard.ProcChance)
                        {
                            continue;
                        }
                    }

                    if (!_buffBCards.TryGetValue(buff.BuffId, out HashSet<BCardDTO> hashSet))
                    {
                        hashSet = new HashSet<BCardDTO>();
                        _buffBCards[buff.BuffId] = hashSet;
                    }

                    hashSet.Add(bCard);
                    _buffBCardsCasterLevel.Add((buff.CasterLevel, bCard));
                    if (!_hasBCardCache.TryGetValue(key, out _))
                    {
                        _hasBCardCache.TryAdd(key, 1);
                    }
                    else
                    {
                        _hasBCardCache[key]++;
                    }
                    
                    if (bCard.IsOnAttack())
                    {
                        _onAttack.Add(bCard);
                    }

                    if (bCard.IsOnDefense())
                    {
                        _onDefense.Add(bCard);
                    }

                    if (!_buffInformation.TryGetValue(key, out Dictionary<Guid, (int casterLevel, BCardDTO bCard)> newDictionary))
                    {
                        newDictionary = new Dictionary<Guid, (int casterLevel, BCardDTO bCard)>();
                        _buffInformation[((BCardType)bCard.Type, bCard.SubType)] = newDictionary;
                    }

                    newDictionary[bCard.Id] = (buff.CasterLevel, bCard);
                }
            }
        }
        finally
        {
            _lock.ExitWriteLock();
        }
    }

    public void RemoveBuffBCards(Buff buff)
    {
        _lock.EnterWriteLock();
        try
        {
            if (!_buffBCards.TryRemove(buff.BuffId, out HashSet<BCardDTO> bCards))
            {
                return;
            }

            foreach (BCardDTO bCard in bCards)
            {
                (BCardType, byte SubType) key = ((BCardType)bCard.Type, bCard.SubType);
                if (!_buffInformation.TryGetValue(key, out Dictionary<Guid, (int casterLevel, BCardDTO bCard)> toRemove))
                {
                    _hasBCardCache.TryRemove(key, out _);
                    continue;
                }
                
                if (bCard.IsOnAttack())
                {
                    _onAttack.Remove(bCard);
                }

                if (bCard.IsOnDefense())
                {
                    _onDefense.Remove(bCard);
                }

                toRemove.Remove(bCard.Id);
                _buffBCardsCasterLevel.Remove((buff.CasterLevel, bCard));
                if (_hasBCardCache.TryGetValue(key, out byte amount))
                {
                    amount -= 1;
                    if (amount > 0)
                    {
                        _hasBCardCache[key] = amount;
                        continue;
                    }

                    _hasBCardCache.TryRemove(key, out _);
                    _buffInformation.TryRemove(key, out _);
                    continue;
                }

                _hasBCardCache.TryRemove(key, out _);
                _buffInformation.TryRemove(key, out _);
            }
        }
        finally
        {
            _lock.ExitWriteLock();
        }
    }

    public void AddTriggerBCards(BCardTriggerType triggerType, List<BCardDTO> bCards)
    {
        _lock.EnterWriteLock();
        try
        {
            if (bCards == null)
            {
                return;
            }

            if (!bCards.Any())
            {
                return;
            }

            switch (triggerType)
            {
                case BCardTriggerType.ATTACK:
                    _onAttack.AddRange(bCards);
                    break;
                case BCardTriggerType.DEFENSE:
                    _onDefense.AddRange(bCards);
                    break;
            }
        }
        finally
        {
            _lock.ExitWriteLock();
        }
    }

    public void RemoveAllTriggerBCards(BCardTriggerType triggerType)
    {
        _lock.EnterWriteLock();
        try
        {
            if (triggerType == BCardTriggerType.ATTACK)
            {
                _onAttack.Clear();
                return;
            }

            _onDefense.Clear();
        }
        finally
        {
            _lock.ExitWriteLock();
        }
    }

    public IReadOnlyList<BCardDTO> GetTriggerBCards(BCardTriggerType triggerType)
        => triggerType == BCardTriggerType.ATTACK ? _onAttack : _onDefense;
    
    public void AddShellTrigger(bool isMainWeapon, List<BCardDTO> bCards)
    {
        if (!_shellTriggerOnAttack.TryGetValue(isMainWeapon, out List<BCardDTO> list))
        {
            list = new List<BCardDTO>();
            _shellTriggerOnAttack[isMainWeapon] = list;
        }

        list.AddRange(bCards);
    }

    public void ClearShellTrigger(bool isMainWeapon)
    {
        if (!_shellTriggerOnAttack.TryGetValue(isMainWeapon, out List<BCardDTO> list))
        {
            return;
        }

        list.Clear();
    }

    public IReadOnlyList<BCardDTO> GetShellTriggers(bool isMainWeapon) => !_shellTriggerOnAttack.TryGetValue(isMainWeapon, out List<BCardDTO> list) ? Array.Empty<BCardDTO>() : list;

    public void AddChargeBCard(BCardDTO bCard)
    {
        if (_chargeBCards.Contains(bCard))
        {
            return;
        }

        _chargeBCards.Add(bCard);
    }

    public void RemoveChargeBCard(BCardDTO bCard)
    {
        _chargeBCards.Remove(bCard);
    }

    public void ClearChargeBCard()
    {
        _chargeBCards.Clear();
    }

    public IEnumerable<BCardDTO> GetChargeBCards() => _chargeBCards;
}