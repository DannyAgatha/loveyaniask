using System.Linq;
using System.Threading.Tasks;
using WingsEmu.Game._enum;
using WingsEmu.Game._i18n;
using WingsEmu.Game._NpcDialog;
using WingsEmu.Game._NpcDialog.Event;
using WingsEmu.Game.Characters.Events;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Networking;
using WingsEmu.Packets.Enums;
using WingsEmu.Packets.Enums.Character;
using WingsEmu.Packets.Enums.Chat;

namespace NosEmu.Plugins.BasicImplementations.NpcDialogs
{
    public class ChangeClassHandler : INpcDialogAsyncHandler
    {
        private readonly IGameLanguageService _langService;

        public ChangeClassHandler(IGameLanguageService langService) => _langService = langService;

        public NpcRunType[] NpcRunTypes => new[] { NpcRunType.CHANGE_CLASS};

        public async Task Execute(IClientSession session, NpcDialogEvent e)
        {
            if (e.NpcRunType == NpcRunType.CHANGE_CLASS)
            {
                if (session.PlayerEntity.Class != (byte)ClassType.Adventurer)
                {
                    session.SendMsg(_langService.GetLanguage(GameDialogKey.INFORMATION_SHOUTMESSAGE_NO_ADNVENTURER, session.UserLanguage), MsgMessageType.Middle);
                    return;
                }

                if (session.CantPerformActionOnAct4())
                {
                    return;
                }

                if (session.PlayerEntity.Level < 15 || session.PlayerEntity.JobLevel < 20)
                {
                    session.SendMsg(_langService.GetLanguage(GameDialogKey.INFORMATION_SHOUTMESSAGE_TOO_LOW_LVL, session.UserLanguage), MsgMessageType.Middle);
                    return;
                }

                if (session.PlayerEntity.IsInGroup())
                {
                    session.SendMsg(_langService.GetLanguage(GameDialogKey.INFORMATION_SHOUTMESSAGE_NEED_LEAVE_GROUP, session.UserLanguage), MsgMessageType.Middle);
                    return;
                }

                if (e.Argument == (byte)session.PlayerEntity.Class)
                {
                    return;
                }

                if (e.Argument >= 4 || e.Argument < 0)
                {
                    return;
                }

                if (session.PlayerEntity.EquippedItems.Any(s => s != null && s.ItemInstance.GameItem.Class == (byte)ItemClassType.Adventurer))
                {
                    session.SendMsg(_langService.GetLanguage(GameDialogKey.INFORMATION_SHOUTMESSAGE_EQ_NOT_EMPTY, session.UserLanguage), MsgMessageType.Middle);
                    return;
                }

                await session.EmitEventAsync(new ChangeClassEvent
                {
                    NewClass = (ClassType)e.Argument,
                    ShouldObtainBasicItems = true,
                    ShouldObtainNewFaction = true,
                    ShouldResetJobLevel = true
                });
            }
        }
    }
}
