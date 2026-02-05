using System;
using System.Threading.Tasks;
using WingsAPI.Game.Extensions.ItemExtension.Inventory;
using WingsEmu.Game._i18n;
using WingsEmu.Game._ItemUsage;
using WingsEmu.Game._ItemUsage.Event;
using WingsEmu.Game.Characters;
using WingsEmu.Game.Characters.Events;
using WingsEmu.Game.Entities;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Helpers.Damages;
using WingsEmu.Game.Maps;
using WingsEmu.Game.Networking;
using WingsEmu.Packets.Enums;
using WingsEmu.Packets.Enums.Chat;

namespace NosEmu.Plugins.BasicImplementations.ItemUsage.Etc.Teacher;

public class SteelNetHandler : IItemHandler
{
    public ItemType ItemType => ItemType.PetPartnerItem;
    public long[] Effects => new long[] { 10001 };

    public async Task HandleAsync(IClientSession session, InventoryUseItemEvent e)
    {
        if (session.PlayerEntity == null)
        {
            return;
        }

        IMonsterEntity monsterEntityToCapture = session.CurrentMapInstance?.GetMonsterById(session.PlayerEntity.LastEntity.Item2);

        if (monsterEntityToCapture == null)
        {
            return;
        }

        int dist = session.PlayerEntity.GetDistance(monsterEntityToCapture);
        if (dist > 2)
        {
            return;
        }

        if (session.PlayerEntity.LastMonsterCaught.AddSeconds(2) > DateTime.UtcNow)
        {
            return;
        }

        IPlayerEntity playerEntity = session.PlayerEntity;

        if (monsterEntityToCapture.Level > playerEntity.Level)
        {
            session.SendMsg(session.GetLanguage(GameDialogKey.SKILL_SHOUTMESSAGE_MONSTER_LEVEL_MUST_BE_LOWER_THAN_YOURS), MsgMessageType.Middle);
            return;
        }

        if (playerEntity.MapInstance.MapInstanceType == MapInstanceType.RaidInstance)
        {
            session.SendMsg(session.GetLanguage(GameDialogKey.SKILL_SHOUTMESSAGE_CAPTURE_IN_RAID), MsgMessageType.Middle);
            return;
        }

        if (monsterEntityToCapture.GetHpPercentage() >= 50)
        {
            session.SendMsg(session.GetLanguage(GameDialogKey.SKILL_SHOUTMESSAGE_MONSTER_MUST_BE_LOW_HP), MsgMessageType.Middle);
            return;
        }

        if (playerEntity.MaxPetCount <= playerEntity.MateComponent.GetMates(x => x.MateType == MateType.Pet).Count)
        {
            session.SendMsg(session.GetLanguage(GameDialogKey.INFORMATION_SHOUTMESSAGE_MAX_PET_COUNT), MsgMessageType.Middle);
            return;
        }

        if (!monsterEntityToCapture.CanBeCaught)
        {
            session.SendMsg(session.GetLanguage(GameDialogKey.SKILL_SHOUTMESSAGE_CAPTURE_IMPOSSIBLE), MsgMessageType.Middle);
            return;
        }

        if (playerEntity.GetDignityIco() > 3)
        {
            session.SendMsg(session.GetLanguage(GameDialogKey.SKILL_SHOUTMESSAGE_CAPTURE_DIGNITY_LOW), MsgMessageType.Middle);
            return;
        }

        await session.EmitEventAsync(new MonsterCaptureEvent(monsterEntityToCapture, false));
        session.PlayerEntity.LastMonsterCaught = DateTime.UtcNow;

        await session.RemoveItemFromInventory(item: e.Item);
    }
}