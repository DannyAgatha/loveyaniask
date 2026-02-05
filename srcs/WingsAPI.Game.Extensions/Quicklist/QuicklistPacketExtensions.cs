using System.Collections.Generic;
using System.Text;
using WingsEmu.DTOs.Quicklist;
using WingsEmu.Game._enum;
using WingsEmu.Game.Buffs;
using WingsEmu.Game.Networking;
using WingsEmu.Packets.Enums;
using WingsEmu.Packets.Enums.Character;

namespace WingsAPI.Game.Extensions.Quicklist
{
    public static class QuicklistPacketExtensions
    {
        public static void RefreshQuicklist(this IClientSession session, bool force = false) => session.SendPackets(session.GenerateQuicklist(force));
        
        public static IEnumerable<string> GenerateQuicklist(this IClientSession session, bool forceAmSkill)
        {
            // to rework
            StringBuilder[] pktQs = { new("qslot 0"), new("qslot 1") };

            int morphId = session.PlayerEntity.UseSp ? session.PlayerEntity.Specialist?.GameItem.Morph ?? 0 : 0;

            if ((session.PlayerEntity.Class == ClassType.MartialArtist && session.PlayerEntity.Morph == (byte)MorphType.DraconicFistTransformed) || session.PlayerEntity.Morph == (byte)MorphType.DraconicFist)
            {
                morphId = (int)MorphType.DraconicFistTransformed;
            }

            switch (session.PlayerEntity.Class)
            {
                case ClassType.MartialArtist when session.PlayerEntity.Specialist != null && session.PlayerEntity.Morph == (int)MorphType.MysticArts && session.PlayerEntity.UseSp && 
                    session.PlayerEntity.Specialist.SpLevel >= 20 && forceAmSkill:
                case ClassType.MartialArtist when session.PlayerEntity.Specialist != null && session.PlayerEntity.Morph == (int)MorphType.MysticArts && session.PlayerEntity.UseSp && 
                    session.PlayerEntity.Specialist.SpLevel >= 20 && session.PlayerEntity.BCardComponent.HasBCard(BCardType.MysticArts, (byte)AdditionalTypes.MysticArts.LotusFlowerSkillUse):
                    for (short i = 0; i < 30; i++)
                    {
                        for (short j = 0; j < 2; j++)
                        {
                            IReadOnlyList<CharacterQuicklistEntryDto> tmp = session.PlayerEntity.QuicklistComponent.GetQuicklistByTab(j, morphId);

                            if (tmp?[i] == null)
                            {
                                pktQs[j].Append(" 0.-1.-1");
                                continue;
                            }

                            CharacterQuicklistEntryDto qi = tmp[i];

                            short? pos = qi?.InvSlotOrSkillSlotOrSkillVnum;

                            if (pos is >= 6 and <= 9)
                            {
                                pos += 5;
                            }

                            pktQs[j].Append($" {(byte?)qi?.Type ?? 0}.{qi?.InventoryTypeOrSkillTab ?? -1}.{pos.ToString() ?? "-1"}");
                        }
                    }
                    break;

                case ClassType.MartialArtist when session.PlayerEntity.Specialist != null && session.PlayerEntity.Morph == (int)MorphType.MasterWolf && session.PlayerEntity.UseSp &&
                    session.PlayerEntity.Specialist.SpLevel >= 20 && forceAmSkill:
                case ClassType.MartialArtist when session.PlayerEntity.Specialist != null && session.PlayerEntity.Morph == (int)MorphType.MasterWolf && session.PlayerEntity.UseSp &&
                    session.PlayerEntity.Specialist.SpLevel >= 20 && session.PlayerEntity.BCardComponent
                        .HasBCard(BCardType.IncreaseDamageDebuffs, (byte)AdditionalTypes.IncreaseDamageDebuffs.CanUseUltimateSkills):
                    for (short i = 0; i < 30; i++)
                    {
                        for (short j = 0; j < 2; j++)
                        {
                            IReadOnlyList<CharacterQuicklistEntryDto> tmp = session.PlayerEntity.QuicklistComponent.GetQuicklistByTab(j, morphId);

                            if (tmp?[i] == null)
                            {
                                pktQs[j].Append(" 0.-1.-1");
                                continue;
                            }

                            CharacterQuicklistEntryDto qi = tmp[i];

                            short? pos = qi?.InvSlotOrSkillSlotOrSkillVnum;

                            switch (pos)
                            {
                                case 3 when session.PlayerEntity.EnergyBar >= 2000:
                                case 4 when session.PlayerEntity.EnergyBar >= 1000:
                                case 5 when session.PlayerEntity.EnergyBar >= 3000:
                                    pos += 8;
                                    break;
                                case 10 when session.PlayerEntity.EnergyBar >= 3000:
                                    pos += 4;
                                    break;
                            }

                            pktQs[j].Append($" {(byte?)qi?.Type ?? 0}.{qi?.InventoryTypeOrSkillTab ?? -1}.{pos.ToString() ?? "-1"}");
                        }
                    }
                    break;

                default:
                    for (short i = 0; i < 30; i++)
                    {
                        for (short j = 0; j < 2; j++)
                        {
                            IReadOnlyList<CharacterQuicklistEntryDto> tmp = session.PlayerEntity.QuicklistComponent.GetQuicklistByTab(j, morphId);

                            if (tmp?[i] == null)
                            {
                                pktQs[j].Append(" 0.-1.-1");
                                continue;
                            }

                            CharacterQuicklistEntryDto qi = tmp[i];
                            pktQs[j].Append($" {(byte?)qi?.Type ?? 0}.{qi?.InventoryTypeOrSkillTab ?? -1}.{qi?.InvSlotOrSkillSlotOrSkillVnum.ToString() ?? "-1"}");
                        }
                    }
                    break;
            }
            
            return new[] { pktQs[0].ToString(), pktQs[1].ToString() };
        }

        public static void SendQuicklistSlot(this IClientSession session, CharacterQuicklistEntryDto entry, short? skillCastId = null)
        {
            session.SendPacket(session.GenerateQsetPacket(entry, skillCastId));
        }

        public static string GenerateQsetPacket(this IClientSession session, CharacterQuicklistEntryDto quicklistEntryDto, short? skillCastId = null) =>
            $"qset {quicklistEntryDto.QuicklistTab} {quicklistEntryDto.QuicklistSlot} {(byte)quicklistEntryDto.Type}.{quicklistEntryDto.InventoryTypeOrSkillTab}.{skillCastId ?? quicklistEntryDto.InvSlotOrSkillSlotOrSkillVnum}.0";

        public static void SendEmptyQuicklistSlot(this IClientSession session, short tab, short slot)
        {
            session.SendPacket(session.GenerateEmptyQsetPacket(tab, slot));
        }

        public static string GenerateEmptyQsetPacket(this IClientSession session, short tab, short slot) => $"qset {tab} {slot} 0.-1.-1.0";
    }
}