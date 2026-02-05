using System;
using System.Linq;
using System.Threading.Tasks;
using WingsAPI.Game.Extensions.Groups;
using WingsAPI.Game.Extensions.ItemExtension.Inventory;
using WingsAPI.Game.Extensions.Quicklist;
using WingsEmu.Game._enum;
using WingsEmu.Game._i18n;
using WingsEmu.Game._ItemUsage;
using WingsEmu.Game._ItemUsage.Configuration;
using WingsEmu.Game._ItemUsage.Event;
using WingsEmu.Game.Algorithm;
using WingsEmu.Game.Battle;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Extensions.Mates;
using WingsEmu.Game.Mates;
using WingsEmu.Game.Networking;
using WingsEmu.Game.Skills;
using WingsEmu.Packets.Enums;

namespace NosEmu.Plugins.BasicImplementations.ItemUsage.Etc.Teacher;

public class IncreaseLevelPartnerHandler : IItemHandler
{
    private readonly IBattleEntityAlgorithmService _algorithm;

    private readonly IGameLanguageService _gameLanguage;
    private readonly ISpPartnerConfiguration _spPartnerConfiguration;

    public IncreaseLevelPartnerHandler(IGameLanguageService gameLanguage, IBattleEntityAlgorithmService algorithm, ISpPartnerConfiguration spPartnerConfiguration)
    {
        _gameLanguage = gameLanguage;
        _algorithm = algorithm;
        _spPartnerConfiguration = spPartnerConfiguration;
    }

    public ItemType ItemType => ItemType.PetPartnerItem;
    public long[] Effects => new long[] { 12 };

    public async Task HandleAsync(IClientSession session, InventoryUseItemEvent e)
    {
        if (!int.TryParse(e.Packet[3], out int petId))
        {
            return;
        }

        IMateEntity mateEntity = session.PlayerEntity.MateComponent.GetMate(s => s.Id == petId && s.MateType == MateType.Partner);
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

        switch (mateEntity.MateType)
        {
            case MateType.Partner when mateEntity.Skills is not null:
                mateEntity.RefreshPartnerSkills();
                session.RefreshQuicklist();
                break;
        }
        session.GenerateSpPointPacket();
    }
}