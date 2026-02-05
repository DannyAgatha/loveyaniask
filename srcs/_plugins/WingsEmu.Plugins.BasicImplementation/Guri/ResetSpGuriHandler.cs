using System.Threading.Tasks;
using WingsAPI.Game.Extensions.ItemExtension.Inventory;
using WingsAPI.Game.Extensions.ItemExtension.Item;
using WingsAPI.Packets.Enums.Chat;
using WingsEmu.Game._enum;
using WingsEmu.Game._Guri;
using WingsEmu.Game._Guri.Event;
using WingsEmu.Game._i18n;
using WingsEmu.Game.Algorithm;
using WingsEmu.Game.Characters.Events;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Items;
using WingsEmu.Game.Managers.StaticData;
using WingsEmu.Game.Networking;
using WingsEmu.Packets.Enums.Chat;

namespace NosEmu.Plugins.BasicImplementations.Guri;

public class ResetSpGuriHandler : IGuriHandler
{
    private readonly ICharacterAlgorithm _characterAlgorithm;
    private readonly IGameLanguageService _gameLanguageService;
    private readonly IItemsManager _itemsManager;

    public ResetSpGuriHandler(IGameLanguageService gameLanguageService, ICharacterAlgorithm characterAlgorithm, IItemsManager itemsManager)
    {
        _gameLanguageService = gameLanguageService;
        _characterAlgorithm = characterAlgorithm;
        _itemsManager = itemsManager;
    }

    public long GuriEffectId => 203;

    private bool UserHasPotion (IClientSession session) => session.PlayerEntity.HasItem((short)ItemVnums.RESET_SP_POINT) || session.PlayerEntity.HasItem((short)ItemVnums.RESET_SP_POINT_LIMITED);
    
    private async Task RemoveItem(IClientSession session)
    {
        short itemVNumToRemove = session.PlayerEntity.HasItem((short)ItemVnums.RESET_SP_POINT_LIMITED)
            ? (short)ItemVnums.RESET_SP_POINT_LIMITED
            : (short)ItemVnums.RESET_SP_POINT;
        
        await session.RemoveItemFromInventory(itemVNumToRemove);
    }


    public async Task ExecuteAsync(IClientSession session, GuriEvent guriPacket)
    {
        if (guriPacket.Data != 0)
        {
            return;
        }

        if (session.PlayerEntity.IsSeal)
        {
            return;
        }

        bool hasPotion = UserHasPotion(session);

        if (!hasPotion)
        {
            session.SendInfoi(Game18NConstString.NotEnoughSpecialistPoints);
            return;
        }

        GameItemInstance specialistInstance = session.PlayerEntity.Specialist;
        if (specialistInstance == null)
        {
            session.SendMsgi(MessageType.Default, Game18NConstString.SpecialistCardMustBeEquipped);
            return;
        }

        await RemoveItem(session);
        specialistInstance.SlDamage = 0;
        specialistInstance.SlDefence = 0;
        specialistInstance.SlElement = 0;
        specialistInstance.SlHp = 0;

        session.SendCondPacket();
        session.SendSpecialistCardInfo(specialistInstance, _characterAlgorithm);
        session.RefreshLevel(_characterAlgorithm);
        session.RefreshStatChar();
        session.SendMsgi(MessageType.Default, Game18NConstString.PointsRestored);
    }
}