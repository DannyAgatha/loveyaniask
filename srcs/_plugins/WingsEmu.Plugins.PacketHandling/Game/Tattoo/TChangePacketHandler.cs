using System.Linq;
using System.Threading.Tasks;
using WingsAPI.Game.Extensions.ItemExtension;
using WingsAPI.Game.Extensions.ItemExtension.Inventory;
using WingsAPI.Game.Extensions.Quicklist;
using WingsAPI.Packets.ClientPackets;
using WingsAPI.Packets.Enums;
using WingsAPI.Packets.Enums.Chat;
using WingsEmu.Game;
using WingsEmu.Game._enum;
using WingsEmu.Game.Configurations.Act7.Tattoos;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Networking;
using WingsEmu.Game.Skills;
using WingsEmu.Packets.Enums;
using WingsEmu.Packets.Enums.Chat;

namespace WingsEmu.Plugins.PacketHandling.Game.Tattoo;

public class TChangePacketHandler : GenericGamePacketHandlerBase<TchangePacket>
{
    private readonly TattooOptionsConfiguration _tattooOptionsConfiguration;
    private readonly IRandomGenerator _randomGenerator;

    public TChangePacketHandler(TattooOptionsConfiguration tattooOptionsConfiguration, IRandomGenerator randomGenerator)
    {
        _tattooOptionsConfiguration = tattooOptionsConfiguration;
        _randomGenerator = randomGenerator;
    }
    
    protected override async Task HandlePacketAsync(IClientSession session, TchangePacket packet)
    {
        if (session.PlayerEntity.Skills.Any(s => !session.PlayerEntity.SkillCanBeUsed(s)))
        {
            session.SendMsgi(MessageType.Default, Game18NConstString.AbleToRemoveTattooWhenCooldownOff);
            session.SendShopEndPacket(ShopEndType.Player);
            return;
        }
        
        ListTattooOptions selectedTattooOptions = _tattooOptionsConfiguration.Tattoos.FirstOrDefault(s => s.TattooOptions.Contains(packet.TattooVnum));
        
        if (selectedTattooOptions == null)
        {
            session.SendShopEndPacket(ShopEndType.Item);
            return;
        }

        switch (packet.Type)
        {
            case TattooChangeType.LoaChange:
            {
                if (!session.PlayerEntity.HasItem((short)ItemVnums.LOA_CRISTAL_CHANGE) &&
                    !session.PlayerEntity.HasItem((short)ItemVnums.LOA_CRISTAL_CHANGE_LIMITED))
                {
                    session.SendShopEndPacket(ShopEndType.Item);
                    return;
                }

                ListTattooOptions tattooOptions = _tattooOptionsConfiguration.Tattoos.FirstOrDefault(s => s.CastId == packet.Data);
                
                if (tattooOptions is null)
                {
                    session.SendShopEndPacket(ShopEndType.Item);
                    session.SendShopEndPacket(ShopEndType.Npc);
                    return;
                }
                
                int randomOption = tattooOptions.TattooOptions[_randomGenerator.RandomNumber(tattooOptions.TattooOptions.Count)];
                
                session.PlayerEntity.CharacterSkills.TryGetValue(packet.TattooVnum, out CharacterSkill skill);

                if (skill is null)
                {
                    session.SendShopEndPacket(ShopEndType.Item);
                    session.SendShopEndPacket(ShopEndType.Npc);
                    return;
                }

                var newSkill = new CharacterSkill
                {
                    SkillVNum = randomOption,
                    UpgradeSkill = skill.UpgradeSkill
                };

                session.SendGuriPacket((byte)GuriType.OpenTattoo);
                AddTattooSKill(session, randomOption, newSkill);
                RemoveTattoo(session, packet.TattooVnum);
                SendSkillPackets(session);
                session.SendSound(SoundType.REMOVED_TATTOO);
                session.SendInfoi(Game18NConstString.SwitchedToTattoo, 5, randomOption);
                session.SendSayi(ChatMessageColorType.Red, Game18NConstString.SwitchedToTattoo, 5, randomOption);
                await RemoveItems(session, (short)ItemVnums.LOA_CRISTAL_CHANGE_LIMITED, (short)ItemVnums.LOA_CRISTAL_CHANGE);
            }
                break;

            case TattooChangeType.Chosen:
            {
                if (!session.PlayerEntity.HasItem((short)ItemVnums.TATTOO_SCROLL) &&
                    !session.PlayerEntity.HasItem((short)ItemVnums.TATTOO_SCROLL_LIMITED))
                {
                    session.SendShopEndPacket(ShopEndType.Item);
                    return;
                }
                
                session.PlayerEntity.CharacterSkills.TryGetValue(packet.TattooVnum, out CharacterSkill skill);

                if (skill is null)
                {
                    session.SendShopEndPacket(ShopEndType.Item);
                    session.SendShopEndPacket(ShopEndType.Npc);
                    return;
                }
                
                var newSkill = new CharacterSkill
                {
                    SkillVNum = packet.Data,
                    UpgradeSkill = skill.UpgradeSkill
                };

                if (!selectedTattooOptions.TattooOptions.Contains(packet.Data))
                {
                    session.SendShopEndPacket(ShopEndType.Item);
                    return;
                }

                AddTattooSKill(session, packet.Data, newSkill);
                RemoveTattoo(session, packet.TattooVnum);
                SendSkillPackets(session);
                session.SendSound(SoundType.REMOVED_TATTOO);
                session.SendInfoi(Game18NConstString.SwitchedToTattoo, 5, packet.Data);
                session.SendSayi(ChatMessageColorType.Red, Game18NConstString.SwitchedToTattoo, 5, packet.Data);
                await RemoveItems(session, (short)ItemVnums.TATTOO_SCROLL_LIMITED, (short)ItemVnums.TATTOO_SCROLL);
            }
                break;

            case TattooChangeType.Random:
            {
                if (!session.PlayerEntity.HasItem((short)ItemVnums.RANDOM_TATTOO_SCROLL) &&
                    !session.PlayerEntity.HasItem((short)ItemVnums.RANDOM_TATTOO_SCROLL_LIMITED))
                {
                    session.SendShopEndPacket(ShopEndType.Item);
                    return;
                }
                
                var tattooOptions = selectedTattooOptions.TattooOptions.ToList();
                var filteredOptions = tattooOptions.Where(option => option != packet.TattooVnum).ToList();
                int randomOption = filteredOptions[_randomGenerator.RandomNumber(filteredOptions.Count)];
                
                session.PlayerEntity.CharacterSkills.TryGetValue(packet.TattooVnum, out CharacterSkill skill);

                if (skill is null)
                {
                    session.SendShopEndPacket(ShopEndType.Item);
                    session.SendShopEndPacket(ShopEndType.Npc);
                    return;
                }

                var newSkill = new CharacterSkill
                {
                    SkillVNum = randomOption,
                    UpgradeSkill = skill.UpgradeSkill
                };

                AddTattooSKill(session, randomOption, newSkill);
                RemoveTattoo(session, packet.TattooVnum);
                SendSkillPackets(session);
                session.SendSound(SoundType.REMOVED_TATTOO);
                session.SendInfoi(Game18NConstString.SwitchedToTattoo, 5, randomOption);
                session.SendSayi(ChatMessageColorType.Red, Game18NConstString.SwitchedToTattoo, 5, randomOption);
                await RemoveItems(session, (short)ItemVnums.RANDOM_TATTOO_SCROLL_LIMITED, (short)ItemVnums.RANDOM_TATTOO_SCROLL);
            }
                break;
        }
    }

    private void AddTattooSKill(IClientSession session, int option, CharacterSkill newSkill)
    {
        session.PlayerEntity.CharacterSkills.TryAdd(option, newSkill);
        session.PlayerEntity.Skills.Add(newSkill);
    }

    private void SendSkillPackets(IClientSession session)
    {
        session.RefreshSkillList();
        session.RefreshQuicklist();
        session.SendShopEndPacket(ShopEndType.Item);
    }

    private async Task RemoveItems(IClientSession session, short limitedItem, short normalItem)
    {
        if (session.PlayerEntity.HasItem(limitedItem))
        {
            await session.RemoveItemFromInventory(limitedItem);
            return;
        }
        await session.RemoveItemFromInventory(normalItem);
    }
    
    private void RemoveTattoo(IClientSession session, int tattooVnum)
    {
        IBattleEntitySkill toRemove = session.PlayerEntity.Skills.FirstOrDefault(x => x.Skill.Id == tattooVnum);
        if (toRemove == null)
        {
            return;
        }

        session.PlayerEntity.CharacterSkills.TryRemove(toRemove.Skill.Id, out CharacterSkill value);
        session.PlayerEntity.Skills.Remove(value);
    }
}