using System.Threading.Tasks;
using WingsEmu.DTOs.Titles;
using WingsEmu.Game._i18n;
using WingsEmu.Game.Buffs;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Managers.StaticData;
using WingsEmu.Game.Networking;
using WingsEmu.Packets.ClientPackets;
using WingsEmu.Packets.Enums.Chat;
using WingsEmu.Packets.Enums.Titles;

namespace WingsEmu.Plugins.PacketHandling.Game.Basic;

public sealed class TitleEquipPacketHandler : GenericGamePacketHandlerBase<TitEqPacket>
{
    private readonly IBCardEffectHandlerContainer _bcardHandler;
    private readonly IItemsManager _itemsManager;
    private readonly IGameLanguageService _languageService;

    public TitleEquipPacketHandler(IGameLanguageService languageService, IItemsManager itemsManager, IBCardEffectHandlerContainer bcardHandler)
    {
        _languageService = languageService;
        _itemsManager = itemsManager;
        _bcardHandler = bcardHandler;
    }

    protected override async Task HandlePacketAsync(IClientSession session, TitEqPacket packet)
    {
        CharacterTitleDto targetTitle = session.PlayerEntity.Titles.Find(x => x.ItemVnum == packet.ItemVnum);
        if (targetTitle == null)
        {
            return;
        }

        switch (packet.Type)
        {
            case TitEqPacketType.EquipAsEffect:
            {
                CharacterTitleDto effectTitle = session.PlayerEntity.Titles.Find(x => x.IsEquipped);
                if (effectTitle != null && targetTitle != effectTitle)
                {
                    effectTitle.IsEquipped = false;
                    session.PlayerEntity.RefreshTitleBCards(_itemsManager, effectTitle, _bcardHandler, true);
                }

                targetTitle.IsEquipped = !targetTitle.IsEquipped;

                session.PlayerEntity.RefreshTitleBCards(_itemsManager, targetTitle, _bcardHandler, !targetTitle.IsEquipped);
                session.SendInfoi(targetTitle.IsEquipped ? Game18NConstString.TitleEffectChangedOrDeactivated : Game18NConstString.TitleEffectDeactivated);
                break;
            }
            case TitEqPacketType.EquipAsVisible:
            {
                CharacterTitleDto visibleTitle = session.PlayerEntity.Titles.Find(x => x.IsVisible);
                if (visibleTitle != null && targetTitle != visibleTitle)
                {
                    visibleTitle.IsVisible = false;
                }

                targetTitle.IsVisible = !targetTitle.IsVisible;
                session.SendInfoi(targetTitle.IsVisible ? Game18NConstString.TitleChangedOrHidden : Game18NConstString.TitleNotDisplayed);
                break;
            }
        }

        session.RefreshStatChar();
        session.RefreshStat();
        session.SendCondPacket();
        session.BroadcastTitleInfo();
        session.SendTitlePacket();
    }
}