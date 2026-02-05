using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using PhoenixLib.Events;
using WingsAPI.Game.Extensions.ItemExtension;
using WingsAPI.Game.Extensions.ItemExtension.Inventory;
using WingsAPI.Game.Extensions.Quicklist;
using WingsAPI.Packets.Enums.Chat;
using WingsEmu.Game._enum;
using WingsEmu.Game.Act7.Tattoos;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Networking;
using WingsEmu.Game.Skills;
using WingsEmu.Packets.Enums;
using WingsEmu.Packets.Enums.Chat;

namespace NosEmu.Plugins.BasicImplementations.Act7.Tattoos;

public class DeleteTattooEventHandler : IAsyncEventProcessor<DeleteTattooEvent>
{
    public async Task HandleAsync(DeleteTattooEvent e, CancellationToken cancellation)
    {
        IClientSession session = e.Sender;
        IBattleEntitySkill tattooSkill = e.TattooSkill;

        if (!session.PlayerEntity.CharacterSkills.TryGetValue(tattooSkill.Skill.Id, out CharacterSkill skill))
        {
            session.SendShopEndPacket(ShopEndType.Npc);
            return;
        }

        if (!session.PlayerEntity.HasItem((short)ItemVnums.TATTOO_REMOVER))
        {
            session.SendShopEndPacket(ShopEndType.Npc);
            return;
        }
        
        if (session.PlayerEntity.Skills.Any(s => !session.PlayerEntity.SkillCanBeUsed(s)))
        {
            session.SendMsgi(MessageType.Default, Game18NConstString.AbleToRemoveTattooWhenCooldownOff);
            session.SendShopEndPacket(ShopEndType.Npc);
            return;
        }
        
        session.PlayerEntity.CharacterSkills.TryRemove(skill.Skill.Id, out CharacterSkill value);
        session.PlayerEntity.Skills.Remove(value);
        session.RefreshSkillList();
        session.SendInfoi(Game18NConstString.TattooRemoved, 5, tattooSkill.Skill.Id);
        session.SendSayi(ChatMessageColorType.Red, Game18NConstString.TattooRemoved, 5, tattooSkill.Skill.Id);
        session.RefreshQuicklist();
        session.SendSound(SoundType.REMOVED_TATTOO);
        await session.RemoveItemFromInventory((short)ItemVnums.TATTOO_REMOVER);
        session.SendShopEndPacket(ShopEndType.Item);
        session.SendShopEndPacket(ShopEndType.Npc);
        session.SendShopEndPacket(ShopEndType.Player);
    }
}
