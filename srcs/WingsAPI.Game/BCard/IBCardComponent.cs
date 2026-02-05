using System;
using System.Collections.Generic;
using WingsEmu.DTOs.BCards;
using WingsEmu.Packets.Enums;

namespace WingsEmu.Game.Buffs;

public interface IBCardComponent
{
    public void AddEquipmentBCards(EquipmentType equipmentType, IEnumerable<BCardDTO> bCards);
    public void ClearEquipmentBCards(EquipmentType equipmentType);

    public IReadOnlyList<BCardDTO> GetEquipmentBCards(EquipmentType equipmentType);
    public IReadOnlyDictionary<EquipmentType, List<BCardDTO>> GetEquipmentBCards();

    public (int firstData, int secondData) GetAllBCardsInformation(BCardType type, byte subType, int level);

    public IReadOnlyList<BCardDTO> GetAllBCards();
    public IReadOnlyList<(int casterLevel, BCardDTO bCard)> GetBuffBCards(Func<(int, BCardDTO), bool> predicate = null);

    public bool HasBCard(BCardType bCardType, byte subType);
    public bool HasEquipmentsBCard(BCardType bCardType, byte subType);

    public void AddBCard(BCardDTO bCard);
    public void RemoveBCard(BCardDTO bCard);

    public void AddBuffBCards(Buff buff, IEnumerable<BCardDTO> bCards = null);
    public void RemoveBuffBCards(Buff buff);

    public void AddTriggerBCards(BCardTriggerType triggerType, List<BCardDTO> bCards);
    public void RemoveAllTriggerBCards(BCardTriggerType triggerType);

    public IReadOnlyList<BCardDTO> GetTriggerBCards(BCardTriggerType triggerType);

    public void AddShellTrigger(bool isMainWeapon, List<BCardDTO> bCards);
    public void ClearShellTrigger(bool isMainWeapon);
    public IReadOnlyList<BCardDTO> GetShellTriggers(bool isMainWeapon);

    public void AddChargeBCard(BCardDTO bCard);
    public void RemoveChargeBCard(BCardDTO bCard);
    public void ClearChargeBCard();
    public IEnumerable<BCardDTO> GetChargeBCards();
}