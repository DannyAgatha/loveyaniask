using System.Linq;
using System.Threading.Tasks;
using WingsAPI.Game.Extensions.ItemExtension.Inventory;
using WingsEmu.Game._enum;
using WingsEmu.Game._i18n;
using WingsEmu.Game._ItemUsage;
using WingsEmu.Game._ItemUsage.Event;
using WingsEmu.Game.Battle;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Networking;
using WingsEmu.Game.Networking.Broadcasting;
using WingsEmu.Packets.Enums;
using WingsEmu.Packets.Enums.Character;
using WingsEmu.Packets.Enums.Chat;

namespace NosEmu.Plugins.BasicImplementations.ItemUsage.Main.Special;

public class MagicLampHandler : IItemUsageByVnumHandler
{
    private readonly IGameLanguageService _gameLanguage;

    public MagicLampHandler(IGameLanguageService gameLanguage) => _gameLanguage = gameLanguage;

    public long[] Vnums => new[] { (long)ItemVnums.MAGIC_LAMP };

    public async Task HandleAsync(IClientSession session, InventoryUseItemEvent e)
    {
        if (session.PlayerEntity.IsOnVehicle || !session.PlayerEntity.IsAlive() || session.PlayerEntity.UseSp)
        {
            return;
        }

        if (session.PlayerEntity.EquippedItems.Any(i => i != null && i.ItemInstance.GameItem.Type == InventoryType.EquippedItems))
        {
            session.SendMsg(_gameLanguage.GetLanguage(GameDialogKey.INFORMATION_SHOUTMESSAGE_EQ_NOT_EMPTY, session.UserLanguage), MsgMessageType.Middle);
            return;
        }

        switch (e.Option)
        {
            case 0:
                session.SendQnaPacket($"u_i 1 {session.PlayerEntity.Id} {(byte)e.Item.ItemInstance.GameItem.Type} {e.Item.Slot} 3 ",
                    _gameLanguage.GetLanguage(GameDialogKey.ITEM_DIALOG_ASK_USE, session.UserLanguage));
                break;
            default:
                session.PlayerEntity.Gender = session.PlayerEntity.Gender == GenderType.Female ? GenderType.Male : GenderType.Female;
                session.SendMsg(_gameLanguage.GetLanguage(GameDialogKey.INFORMATION_SHOUTMESSAGE_SEX_CHANGED, session.UserLanguage), MsgMessageType.Middle);

                session.BroadcastEq();
                session.SendGenderPacket();
                session.BroadcastEffect(EffectType.Transform, new RangeBroadcast(session.PlayerEntity.PositionX, session.PlayerEntity.PositionY));
                await session.RemoveItemFromInventory(item: e.Item);
                break;
        }
    }
}