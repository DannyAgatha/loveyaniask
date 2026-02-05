using System;
using System.Collections.Generic;
using System.Linq;
using WingsEmu.Game._i18n;
using WingsEmu.Game._ItemUsage.Configuration;
using WingsEmu.Game.Characters;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Groups;
using WingsEmu.Game.Managers;
using WingsEmu.Game.Mates;
using WingsEmu.Game.Networking;

namespace WingsAPI.Game.Extensions.Groups
{
    public static class GroupPacketExtensions
    {
        public static List<string> GeneratePartyUiPackets(this IClientSession session)
        {
            var str = new List<string>();

            IReadOnlyList<IPlayerEntity> groupMembers = session.PlayerEntity.GetGroup().Members;

            // entwell mixing pets slots & group slots :/
            int groupSlotPetOffset = 3;

            for (int i = 0; i < groupMembers.Count; i++)
            {
                IPlayerEntity member = groupMembers[i];

                if (member.Id == session.PlayerEntity.Id)
                {
                    continue;
                }

                byte groupSlot = (byte)(i + groupSlotPetOffset);
                str.Add(
                    $"pst 1 {member.Id} {groupSlot.ToString()} {member.GetHpPercentage()} {member.GetMpPercentage()} {member.Hp} {member.Mp} {(byte)member.Class} {(byte)member.Gender} {(member.UseSp ? member.Morph : 0)}{member.BuffComponent.GetAllBuffs().Aggregate(string.Empty, (current, buff) => current + $" {buff.CardId}.{buff.CasterLevel}")}");
            }

            return str;
        }

        public static void RefreshPartyUi(this IClientSession session)
        {
            if (!session.PlayerEntity.IsInGroup())
            {
                return;
            }

            session.SendPackets(session.GeneratePartyUiPackets());
        }

        public static byte GetGroupSlotIndex(this IClientSession session)
        {
            if (!session.PlayerEntity.IsInGroup())
            {
                return 0;
            }

            // offset of mates + 1 (players start at 0 in their group index)
            byte groupSlot = 3;
            IReadOnlyList<IPlayerEntity> groupMembers = session.PlayerEntity.GetGroup().Members;
            for (int i = 0; i < groupMembers.Count; i++)
            {
                IPlayerEntity member = groupMembers[i];

                if (member.Id != session.PlayerEntity.Id)
                {
                    continue;
                }

                groupSlot = (byte)(i + 3);
            }

            return groupSlot;
        }

        public static string GeneratePInitPacket(this IClientSession session, IGameLanguageService gameLanguage, ISpPartnerConfiguration spPartnerConfiguration)
        {
            IReadOnlyList<IMateEntity> mates = session.PlayerEntity.MateComponent.GetMates();
            int allyCount = 0;

            string str = string.Empty;
            int groupId = session.PlayerEntity.IsInGroup() ? session.PlayerEntity.GetGroup().GroupId : -1;
            if (mates != null)
            {
                // FUCK GF I18N SOOOOO MUCH ༼ つ ◕_◕ ༽つ
                foreach (IMateEntity mate in mates.Where(s => s.IsTeamMember).OrderBy(s => s.MateType))
                {
                    if (mate.IsTemporaryPet)
                    {
                        continue;
                    }

                    if (mate.Specialist != null && mate.IsUsingSp)
                    {
                        allyCount++;

                        GameDialogKey? key = Enum.TryParse(spPartnerConfiguration.GetByMorph(mate.Specialist.GameItem.Morph)?.Name, out GameDialogKey gameDialogKey) ? gameDialogKey : null;
                        string specialistName = key.HasValue
                            ? gameLanguage.GetLanguage(key.Value, session.UserLanguage)
                            : gameLanguage.GetLanguage(GameDataType.NpcMonster, mate.Name, session.UserLanguage);
                        int specialistMorph = mate.Specialist.GameItem.Morph;
                        str += $" 2|{mate.Id}|{(int)mate.MateType}|{mate.Level}|{specialistName.Replace(' ', '^')}|{groupId}|{specialistMorph}|1|0|0|0";
                        continue;
                    }

                    allyCount++;
                    string mateName = string.IsNullOrEmpty(mate.MateName) || mate.Name == mate.MateName
                        ? gameLanguage.GetLanguage(GameDataType.NpcMonster, mate.Name, session.UserLanguage)
                        : mate.MateName;
                    int morph = mate.MonsterVNum;

                    str += $" 2|{mate.Id}|{(int)mate.MateType}|{mate.Level}|{mateName.Replace(' ', '^')}|{groupId}|{morph}|0|0|0|0";
                }
            }

            if (!session.PlayerEntity.IsInGroup())
            {
                return $"pinit {allyCount}{str}";
            }

            PlayerGroup group = session.PlayerEntity.GetGroup();
            IReadOnlyList<IPlayerEntity> grpMembers = group.Members;
            foreach (IPlayerEntity member in grpMembers)
            {
                allyCount++;
                str +=
                    $" 1|{member.Id}|{member.Session.GetGroupSlotIndex()}|{member.Level}|{member.Name}|{group.GroupId}|{(byte)member.Gender}|{(byte)member.Class}|{(member.UseSp ? member.Morph : 0)}|{member.HeroLevel}|0|0|0";
            }

            return $"pinit {allyCount}{str}";
        }

        public static void RefreshParty(this IClientSession session, ISpPartnerConfiguration partnerConfiguration)
            => session.SendPacket(session.GeneratePInitPacket(StaticGameLanguageService.Instance, partnerConfiguration));

        public static string GeneratePidx(this IClientSession session)
        {
            const string header = "pidx";
            string packet;

            if (!session.PlayerEntity.IsInGroup())
            {
                packet = $" -1 1.{session.PlayerEntity.Id}";
                return header + packet;
            }

            PlayerGroup grp = session.PlayerEntity.GetGroup();
            packet = $" {grp.GroupId}";

            foreach (IPlayerEntity member in grp.Members)
            {
                packet += $" 1.{member.Id}";
            }

            return header + packet;
        }

        public static void BroadcastPidx(this IClientSession session) => session.Broadcast(session.GeneratePidx());

        public static void RefreshGroupLevelUi(this IClientSession session, ISpPartnerConfiguration spPartnerConfiguration)
        {
            if (!session.PlayerEntity.IsInGroup())
            {
                return;
            }

            foreach (IPlayerEntity member in session.PlayerEntity.GetGroup().Members)
            {
                member.Session.RefreshParty(spPartnerConfiguration);
            }
        }

        public static bool IsMemberWith(this IPlayerEntity character, long id, ISessionManager sessionManager)
        {
            if (!character.IsInGroup())
            {
                return false;
            }

            IClientSession target = sessionManager.GetSessionByCharacterId(id);

            if (target == null)
            {
                return false;
            }

            if (!target.PlayerEntity.IsInGroup())
            {
                return false;
            }

            PlayerGroup characterGroup = character.GetGroup();
            PlayerGroup targetGroup = target.PlayerEntity.GetGroup();

            return characterGroup.GroupId == targetGroup.GroupId;
        }
    }
}