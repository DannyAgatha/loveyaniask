using System.Linq;
using System.Threading.Tasks;
using WingsAPI.Game.Extensions.ItemExtension.Inventory;
using WingsEmu.DTOs.Maps;
using WingsEmu.Game._ItemUsage;
using WingsEmu.Game._ItemUsage.Event;
using WingsEmu.Game.Battle;
using WingsEmu.Game.Characters;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Helpers.Damages;
using WingsEmu.Game.Networking;
using WingsEmu.Packets.Enums;

namespace NosEmu.Plugins.BasicImplementations.ItemUsage.Etc.Magical;

public class TeamStoneHandler : IItemHandler
{
    public ItemType ItemType => ItemType.Magical;
    public long[] Effects => new long[] { 300 };

    public async Task HandleAsync(IClientSession session, InventoryUseItemEvent e)
    {
        IPlayerEntity character = session.PlayerEntity;

        if (!character.IsInRaidParty)
        {
            return;
        }

        if (!character.IsRaidLeader(character.Id))
        {
            return;
        }

        if (session.CurrentMapInstance.Portals.All(x => x.Type != PortalType.Raid))
        {
            return;
        }

        foreach (IClientSession member in character.Raid.Members)
        {
            if (member.PlayerEntity.Id == character.Id)
            {
                continue;
            }

            if (!member.CurrentMapInstance.HasMapFlag(MapFlags.IS_BASE_MAP))
            {
                continue;
            }

            if (member.CurrentMapInstance?.Id == character.MapInstance.Id)
            {
                continue;
            }

            if (!member.PlayerEntity.IsAlive())
            {
                member.PlayerEntity.Hp = 1;
                member.PlayerEntity.Mp = 1;
            }

            Position randomPosition = character.MapInstance.GetRandomPosition();
            member.ChangeMap(session.PlayerEntity.MapInstanceId, randomPosition.X, randomPosition.Y);
        }

        await session.RemoveItemFromInventory(item: e.Item);
    }
}