using System;
using System.Threading.Tasks;
using WingsAPI.Game.Extensions.Groups;
using WingsAPI.Game.Extensions.ItemExtension.Inventory;
using WingsAPI.Packets.Enums;
using WingsEmu.Game._enum;
using WingsEmu.Game._i18n;
using WingsEmu.Game._ItemUsage;
using WingsEmu.Game._ItemUsage.Configuration;
using WingsEmu.Game._ItemUsage.Event;
using WingsEmu.Game.Algorithm;
using WingsEmu.Game.Battle;
using WingsEmu.Game.Characters.Events;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Extensions.Mates;
using WingsEmu.Game.Helpers;
using WingsEmu.Game.Mates;
using WingsEmu.Game.Mates.PetEvolution;
using WingsEmu.Game.Networking;
using WingsEmu.Game.Skills;
using WingsEmu.Packets.Enums;

namespace NosEmu.Plugins.BasicImplementations.ItemUsage.Etc.Teacher;

public class IncreaseLevelPetFoodHandler : IItemHandler
{
    private readonly IBattleEntityAlgorithmService _algorithm;

    private readonly IGameLanguageService _gameLanguage;
    private readonly ISpPartnerConfiguration _spPartnerConfiguration;

    public IncreaseLevelPetFoodHandler(IGameLanguageService gameLanguage, IBattleEntityAlgorithmService algorithm, ISpPartnerConfiguration spPartnerConfiguration)
    {
        _gameLanguage = gameLanguage;
        _algorithm = algorithm;
        _spPartnerConfiguration = spPartnerConfiguration;
    }

    public ItemType ItemType => ItemType.PetPartnerItem;
    public long[] Effects => [11];

    public async Task HandleAsync(IClientSession session, InventoryUseItemEvent e)
    {
        if (!int.TryParse(e.Packet[3], out int petId))
        {
            return;
        }

        IMateEntity mateEntity = session.PlayerEntity.MateComponent.GetMate(s => s.Id == petId && s.MateType == MateType.Pet);
        if (mateEntity == null || mateEntity.Level >= session.PlayerEntity.Level - 5)
        {
            return;
        }

        if (!mateEntity.IsAlive())
        {
            return;
        }

        if (session.PlayerEntity.IsOnVehicle)
        {
            return;
        }

        int loyalty = mateEntity.Loyalty + 100 > 1000 ? 1000 - mateEntity.Loyalty : 100;
        mateEntity.Loyalty += (short)loyalty;
        mateEntity.Experience = 0;
        mateEntity.Level++;
        mateEntity.RefreshMaxHpMp(_algorithm);
        session.RefreshParty(_spPartnerConfiguration);
        mateEntity.Hp = mateEntity.MaxHp;
        mateEntity.Mp = mateEntity.MaxMp;
        session.SendPetInfo(mateEntity, _gameLanguage);
        await session.RemoveItemFromInventory(item: e.Item);
        mateEntity.BroadcastEffectInRange(EffectType.NormalLevelUp);
        mateEntity.BroadcastEffectInRange(EffectType.NormalLevelUpSubEffect);
        
        foreach (IBattleEntitySkill battleEntitySkill in mateEntity.Skills)
        {
            battleEntitySkill.LastUse = DateTime.MinValue;
        }

        await session.EmitEventAsync(new LevelUpMateEvent
        {
            Level = mateEntity.Level,
            LevelUpType = MateLevelUpType.ItemUsed,
            ItemVnum = e.Item.ItemInstance.ItemVNum,
            NosMateMonsterVnum = mateEntity.NpcMonsterVNum
        });
        
        await session.EmitEventAsync(new PetLevelUpEvolutionEvent
        {
            Sender = session,
            Level = mateEntity.Level,
            LevelType = PetLevelType.Level,
            Location = new Location(mateEntity.MapInstance.MapId, mateEntity.MapX, mateEntity.MapY),
            NosMateMonsterVnum = mateEntity.NpcMonsterVNum,
            ItemVnum = e.Item.ItemInstance.ItemVNum
        });
    }
}