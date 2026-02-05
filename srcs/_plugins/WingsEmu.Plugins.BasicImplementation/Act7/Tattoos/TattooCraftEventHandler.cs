using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using PhoenixLib.Events;
using WingsAPI.Game.Extensions.ItemExtension;
using WingsAPI.Game.Extensions.ItemExtension.Inventory;
using WingsAPI.Game.Extensions.Quicklist;
using WingsAPI.Packets.Enums.Chat;
using WingsEmu.Game;
using WingsEmu.Game._enum;
using WingsEmu.Game._i18n;
using WingsEmu.Game.Act7;
using WingsEmu.Game.Act7.Tattoos;
using WingsEmu.Game.Characters.Events;
using WingsEmu.Game.Configurations.Act7.Tattoos;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Inventory;
using WingsEmu.Game.Networking;
using WingsEmu.Game.Skills;
using WingsEmu.Packets.Enums;
using WingsEmu.Packets.Enums.Chat;

namespace NosEmu.Plugins.BasicImplementations.Act7.Tattoos;

public class TattooCraftEventHandler : IAsyncEventProcessor<TattooCraftEvent>
{
    private readonly CraftTattooItemsConfiguration _craftTattooItemsConfiguration;
    private readonly TattooOptionsConfiguration _tattooOptionsConfiguration;
    private readonly IRandomGenerator _randomGenerator;

    public TattooCraftEventHandler(CraftTattooItemsConfiguration craftTattooItemsConfiguration, TattooOptionsConfiguration tattooConfiguration, IRandomGenerator randomGenerator)
    {
        _craftTattooItemsConfiguration = craftTattooItemsConfiguration;
        _tattooOptionsConfiguration = tattooConfiguration;
        _randomGenerator = randomGenerator;
    }
    
    public async Task HandleAsync(TattooCraftEvent e, CancellationToken cancellation)
    {
        IClientSession session = e.Sender;
        InventoryItem item = e.Pattern;
        
        IEnumerable<IBattleEntitySkill> tattooSkills = 
            session.PlayerEntity.CharacterSkills.Values.Where(s => s.Skill.CastId is >= 40 and <= 44);
        
        if (tattooSkills.Count() >= 2)
        {
            session.SendMsgi(MessageType.Default, Game18NConstString.CanNotHaveOtherTatoo);
            session.SendShopEndPacket(ShopEndType.Npc);
            return;
        }
        
        ListTattooOptions selectedTattooOptions = _tattooOptionsConfiguration.Tattoos
            .FirstOrDefault(s => s.ItemVnum == item.ItemInstance.ItemVNum);

        if (selectedTattooOptions == null || !selectedTattooOptions.TattooOptions.Any())
        {
            return;
        }
        
        bool typeAlreadyUsed = session.PlayerEntity.Skills.Any(skill => selectedTattooOptions.TattooOptions.Contains(skill.Skill.Id));

        if (typeAlreadyUsed)
        {
            session.SendSayi(ChatMessageColorType.Red, Game18NConstString.AlreadyHaveTatooType);
            session.SendShopEndPacket(ShopEndType.Npc);
            return;
        }
        
        if (!session.PlayerEntity.RemoveGold(_craftTattooItemsConfiguration.Gold))
        {
            session.SendChatMessage(session.GetLanguage(GameDialogKey.INTERACTION_MESSAGE_NOT_ENOUGH_GOLD), ChatMessageColorType.PlayerSay);
            return;
        }

        int randomOption = selectedTattooOptions.TattooOptions[_randomGenerator.RandomNumber(selectedTattooOptions.TattooOptions.Count)];
        
        foreach (TattooItem items in _craftTattooItemsConfiguration.Items)
        {
            if (!session.PlayerEntity.HasItem(items.Vnum, items.Quantity))
            {
                return;
            }
                    
            await session.RemoveItemFromInventory(items.Vnum, items.Quantity);
        }
        
        var newSkill = new CharacterSkill
        {
            SkillVNum = randomOption
        };

        session.PlayerEntity.CharacterSkills.TryAdd(randomOption, newSkill);
        session.PlayerEntity.Skills.Add(newSkill);
        session.RefreshSkillList();
        session.RefreshQuicklist();
        session.SendSound(SoundType.CRAFTING_SUCCESS);
        await session.RemoveItemFromInventory(item.ItemInstance.ItemVNum);
        session.SendShopEndPacket(ShopEndType.Npc);
        session.SendMsgi(MessageType.Default, Game18NConstString.TattooAplied, 5, randomOption);
        session.SendSayi(ChatMessageColorType.Red, Game18NConstString.TattooAplied, 5, randomOption);
        session.SendEffect(EffectType.UpgradeSuccess);
        session.SendGuriPacket((byte)GuriType.OpenTattoo);
    }
}