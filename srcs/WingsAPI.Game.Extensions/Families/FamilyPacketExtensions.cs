using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using PhoenixLib.MultiLanguage;
using WingsAPI.Data.Families;
using WingsEmu.Core;
using WingsEmu.Game._i18n;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Families;
using WingsEmu.Game.Families.Configuration;
using WingsEmu.Game.Items;
using WingsEmu.Game.Managers;
using WingsEmu.Game.Managers.StaticData;
using WingsEmu.Game.Networking;
using WingsEmu.Packets.Enums;
using WingsEmu.Packets.Enums.Chat;
using WingsEmu.Packets.Enums.Families;

namespace WingsAPI.Game.Extensions.Families
{
    public static class FamilyPacketExtensions
    {
        #region Send Packets

        public static void SendFmpPacket(this IFamily family, ISessionManager sessionManager, IItemsManager itemsManager, IFamilyManager familyManager, IRankingManager rankingManager, FamilyConfiguration familyConfiguration)
        {
            if (family == null)
            {
                return;
            }

            string packet = family.GenerateFmpPacket(itemsManager, familyManager, rankingManager, familyConfiguration);
            
            foreach (FamilyMembership member in family.Members)
            {
                IClientSession session = sessionManager.GetSessionByCharacterId(member.CharacterId);
                session?.SendPacket(packet);
            }
        }


        public static void SendFmpPacket(this IClientSession session, IItemsManager itemsManager, IFamilyManager familyManager, IRankingManager rankingManager, FamilyConfiguration familyConfiguration)
        {
            IFamily family = session.PlayerEntity.Family;
            if (family == null)
            {
                return;
            }

            session.SendPacket(family.GenerateFmpPacket(itemsManager, familyManager, rankingManager, familyConfiguration));
        }

        public static string GenerateFmpPacket(this IFamily family, IItemsManager itemsManager, IFamilyManager familyManager, IRankingManager rankingManager, FamilyConfiguration familyConfiguration)
        {
            var stringBuilder = new StringBuilder("fmp");

            Dictionary<FamilyUpgradeType, (byte level, FamilyUpgrade familyUpgrade)> toShow = new();

            if (family?.Upgrades == null)
            {
                return stringBuilder.ToString();
            }

            foreach ((int upgradeId, FamilyUpgrade upgrade) in family.Upgrades)
            {
                IGameItem item = itemsManager.GetItem(upgradeId);
                if (item == null)
                {
                    continue;
                }

                // Get the highest upgradeLevel by FamilyUpgradeType
                FamilyUpgradeType type = Enum.Parse<FamilyUpgradeType>(item.Data[2].ToString());
                byte upgradeLevel = (byte)item.Data[3];

                if (!toShow.TryGetValue(type, out (byte level, FamilyUpgrade familyUpgrade) familyUpgrade))
                {
                    familyUpgrade = (upgradeLevel, upgrade);
                    toShow[type] = familyUpgrade;
                    continue;
                }

                if (upgradeLevel <= familyUpgrade.level)
                {
                    continue;
                }

                familyUpgrade = (upgradeLevel, upgrade);
                toShow[type] = familyUpgrade;
            }

            foreach ((FamilyUpgradeType _, (byte _, FamilyUpgrade familyUpgrade)) in toShow)
            {
                stringBuilder.AppendFormat(" {0}|{1}", familyUpgrade.Id, (byte)familyUpgrade.State);
            }

            foreach (FamilyBuffCrossChannel familyBuffCrossChannel in familyManager.CurrentFamilyBuffs)
            {
                if (family.Id != familyBuffCrossChannel.FamilyId)
                {
                    continue;
                }
                
                stringBuilder.AppendFormat(" {0}|{1}", familyBuffCrossChannel.ItemVnum, (byte)FamilyUpgradeState.PASSIVE);
            }
            
            long? monthlyExpFamilyId = rankingManager.MonthlyExpFamilyId;
            long? monthlyPvpAngelFamilyId = rankingManager.MonthlyPvpAngelFamilyId;
            long? monthlyPvpDemonFamilyId = rankingManager.MonthlyPvpDemonFamilyId;
            long? monthlyRainbowBattleFamilyId = rankingManager.MonthlyRainbowBattleFamilyId;
            
            bool canUse = family.Id == monthlyExpFamilyId || family.Id == monthlyPvpAngelFamilyId || family.Id == monthlyPvpDemonFamilyId || family.Id == monthlyRainbowBattleFamilyId;

            if (!canUse)
            {
                return stringBuilder.ToString();
            }
            
            foreach (FamilyBuff familyConfigurationFamilyBuff in familyConfiguration.FamilyBuffs)
            {
                var familyFaction = (FactionType)family.Faction;
                
                // Buff already in use
                if (familyManager.CurrentFamilyBuffs.Any(x => x.BuffVnum == familyConfigurationFamilyBuff.BuffVnum && (x.FactionType == null || x.FactionType == familyFaction)))
                {
                    continue;
                }
                
                bool canFactionBuffBeUsed = familyManager.CanUseFamilyBuffAsync(familyConfigurationFamilyBuff.ItemVnum, family.Id).ConfigureAwait(false).GetAwaiter().GetResult();
                stringBuilder.AppendFormat(" {0}|{1}", familyConfigurationFamilyBuff.ItemVnum, canFactionBuffBeUsed ? (byte)FamilyUpgradeState.ACTIVATABLE : (byte)FamilyUpgradeState.ALREADY_USED_TODAY);
            }

            return stringBuilder.ToString();
        }

        public static void SendFmiPacket(this IFamily family, ISessionManager sessionManager)
        {
            if (family == null)
            {
                return;
            }

            string packet = family.GenerateFmiPacket();

            foreach (FamilyMembership member in family.Members)
            {
                IClientSession session = sessionManager.GetSessionByCharacterId(member.CharacterId);

                session?.SendPacket(packet);
            }
        }

        public static void SendFmiPacket(this IClientSession session)
        {
            IFamily family = session.PlayerEntity.Family;
            if (family == null)
            {
                return;
            }

            session.SendPacket(family.GenerateFmiPacket());
        }

        public static string GenerateFmiPacket(this IFamily family)
        {
            var stringBuilder = new StringBuilder("fmi");

            // iterate over mission config & check level restrictions
            DateTime resetDate = DateTime.UtcNow.Date;
            foreach ((int missionId, FamilyMissionDto upgrade) in family.Mission)
            {
                // progressType => 1 = Completed
                // progressType => 2 = InProgress
                int progressType = upgrade.CompletionDate.HasValue && upgrade.CompletionDate > resetDate ? 1 : 2;
                string thirdArgument = progressType == 1 && upgrade.CompletionDate.HasValue ? upgrade.CompletionDate.Value.ToString("yyyyMMdd") : upgrade.Count.ToString();
                stringBuilder.AppendFormat(" 0|{0}|{1}|{2}|{3}", missionId, progressType, thirdArgument, upgrade.CompletionCount);
            }

            foreach ((int achievementId, FamilyAchievementProgressDto upgrade) in family.AchievementProgress)
            {
                stringBuilder.AppendFormat(" 1|{0}|2|{1}|0", achievementId, upgrade.Count);
            }

            foreach ((int achievementId, FamilyAchievementCompletionDto upgrade) in family.Achievements)
            {
                stringBuilder.AppendFormat(" 1|{0}|1|{1:yyyyMMdd}|0", achievementId, upgrade.CompletionDate);
            }

            return stringBuilder.ToString();
        }

        public static void SendFamilyLevelUpMessageToMembers(Family family, ISessionManager sessionManager, IGameLanguageService languageService, FamilyConfiguration familyConfiguration)
        {
            if (family == null)
            {
                return;
            }

            foreach (FamilyMembership member in family.Members)
            {
                IClientSession session = sessionManager.GetSessionByCharacterId(member.CharacterId);
                if (session == null)
                {
                    continue;
                }

                session.RefreshFamilyInfo(family, familyConfiguration);
                session.BroadcastGidx(family, languageService);
                session.SendMsg(languageService.GetLanguageFormat(GameDialogKey.FAMILY_SHOUTMESSAGE_LEVEL_UP, session.UserLanguage, family.Level.ToString()), MsgMessageType.Middle);
            }
        }

        public static void SendFamilyNoticeMessage(Family family, ISessionManager sessionManager, FamilyConfiguration familyConfiguration)
        {
            if (family == null)
            {
                return;
            }

            string message = family.Message;
            if (string.IsNullOrEmpty(message))
            {
                return;
            }

            foreach (FamilyMembership member in family.Members)
            {
                IClientSession session = sessionManager.GetSessionByCharacterId(member.CharacterId);
                session?.SendInfo("--- Family Message ---\n" + message);
                session?.RefreshFamilyInfo(family, familyConfiguration);
            }
        }

        public static void SendFamilyInfoToMembers(Family family, ISessionManager sessionManager, FamilyConfiguration familyConfiguration)
        {
            if (family == null)
            {
                return;
            }

            foreach (FamilyMembership member in family.Members.ToArray())
            {
                IClientSession session = sessionManager.GetSessionByCharacterId(member.CharacterId);
                session?.RefreshFamilyInfo(family, familyConfiguration);
            }
        }

        public static void SendFamilyMembersAuthorityToMembers(Family family, ISessionManager sessionManager, FamilyConfiguration familyConfiguration)
        {
            if (family == null)
            {
                return;
            }

            IReadOnlyList<string> packets = null;

            foreach (FamilyMembership member in family.Members)
            {
                IClientSession session = sessionManager.GetSessionByCharacterId(member.CharacterId);

                if (session == null)
                {
                    continue;
                }

                packets ??= new List<string>
                {
                    GenerateFamilyMembersPacket(sessionManager, family),
                    GenerateFamilyMembersMessagesPacket(family)
                };

                session.RefreshFamilyInfo(family, familyConfiguration);
                session.SendPackets(packets);
            }
        }

        public static void SendFamilyMembersInfoToMembers(IFamily family, ISessionManager sessionManager, FamilyConfiguration familyConfiguration)
        {
            if (family == null)
            {
                return;
            }

            IReadOnlyList<string> packets = null;

            foreach (FamilyMembership member in family.Members.ToArray())
            {
                IClientSession session = sessionManager.GetSessionByCharacterId(member.CharacterId);

                if (session == null)
                {
                    continue;
                }

                packets ??= new List<string>
                {
                    GenerateFamilyMembersPacket(sessionManager, family),
                    GenerateFamilyMembersMessagesPacket(family),
                    GenerateFamilyMembersExpPacket(family)
                };

                session.RefreshFamilyInfo(family, familyConfiguration);
                session.SendPackets(packets);
            }
        }

        public static void SendOnlineStatusToMembers(this IFamily family, ISessionManager sessionManager, long characterId, bool connecting, IGameLanguageService gameLanguage)
        {
            string packet = null;

            FamilyMembership familyMember = family.Members.ToArray().FirstOrDefault(x => x.CharacterId == characterId);

            if (familyMember == null)
            {
                return;
            }

            foreach (FamilyMembership member in family.Members.ToArray())
            {
                IClientSession session = sessionManager.GetSessionByCharacterId(member.CharacterId);

                if (session == null)
                {
                    continue;
                }

                packet ??= GenerateFamilyMemberConnectionStatus(characterId, connecting);
                session.SendPacket(packet);

                if (!connecting)
                {
                    continue;
                }

                string authority = gameLanguage.GetLanguage(familyMember.Authority.GetMemberLanguageKey(), session.UserLanguage);
                session.SendInformationChatMessage(gameLanguage.GetLanguageFormat(GameDialogKey.FAMILY_CHATMESSAGE_CHARACTER_LOGGED_IN, session.UserLanguage, familyMember.Character.Name, authority));
            }
        }

        public static void SendMembersExpToMembers(IFamily family, ISessionManager sessionManager)
        {
            if (family == null)
            {
                return;
            }

            string packet = null;

            foreach (FamilyMembership member in family.Members)
            {
                IClientSession session = sessionManager.GetSessionByCharacterId(member.CharacterId);

                if (session == null)
                {
                    continue;
                }

                packet ??= GenerateFamilyMembersExpPacket(family);
                session.SendPacket(packet);
            }
        }

        public static void SendMembersDailyMessages(IFamily family, ISessionManager sessionManager)
        {
            if (family == null)
            {
                return;
            }

            string packet = null;

            foreach (FamilyMembership member in family.Members)
            {
                IClientSession session = sessionManager.GetSessionByCharacterId(member.CharacterId);

                if (session == null)
                {
                    continue;
                }

                packet ??= GenerateFamilyMembersMessagesPacket(family);
                session.SendPacket(packet);
            }
        }

        public static void SendFamilyLogsToMembers(IFamily family, ISessionManager sessionManager)
        {
            if (family == null)
            {
                return;
            }

            string packet = GenerateFamilyHistoryUpdate();

            foreach (FamilyMembership member in family.Members)
            {
                IClientSession session = sessionManager.GetSessionByCharacterId(member.CharacterId);
                session?.SendPacket(packet);
            }
        }

        public static void SendFamilyLogsToMember(this IClientSession session, IFamily family)
        {
            session.SendPackets(GenerateGhisPackets(family));
        }

        public static void SendResetFamilyInterface(this IClientSession session)
        {
            session.SendPacket(GenerateRestartFamilyInterface());
        }

        public static void RefreshFamilyMembers(this IClientSession session, ISessionManager sessionManager, IFamily family)
        {
            if (family == null)
            {
                session.SendEmptyFamilyMembers();
                return;
            }

            session.SendPacket(GenerateFamilyMembersPacket(sessionManager, family));
        }


        public static void RefreshFamilyMembersMessages(this IClientSession session, IFamily family)
        {
            if (family == null)
            {
                session.SendEmptyFamilyMessages();
            }

            session.SendPacket(GenerateFamilyMembersMessagesPacket(family));
        }


        public static void RefreshFamilyMembersExp(this IClientSession session, IFamily family)
        {
            if (family == null)
            {
                session.SendEmptyFamilyExp();
                return;
            }

            session.SendPacket(GenerateFamilyMembersExpPacket(family));
        }

        public static void RefreshFamilyInfo(this IClientSession session, IFamily family, FamilyConfiguration familyConfiguration)
        {
            if (family == null)
            {
                session.SendEmptyFamilyInfo();
                return;
            }

            session.SendPacket(session.GenerateFamilyInfoPacket(family, familyConfiguration));
        }

        public static void BroadcastGidx(this IClientSession session, IFamily family, IGameLanguageService gameLanguageService)
            => session.CurrentMapInstance?.Broadcast(x => session.GenerateGidxPacket(gameLanguageService, family, x.UserLanguage));

        public static void SendGidxPacket(this IClientSession session, IFamily family, IGameLanguageService gameLanguage) =>
            session.SendPacket(session.GenerateGidxPacket(gameLanguage, family, session.UserLanguage));

        public static void SendTargetGidxPacket(this IClientSession session, IClientSession target, IFamily family, IGameLanguageService gameLanguageService)
            => session.SendPacket(target.GenerateGidxPacket(gameLanguageService, family, session.UserLanguage));

        public static void SendEmptyFamilyInfo(this IClientSession session) => session.SendPacket("ginfo");

        public static void SendEmptyFamilyMembers(this IClientSession session) => session.SendPacket("gmbr 0");

        public static void SendEmptyFamilyMessages(this IClientSession session) => session.SendPacket("gmsg");

        public static void SendEmptyFamilyExp(this IClientSession session) => session.SendPacket("gexp");

        #endregion

        #region Generate Packets

        public static string GenerateGidxPacket(this IClientSession session, IGameLanguageService gameLanguage, IFamily family, RegionLanguageType type)
        {
            IRankingManager rankingManager = StaticRankingManager.Instance;
            bool isFirstPlace = rankingManager.MonthlyExpFamilyId.HasValue && rankingManager.MonthlyExpFamilyId.Value == family?.Id;
            bool isAngelFirstPlace = rankingManager.MonthlyPvpAngelFamilyId.HasValue && rankingManager.MonthlyPvpAngelFamilyId.Value == family?.Id;
            bool isDemonFirstPlace = rankingManager.MonthlyPvpDemonFamilyId.HasValue && rankingManager.MonthlyPvpDemonFamilyId.Value == family?.Id;
            bool isRainbowBattlePlace = rankingManager.MonthlyRainbowBattleFamilyId.HasValue && rankingManager.MonthlyRainbowBattleFamilyId.Value == family?.Id;
            
            return family == null
                ? $"gidx 1 {session.PlayerEntity.Id} -1 - 0 0|0|0"
                : $"gidx 1 {session.PlayerEntity.Id} {family.Id}.-1 {family.Name}({gameLanguage.GetLanguage(session.PlayerEntity.GetFamilyAuthority().GetMemberLanguageKey(), type)}) {family.Level} {(isFirstPlace ? 1 : 0)}|{(isRainbowBattlePlace ? 1 : 0)}|{(isAngelFirstPlace ? 1 : isDemonFirstPlace ? 2 : 0)}";
        }

        public static IReadOnlyCollection<string> GenerateGhisPackets(IFamily family)
        {
            var list = new List<string>();
            int index = 0;
            DateTime currentTime = DateTime.UtcNow;
            list.Add(GenerateGhisPacket(true, currentTime, family.Logs, ref index));

            while (index < family.Logs.Count)
            {
                list.Add(GenerateGhisPacket(false, currentTime, family.Logs, ref index));
            }

            return list;
        }

        public static string GenerateGhisPacket(bool initial, DateTime currentTime, IReadOnlyList<FamilyLogDto> logs, ref int index)
        {
            int initialIndexValue = index;
            var packet = new StringBuilder("ghis");

            if (initial)
            {
                packet.Append(" 0");
            }

            for (; index < logs.Count && index < initialIndexValue + 55; index++)
            {
                FamilyLogDto log = logs[index];
                packet.Append(GenerateGhisSubPacket(log, currentTime));
            }

            return packet.ToString();
        }

        public static string GenerateGhisSubPacket(FamilyLogDto log, DateTime currentTime)
        {
            string packet = $" {(byte)log.FamilyLogType}|";

            switch (log.FamilyLogType)
            {
                case FamilyLogType.DailyMessage:
                    packet += $"{log.Actor}|{(log.Argument1 == null ? string.Empty : log.Argument1.Replace(' ', (char)0xB))}"; // memberName|message - Quarry|Hey
                    break;
                case FamilyLogType.RaidWon:
                    packet += $"{log.Actor}"; // Act4RaidType
                    break;
                case FamilyLogType.RainbowBattle:
                    packet += $"{log.Actor}|-"; // EnemyFamilyName - not used anymore because of the event rework
                    break;
                case FamilyLogType.FamilyXP:
                    packet += $"{log.Actor}|{log.Argument1 ?? string.Empty}"; // memberName|memberXp - Quarry|50000
                    break;
                case FamilyLogType.FamilyLevelUp:
                    packet += $"{log.Actor}"; // FamilyLevel
                    break;
                case FamilyLogType.LevelUp:
                    packet += $"{log.Actor}|{log.Argument1 ?? string.Empty}"; // memberName|memberLevel - Quarry|60
                    break;
                case FamilyLogType.ItemUpgraded:
                    packet += $"{log.Actor}|{log.Argument1 ?? string.Empty}|{log.Argument2 ?? string.Empty}"; // memberName|itemVnum|itemUpgrade - Quarry|1|10
                    break;
                case FamilyLogType.RightChanged:
                    packet +=
                        $"{log.Actor}|{log.Argument1 ?? string.Empty}|{log.Argument2 ?? string.Empty}|{log.Argument3 ?? string.Empty}"; // memberName|FamilyAuthorityType|FamilyActionType|value - Quarry|2|1|1
                    break;
                case FamilyLogType.AuthorityChanged:
                    packet += $"{log.Actor}|{log.Argument1 ?? string.Empty}|{log.Argument2 ?? string.Empty}"; // memberName|FamilyAuthorityType|targetMemberName - Blowa|2|Quarry
                    break;
                case FamilyLogType.MemberLeave:
                    packet += $"{log.Actor}"; // memberName
                    break;
                case FamilyLogType.MemberJoin:
                    packet += $"{log.Actor}|{log.Argument1 ?? string.Empty}"; // memberName|targetMemberName - Blowa|Quarry
                    break;
                case FamilyLogType.FamilyMission:
                    packet += $"{log.Actor}"; // FamilyMissionItemVnum
                    break;
                case FamilyLogType.FamilyAchievement:
                    packet += $"{log.Actor}"; // FamilyAchievementItemVnum
                    break;
                case FamilyLogType.HeroLevelUp:
                    packet += $"{log.Actor}|{log.Argument1 ?? string.Empty}"; // memberName|HeroLevel - Quarry|60
                    break;
                case FamilyLogType.MemberUsedItem:
                    packet += $"{log.Actor}|{log.Argument1 ?? string.Empty}"; // memberName|ItemVnum - Quarry|5060 - used only for FXP Boosters
                    break;
            }

            return $"{packet}|{Math.Floor((currentTime - log.Timestamp).TotalHours).ToString(CultureInfo.InvariantCulture)}";
        }

        public static string GenerateFamilyMemberConnectionStatus(long characterId, bool connecting) => $"gcon {characterId.ToString()}|{(connecting ? 1 : 0).ToString()}";

        public static string GenerateRestartFamilyInterface() => "fclear";

        public static string GenerateFamilyMembersPacket(ISessionManager sessionManager, IFamily family)
        {
            var packet = new StringBuilder("gmbr 0");
            DateTime actualTime = DateTime.UtcNow;
            foreach (FamilyMembership member in family.Members.OrderBy(x => x.JoinDate))
            {
                bool isOnline = sessionManager.IsOnline(member.CharacterId);
                packet.Append($" {member.CharacterId.ToString()}|{member.JoinDate:yyMMddHH)}|{member.Character.Name}|{member.Character.Level.ToString()}|{((byte)member.Character.Class).ToString()}" +
                    $"|{((byte)member.Authority).ToString()}|{((byte)member.Title).ToString()}|{(isOnline ? 1 : 0).ToString()}|{member.Character.HeroLevel.ToString()}" +
                    $"|{(isOnline ? -1 : Math.Floor((actualTime - member.LastOnlineDate).TotalHours)).ToString(CultureInfo.InvariantCulture)}");
            }

            return packet.ToString();
        }

        public static string GenerateFamilyMembersMessagesPacket(IFamily family)
        {
            var packet = new StringBuilder("gmsg");
            foreach (FamilyMembership member in family.Members)
            {
                if (string.IsNullOrEmpty(member.DailyMessage))
                {
                    continue;
                }

                packet.Append($" {member.CharacterId.ToString()}|{member.DailyMessage.Replace(" ", "")}");
            }

            return packet.ToString();
        }

        public static string GenerateFamilyMembersExpPacket(IFamily family)
        {
            var packet = new StringBuilder("gexp");
            foreach (FamilyMembership member in family.Members.ToArray())
            {
                packet.Append($" {member.CharacterId.ToString()}|{member.Experience.ToString()}");
            }

            return packet.ToString();
        }

        public static string GenerateFamilyInfoPacket(this IClientSession session, IFamily family, FamilyConfiguration familyConfiguration)
        {
            if (family?.Head == null || session.PlayerEntity.Family == null)
            {
                return "ginfo";
            }

            Range<long> levelInfo = familyConfiguration.GetRangeByFamilyXp(family.Experience);
            bool hasLevelInfo = levelInfo != null;
            long refinedMinXp = hasLevelInfo ? family.Experience - levelInfo.Minimum : 0;
            long refinedMaxXp = hasLevelInfo ? levelInfo.Maximum - levelInfo.Minimum : 0;

            return
                "ginfo " +
                $"{family.Name} " +
                $"{family.Head.Character.Name} " +
                $"{((byte)family.HeadGender).ToString()} " +
                $"{family.Level.ToString()} " +
                $"{refinedMinXp.ToString()} " +
                $"{refinedMaxXp.ToString()} " +
                $"{family.Members.Count.ToString()} " +
                $"{family.GetMaximumMembershipCapacity().ToString()} " +
                $"{((byte)session.PlayerEntity.GetFamilyAuthority()).ToString()} " +
                $"{(family.AssistantCanInvite ? 1 : 0).ToString()} " +
                $"{(family.AssistantCanNotice ? 1 : 0).ToString()} " +
                $"{(family.AssistantCanShout ? 1 : 0).ToString()} " +
                $"{(family.AssistantCanGetHistory ? 1 : 0).ToString()} " +
                $"{((byte)family.AssistantWarehouseAuthorityType).ToString()} " +
                $"{(family.MemberCanGetHistory ? 1 : 0).ToString()} " +
                $"{((byte)family.MemberWarehouseAuthorityType).ToString()} " +
                $"{family.Message?.Replace(' ', '^') ?? string.Empty}";
        }

        public static string GenerateFamilyHistoryUpdate() => "fhis_stc";

        #endregion
    }
}