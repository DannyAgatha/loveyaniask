using System.Linq;
using System.Threading.Tasks;
using WingsAPI.Game.Extensions.Groups;
using WingsAPI.Game.Extensions.ItemExtension.Inventory;
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
using WingsEmu.Packets.Enums;

namespace NosEmu.Plugins.BasicImplementations.ItemUsage.Etc.Special;

public class StrangePartnerFoodHandler : IItemHandler
{
    private readonly IBattleEntityAlgorithmService _algorithm;
    private readonly ICharacterAlgorithm _characterAlgorithm;
    private readonly IGameLanguageService _gameLanguage;
    private readonly ISpPartnerConfiguration _spPartnerConfiguration;

    public StrangePartnerFoodHandler(IGameLanguageService gameLanguage, IBattleEntityAlgorithmService algorithm, ICharacterAlgorithm characterAlgorithm, ISpPartnerConfiguration spPartnerConfiguration)
    {
        _gameLanguage = gameLanguage;
        _algorithm = algorithm;
        _characterAlgorithm = characterAlgorithm;
        _spPartnerConfiguration = spPartnerConfiguration;
    }

    public ItemType ItemType => ItemType.PetPartnerItem;
    public long[] Effects => new long[] { 18 };

    public async Task HandleAsync(IClientSession session, InventoryUseItemEvent e)
    {
        if (e.Packet == null || e.Packet.Length == 0)
        {
            return;
        }

        if (!int.TryParse(e.Packet[3], out int mateId))
        {
            return;
        }

        IMateEntity mateEntity = session.PlayerEntity.MateComponent.GetMate(x => x.Id == mateId && x.MateType == MateType.Partner);
        if (mateEntity == null)
        {
            return;
        }

        if (!mateEntity.IsAlive())
        {
            return;
        }

        if (mateEntity.Level - 1 <= 0)
        {
            return;
        }

        if (mateEntity.IsUsingSp)
        {
            session.SendInfo(_gameLanguage.GetLanguage(GameDialogKey.PARTNER_INFO_IS_WEARING_SP, e.Sender.UserLanguage));
            return;
        }

        if (session.PlayerEntity.PartnerGetEquippedItems(mateEntity.PetSlot).Any(x => x != null))
        {
            return;
        }

        if (session.PlayerEntity.IsOnVehicle)
        {
            return;
        }

        long mateXp = _characterAlgorithm.GetLevelXp((short)(mateEntity.Level - 1), true, mateEntity.MateType);

        mateEntity.Level -= 1;
        mateEntity.Experience = mateXp;
        mateEntity.RefreshMaxHpMp(_algorithm);
        session.RefreshParty(_spPartnerConfiguration);
        mateEntity.Hp = mateEntity.MaxHp;
        mateEntity.Mp = mateEntity.MaxMp;
        session.SendPetInfo(mateEntity, _gameLanguage);
        session.SendMateEffect(mateEntity, EffectType.PetLoveBroke);
        await session.RemoveItemFromInventory(item: e.Item);
    }
}