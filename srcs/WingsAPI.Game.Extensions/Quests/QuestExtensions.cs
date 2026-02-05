using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PhoenixLib.Logging;
using WingsEmu.DTOs.Quests;
using WingsEmu.Game._enum;
using WingsEmu.Game.Algorithm;
using WingsEmu.Game.Characters;
using WingsEmu.Game.Configurations;
using WingsEmu.Game.Entities;
using WingsEmu.Game.Helpers.Damages;
using WingsEmu.Game.Networking;
using WingsEmu.Game.Quests;
using WingsEmu.Packets.Enums;
using WingsEmu.Packets.Enums.Character;

namespace WingsAPI.Game.Extensions.Quests
{
    public static class QuestExtensions
    {
        private static readonly string _b64SqstIndex = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz{}";


        private static readonly QuestType[] DialogQuestTypes =
        {
            QuestType.DIALOG,
            QuestType.DIALOG_2,
            QuestType.DELIVER_ITEM_TO_NPC,
            QuestType.GIVE_ITEM_TO_NPC,
            QuestType.GIVE_ITEM_TO_NPC_2,
            QuestType.GIVE_NPC_GOLD,
            QuestType.DIALOG_WHILE_WEARING,
            QuestType.DIALOG_WHILE_HAVING_ITEM
        };

        public static string GenerateTargetQuest(this IClientSession session, short x, short y, short mapId, int questId) => $"target {x} {y} {mapId} {questId}";
        public static string GenerateTargetOffQuest(this IClientSession session, short x, short y, short mapId, int questId) => $"targetoff {x} {y} {mapId} {questId}";

        public static string GenerateQnpcPacket(this IClientSession session, short level, short npcVnum, short mapId) =>
            $"qnpc {npcVnum}|{mapId}|{level} -1|-1|-1 -1|-1|-1 -1|-1|-1 -1|-1|-1 -1|-1|-1 -1|-1|-1 -1|-1|-1 -1|-1|-1 -1|-1|-1";

        public static string GenerateQrPacket(this IClientSession session, CharacterQuest quest, IReadOnlyCollection<CharacterQuestGeneratedReward> rndRewards, QuestsRatesConfiguration questRates)
        {
            List<QuestPrizeDto> prizes = quest.Quest.Prizes;
            if (prizes == null)
            {
                return "";
            }

            var qrPacket = new StringBuilder("qr ");
            foreach (QuestPrizeDto prize in prizes)
            {
                switch (prize.RewardType)
                {
                    case (byte)QuestRewardType.Reput:
                    case (byte)QuestRewardType.Exp:
                    case (byte)QuestRewardType.SecondExp:
                    case (byte)QuestRewardType.ThirdExp:
                    case (byte)QuestRewardType.JobExp:
                    case (byte)QuestRewardType.SecondJobExp:
                    case (byte)QuestRewardType.HeroExpPercentage:
                    case (byte)QuestRewardType.SetJobLevel:
                    case (byte)QuestRewardType.SetLevel:
                    case (byte)QuestRewardType.SetLevelAndJobLevel:
                        qrPacket.Append($"{prize.RewardType.ToString()} 0 {prize.Data0.ToString()} ");
                        break;
                    case (byte)QuestRewardType.Gold:
                        qrPacket.Append($"{prize.RewardType.ToString()} 0 {prize.Data0 * questRates.GoldRate} ");
                        break;
                    case (byte)QuestRewardType.AllRewards:
                        qrPacket.Append(prize.Data0 == -1 ? "" : $"{prize.RewardType.ToString()} {prize.Data0.ToString()} 1 ");
                        qrPacket.Append(prize.Data1 == -1 ? "" : $"{prize.RewardType.ToString()} {prize.Data1.ToString()} 1 ");
                        qrPacket.Append(prize.Data2 == -1 ? "" : $"{prize.RewardType.ToString()} {prize.Data2.ToString()} 1 ");
                        qrPacket.Append(prize.Data3 == -1 ? "" : $"{prize.RewardType.ToString()} {prize.Data3.ToString()} 1 ");
                        break;
                    case (byte)QuestRewardType.SecondGold:
                        qrPacket.Append($"{prize.RewardType.ToString()} 0 {prize.Data0 * questRates.BaseGold * questRates.GoldRate} ");
                        break;
                    case (byte)QuestRewardType.ThirdGold:
                        qrPacket.Append($"{prize.RewardType.ToString()} 0 {prize.Data0 * quest.ObjectiveAmount.Sum(s => s.Value.RequiredAmount) * session.PlayerEntity.Level * questRates.GoldRate} ");
                        break;
                    case (byte)QuestRewardType.Unknow:
                    case (byte)QuestRewardType.ItemsDependingOnClass:
                        switch (session.PlayerEntity.Class)
                        {
                            case ClassType.Swordman:
                                qrPacket.Append(prize.Data0 == -1 ? "" : $"{prize.RewardType.ToString()} {prize.Data0.ToString()} {prize.Data4.ToString()} ");
                                break;
                            case ClassType.Archer:
                                qrPacket.Append(prize.Data1 == -1 ? "" : $"{prize.RewardType.ToString()} {prize.Data1.ToString()} {prize.Data4.ToString()} ");
                                break;
                            case ClassType.Magician:
                                qrPacket.Append(prize.Data2 == -1 ? "" : $"{prize.RewardType.ToString()} {prize.Data2.ToString()} {prize.Data4.ToString()} ");
                                break;
                            default:
                                qrPacket.Append(prize.Data3 == -1 ? "" : $"{prize.RewardType.ToString()} {prize.Data3.ToString()} {prize.Data4.ToString()} ");
                                break;
                        }

                        break;
                }
            }

            foreach (CharacterQuestGeneratedReward rndReward in rndRewards)
            {
                qrPacket.Append($"{(byte)QuestRewardType.RandomReward} {rndReward.ItemVnum} {rndReward.Amount} ");
            }

            for (int i = prizes.Count(s => s.RewardType != (byte)QuestRewardType.RandomReward) + rndRewards.Count; i < 4; i++)
            {
                qrPacket.Append("0 0 0 ");
            }

            qrPacket.Append(quest.QuestId);
            return qrPacket.ToString();
        }

        public static string GenerateQuestList(this IClientSession session, IQuestManager questManager, int? questToShowInfo = null)
        {
            IEnumerable<CharacterQuest> quests = session.PlayerEntity.GetCurrentQuests();
            string header = "qstlist";
            if (quests == null)
            {
                return header;
            }

            var packet = new StringBuilder();
            foreach (CharacterQuest quest in quests)
            {
                packet.Append($" {session.GenerateQuestData(quest, questManager, qstlistCall: true, questToShowInfo: questToShowInfo)}");
            }

            int soundFlowerEmptySlot = session.GetEmptyQuestSlot(QuestSlotType.GENERAL, true);
            for (int i = 0; i < session.PlayerEntity.GetPendingSoundFlowerQuests(); i++)
            {
                packet.Append($" {soundFlowerEmptySlot + i}.0.0.0.0.0.0.0.0.0.0.0.0.0.0.0.0");
            }

            return $"{header}{packet}";
        }

        public static string GenerateQuestProgress(this IClientSession session, IQuestManager questManager, int questVnum)
        {
            CharacterQuest currentQuest = session.PlayerEntity.GetCurrentQuest(questVnum);
            return currentQuest == null ? string.Empty : $"qsti {session.GenerateQuestData(currentQuest, questManager, session.PlayerEntity.IsQuestCompleted(currentQuest))}";
        }

        private static string GenerateQuestData(this IClientSession session, CharacterQuest characterQuest, IQuestManager questManager, bool isFinished = false, bool qstlistCall = false,
            int? questToShowInfo = null)
        {
            int questSlot = session.GetQuestSlot(characterQuest);
            if (questSlot == -1)
            {
                Log.Debug($"The slot for quest {characterQuest.QuestId} was not found.");
                return "";
            }

            IReadOnlyList<QuestObjectiveDto> objectives;
            bool statusAdded;
            var data = new StringBuilder($"{questSlot}.{characterQuest.QuestId}");

            switch (characterQuest.SlotType)
            {
                case QuestSlotType.MAIN:
                    data.Append($".{characterQuest.QuestId}");
                    break;
                case QuestSlotType.GENERAL:
                    IReadOnlyCollection<int> questlines = questManager.GetQuestlines(characterQuest.QuestId);
                    data.Append($".{(questlines.Any() ? questlines.First() : characterQuest.QuestId)}");
                    break;
                case QuestSlotType.SECONDARY:
                    data.Append(".0");
                    break;
            }

            data.Append($".{(byte)characterQuest.Quest.QuestType}");

            switch (characterQuest.Quest.QuestType)
            {
                case QuestType.WIN_RAID_AND_TALK_TO_NPC:
                case QuestType.DELIVER_ITEM_TO_NPC:
                case QuestType.KILL_MONSTER_BY_VNUM:
                case QuestType.GIVE_ITEM_TO_NPC:
                case QuestType.GIVE_ITEM_TO_NPC_2:
                case QuestType.DIALOG_WHILE_WEARING:
                case QuestType.DIALOG_WHILE_HAVING_ITEM:
                case QuestType.USE_ITEM_ON_TARGET:
                case QuestType.CAPTURE_WITHOUT_KEEPING:
                case QuestType.CAPTURE_AND_KEEP:
                case QuestType.DROP_IN_TIMESPACE:
                case QuestType.COMPLETE_TIMESPACE:
                case QuestType.CRAFT_WITHOUT_KEEPING:
                case QuestType.DROP_CHANCE:
                case QuestType.DROP_CHANCE_2:
                case QuestType.COLLECT:
                case QuestType.DROP_HARDCODED:
                case QuestType.KILL_X_MOBS_SOUND_FLOWER:
                case QuestType.KILL_PLAYER_IN_REGION:
                    statusAdded = false;
                    objectives = characterQuest.Quest.Objectives;
                    foreach (QuestObjectiveDto objective in objectives)
                    {
                        CharacterQuestObjectiveDto questObjectiveDto = characterQuest.ObjectiveAmount[objective.ObjectiveIndex];
                        data.Append($".{questObjectiveDto.CurrentAmount}.{questObjectiveDto.RequiredAmount}");

                        if (!statusAdded)
                        {
                            data.Append($".{(isFinished ? 1 : 0)}");
                            statusAdded = true;
                        }
                    }

                    for (int i = objectives.Count; i < 5; i++)
                    {
                        data.Append(".0.0");
                    }

                    break;

                case QuestType.DIALOG:
                case QuestType.DIALOG_2:
                case QuestType.NOTHING:
                case QuestType.GO_TO_MAP:
                case QuestType.GIVE_NPC_GOLD:
                    data.Append($".0.0.{(isFinished ? 1 : 0)}.0.0.0.0.0.0.0.0");
                    break;
                case QuestType.DIE_X_TIMES:
                case QuestType.EARN_REPUTATION:
                case QuestType.COMPLETE_TIMESPACE_WITH_ATLEAST_X_POINTS:
                    break;
            }

            if (qstlistCall)
            {
                data.Append(characterQuest.QuestId != questToShowInfo ? ".0" : $".{(isFinished ? 0 : 1)}");
            }
            else
            {
                data.Append(".0");
            }

            return data.ToString();
        }

        public static bool IsQuestCompleted(this IPlayerEntity player, CharacterQuest characterQuest)
        {
            if (characterQuest == null)
            {
                return false;
            }

            IEnumerable<QuestObjectiveDto> objectives = characterQuest.Quest.Objectives;
            switch (characterQuest.Quest.QuestType)
            {
                case QuestType.DELIVER_ITEM_TO_NPC:
                case QuestType.GIVE_ITEM_TO_NPC:
                case QuestType.GIVE_ITEM_TO_NPC_2:
                case QuestType.KILL_MONSTER_BY_VNUM:
                case QuestType.CAPTURE_WITHOUT_KEEPING:
                case QuestType.CAPTURE_AND_KEEP:
                case QuestType.CRAFT_WITHOUT_KEEPING:
                case QuestType.DROP_CHANCE:
                case QuestType.DROP_CHANCE_2:
                case QuestType.DROP_IN_TIMESPACE:
                case QuestType.COLLECT:
                case QuestType.DROP_HARDCODED:
                case QuestType.COMPLETE_TIMESPACE_WITH_ATLEAST_X_POINTS:
                case QuestType.GIVE_NPC_GOLD:
                case QuestType.COMPLETE_TIMESPACE:
                case QuestType.KILL_X_MOBS_SOUND_FLOWER:
                case QuestType.KILL_PLAYER_IN_REGION:
                    return !objectives.Any(s => characterQuest.ObjectiveAmount[s.ObjectiveIndex].CurrentAmount < characterQuest.ObjectiveAmount[s.ObjectiveIndex].RequiredAmount);
                case QuestType.GO_TO_MAP:
                    return player.Position.IsInAoeZone(new Position(characterQuest.Quest.TargetMapX, characterQuest.Quest.TargetMapY), 2);
                case QuestType.USE_ITEM_ON_TARGET:
                    return !objectives.Any(s => characterQuest.ObjectiveAmount[s.ObjectiveIndex].CurrentAmount <= 0);
                case QuestType.DIALOG_WHILE_HAVING_ITEM:
                    return !objectives.Any(s => player.CountItemWithVnum(s.Data1) < characterQuest.ObjectiveAmount[s.ObjectiveIndex].RequiredAmount);
                case QuestType.DIALOG:
                case QuestType.DIALOG_2:
                case QuestType.DIALOG_WHILE_WEARING:
                    if ((characterQuest.QuestId == (int)QuestsVnums.TALK_WEARING_KOVOLT_MASK_1 || characterQuest.QuestId == (int)QuestsVnums.TALK_WEARING_KOVOLT_MASK_2)
                        && player.Class != ClassType.Adventurer) // Kovolt Mask
                    {
                        return true;
                    }

                    return !objectives.Any(s => characterQuest.ObjectiveAmount[s.ObjectiveIndex].CurrentAmount <= 0);
                case QuestType.NOTHING:
                    return true;

                // Not used for now
                case QuestType.WIN_RAID_AND_TALK_TO_NPC:
                    return !objectives.Any(s =>
                    {
                        CharacterQuestObjectiveDto questObjectiveDto = characterQuest.ObjectiveAmount[s.ObjectiveIndex];
                        return questObjectiveDto.CurrentAmount < questObjectiveDto.RequiredAmount;
                    });
                case QuestType.DIE_X_TIMES:
                case QuestType.EARN_REPUTATION:
                    break;
            }

            return false;
        }

        public static string GenerateSqstPacket(this IClientSession session, IQuestManager questManager, int sqstIndex)
        {
            IReadOnlyCollection<QuestNpcDto> blueAlertQuests = questManager.GetNpcBlueAlertQuests().ToList();
            IReadOnlyCollection<CharacterQuest> completedBlueAlertQuests = session.PlayerEntity.GetCompletedQuests().Where(s => blueAlertQuests.Any(t => t.QuestId == s.QuestId)).ToList();
            var sb = new StringBuilder();
            sb.Append($"sqst  {sqstIndex} ");
            for (byte packetIndex = 0; packetIndex < 250; packetIndex++)
            {
                byte[] binary = { 0, 0, 0, 0, 0, 0 };
                for (byte bitIndex = 0; bitIndex < 6; bitIndex++)
                {
                    int questId = packetIndex * 6 + bitIndex + 1500 * sqstIndex;
                    if (completedBlueAlertQuests.Any(s => s.QuestId == questId))
                    {
                        if (bitIndex == 0)
                        {
                            binary[bitIndex] = 1;
                        }
                        else
                        {
                            binary[^bitIndex] = 1;
                        }
                    }
                }

                string binaryString = string.Join("", binary);
                sb.Append(_b64SqstIndex.ElementAt(Convert.ToInt16(binaryString, 2)));
            }

            return sb.ToString();
        }

        public static void SendSqstPackets(this IClientSession session, IQuestManager questManager)
        {
            var packets = new List<string>();
            int sqstPacketsAmount = (int)Math.Ceiling(questManager.GetNpcBlueAlertQuests().Max(s => s.QuestId ?? 0) / 1500.0);
            for (int i = 0; i < sqstPacketsAmount; i++)
            {
                packets.Add(session.GenerateSqstPacket(questManager, i));
            }

            session.SendPackets(packets);
        }

        public static void SendSqstPacket(this IClientSession session, IQuestManager questManager, int sqstIndex) => session.SendPacket(session.GenerateSqstPacket(questManager, sqstIndex));

        public static void UpdateQuestSqstPacket(this IClientSession session, IQuestManager questManager, int questVnum)
        {
            int sqstIndex = (int)Math.Floor(questVnum / 1500.0);
            session.SendPacket(session.GenerateSqstPacket(questManager, sqstIndex));
        }

        public static void SendQuestsTargets(this IClientSession session)
        {
            IReadOnlyCollection<CharacterQuest> characterQuests = session.PlayerEntity.GetCurrentQuests().Where(s => s.Quest.TargetMapId != 0).ToList();
            if (!characterQuests.Any())
            {
                return;
            }

            foreach (CharacterQuest characterQuest in characterQuests)
            {
                QuestDto quest = characterQuest.Quest;
                session.SendTargetQuest(quest.TargetMapX, quest.TargetMapY, quest.TargetMapId, quest.Id);
            }
        }

        public static void DeleteQuestTarget(this IClientSession session, CharacterQuest characterQuest)
        {
            if (characterQuest.Quest.TargetMapId == 0)
            {
                return;
            }

            session.SendTargetOffQuest(characterQuest.Quest.TargetMapX, characterQuest.Quest.TargetMapY, characterQuest.Quest.TargetMapId, characterQuest.QuestId);
        }

        public static void SendTargetQuest(this IClientSession session, short x, short y, short mapId, int questId) => session.SendPacket(session.GenerateTargetQuest(x, y, mapId, questId));
        public static void SendTargetOffQuest(this IClientSession session, short x, short y, short mapId, int questId) => session.SendPacket(session.GenerateTargetOffQuest(x, y, mapId, questId));

        public static void RefreshQuestList(this IClientSession session, IQuestManager questManager, int? questToShowInfo) =>
            session.SendPacket(session.GenerateQuestList(questManager, questToShowInfo));

        public static void RefreshQuestProgress(this IClientSession session, IQuestManager questManager, int questId) => session.SendPacket(session.GenerateQuestProgress(questManager, questId));

        public static void SendQrPacket(this IClientSession session, CharacterQuest quest, IReadOnlyCollection<CharacterQuestGeneratedReward> rndRewards, QuestsRatesConfiguration questRates) =>
            session.SendPacket(session.GenerateQrPacket(quest, rndRewards, questRates));

        public static void SendQnpcPacket(this IClientSession session, short level, short npcVnum, short mapId) => session.SendPacket(session.GenerateQnpcPacket(level, npcVnum, mapId));
        public static CharacterQuest GetQuestById(this IPlayerEntity character, int questId) => character.GetCurrentQuest(questId);
        public static CharacterQuest GetQuestByActionSlot(this IClientSession session, int action, int slot) => action == 1 ? session.PlayerEntity.GetCurrentQuest(slot) : session.GetQuestBySlot(slot);

        public static CharacterQuest GetQuestBySlot(this IClientSession session, int slot)
        {
            CharacterQuest quest;

            if (slot < 0 || slot > 10)
            {
                Log.Debug($"[ERROR] PACKET QT: Invalid slot index: {slot.ToString()}");
                return null;
            }

            if (slot < 5)
            {
                quest = session.PlayerEntity.GetCurrentQuests().Where(s => s.SlotType == QuestSlotType.GENERAL).ElementAtOrDefault(slot);
            }
            else if (slot == 5)
            {
                quest = session.PlayerEntity.GetCurrentQuests().Where(s => s.SlotType == QuestSlotType.MAIN).ElementAtOrDefault(0);
            }
            else
            {
                slot -= 6;
                quest = session.PlayerEntity.GetCurrentQuests().Where(s => s.SlotType == QuestSlotType.SECONDARY).ElementAtOrDefault(slot);
            }

            return quest;
        }

        public static long GetLevelXpPercentage(this ICharacterAlgorithm characterAlgorithm, short percentage, short level) =>
            Convert.ToInt64(characterAlgorithm.GetLevelXp(level) * (percentage / 100.0));

        public static long GetSpecialistJobXpPercentage(this ICharacterAlgorithm characterAlgorithm, short percentage, short level, bool isFunSpecialist) =>
            Convert.ToInt64(characterAlgorithm.GetSpecialistJobXp(level, isFunSpecialist) * (percentage / 100.0));

        public static long GetHeroLevelXpPercentage(this ICharacterAlgorithm characterAlgorithm, short percentage, short level) =>
            Convert.ToInt64(characterAlgorithm.GetHeroLevelXp(level) * (percentage / 100.0));

        public static long GetJobXpPercentage(this ICharacterAlgorithm characterAlgorithm, short percentage, short level) => Convert.ToInt64(characterAlgorithm.GetJobXp(level) * (percentage / 100.0));

        public static int GetEmptyQuestSlot(this IClientSession session, QuestSlotType slotType, bool isSoundFlower = false)
        {
            IReadOnlyCollection<CharacterQuest> quests = session.PlayerEntity.GetCurrentQuests().Where(s => s.SlotType == slotType).ToList();
            int currentGeneralQuestsCount = quests.Count + session.PlayerEntity.GetPendingSoundFlowerQuests();
            int slot = slotType switch
            {
                QuestSlotType.GENERAL => currentGeneralQuestsCount > (isSoundFlower ? 5 : 4) ? -1 : quests.Count,
                QuestSlotType.MAIN => !quests.Any() ? 5 : -1,
                QuestSlotType.SECONDARY => quests.Count > 4 ? -1 : quests.Count + 6,
                _ => -1
            };
            return slot;
        }

        public static int GetQuestSlot(this IClientSession session, CharacterQuest characterQuest)
        {
            var quests = session.PlayerEntity.GetCurrentQuests().Where(s => s.SlotType == characterQuest.SlotType).ToList();
            return characterQuest.SlotType switch
            {
                QuestSlotType.GENERAL => quests.IndexOf(characterQuest),
                QuestSlotType.MAIN => 5,
                QuestSlotType.SECONDARY => quests.IndexOf(characterQuest) + 6,
                _ => -1
            };
        }

        public static bool HasAlreadyQuestOrQuestline(this IClientSession session, QuestDto quest, IQuestManager questManager, INpcRunTypeQuestsConfiguration npcRunTypeQuestsConfiguration)
        {
            return session.PlayerEntity.HasQuestWithId(quest.Id)
                || session.PlayerEntity.GetCurrentQuests().SelectMany(s => questManager.GetQuestlines(s.QuestId)).Intersect(questManager.GetQuestlines(quest.Id)).Any()
                || session.HasRunningPeriodicQuest(quest, npcRunTypeQuestsConfiguration);
        }

        public static async Task<bool> HasCompletedPeriodicQuest(this IClientSession session, QuestDto quest, IQuestManager questManager, INpcRunTypeQuestsConfiguration npcRunTypeQuestsConfiguration,
            IPeriodicQuestsConfiguration periodicQuestsConfiguration)
        {
            IEnumerable<CharacterQuestDto> completedQuests = session.PlayerEntity.GetCompletedPeriodicQuests();
            PeriodicQuestSet periodicQuestSet = periodicQuestsConfiguration?.GetPeriodicQuestSetByQuestId(quest.Id);
            if (periodicQuestSet == null)
            {
                return completedQuests.Any(s => npcRunTypeQuestsConfiguration.HaveTheSameNpcRunType(s.QuestId, quest.Id));
            }

            bool exist = periodicQuestSet.PerNoswingsAccount is true
                ? await questManager.IsDailyQuestExist(session.Account.MasterAccountId, periodicQuestSet.Id)
                : await questManager.IsDailyQuestExist(session.PlayerEntity.Id, periodicQuestSet.Id);
            return completedQuests.Any(s => npcRunTypeQuestsConfiguration.HaveTheSameNpcRunType(s.QuestId, quest.Id)) || exist;
        }

        public static bool HasRunningPeriodicQuest(this IClientSession session, QuestDto quest, INpcRunTypeQuestsConfiguration npcRunTypeQuestsConfiguration)
        {
            IEnumerable<CharacterQuestDto> activeQuests = session.PlayerEntity.GetCurrentQuests();
            return activeQuests.Any(s => npcRunTypeQuestsConfiguration.HaveTheSameNpcRunType(s.QuestId, quest.Id));
        }

        public static PeriodicQuestSet GetPeriodicQuestSet(this QuestDto quest, IQuestManager questManager, IPeriodicQuestsConfiguration periodicQuestsConfiguration)
        {
            IReadOnlyCollection<PeriodicQuestSet> dailyQuests = periodicQuestsConfiguration.GetDailyQuests();
            if (dailyQuests == null)
            {
                return null;
            }

            return dailyQuests.FirstOrDefault(set =>
            {
                foreach (int questVnum in set.QuestVnums)
                {
                    QuestDto linkedQuest = questManager.GetQuestById(questVnum);
                    if (linkedQuest != null && linkedQuest.NextQuestId == quest.Id)
                    {
                        return true;
                    }
                }
                return false;
            });
        }

        public static bool ShowNpcDialog(this IClientSession session, INpcEntity npcEntity, IQuestManager questManager)
        {
            IEnumerable<CharacterQuest> characterQuests = session.PlayerEntity.GetCurrentQuestsByTypes(DialogQuestTypes).ToArray();

            if (!characterQuests.Any())
            {
                return true;
            }

            foreach (CharacterQuest characterQuest in characterQuests)
            {
                if (questManager.IsNpcBlueAlertQuest(characterQuest.QuestId) && (characterQuest.Quest.QuestType == QuestType.DIALOG || characterQuest.Quest.QuestType == QuestType.DIALOG_2))
                {
                    continue;
                }

                foreach (QuestObjectiveDto objective in characterQuest.Quest.Objectives)
                {
                    if (npcEntity.NpcVNum != (characterQuest.Quest.QuestType is QuestType.DELIVER_ITEM_TO_NPC ? objective.Data1 : objective.Data0))
                    {
                        continue;
                    }

                    CharacterQuestObjectiveDto questObjectiveDto = characterQuest.ObjectiveAmount[objective.ObjectiveIndex];
                    switch (characterQuest.Quest.QuestType)
                    {
                        case QuestType.DELIVER_ITEM_TO_NPC:
                        case QuestType.GIVE_ITEM_TO_NPC_2:
                        case QuestType.GIVE_ITEM_TO_NPC:
                            int amountLeft = questObjectiveDto.RequiredAmount - questObjectiveDto.CurrentAmount;
                            if (amountLeft == 0)
                            {
                                continue;
                            }

                            int amountInPossession = session.PlayerEntity.CountItemWithVnum(characterQuest.Quest.QuestType is QuestType.DELIVER_ITEM_TO_NPC ? objective.Data0 : objective.Data1);
                            if (amountInPossession == 0)
                            {
                                continue;
                            }

                            return false;
                        case QuestType.GIVE_NPC_GOLD:
                            int totalGoldToGive = questObjectiveDto.RequiredAmount;
                            if (session.PlayerEntity.Gold < totalGoldToGive)
                            {
                                continue;
                            }

                            return false;
                        case QuestType.DIALOG:
                        case QuestType.DIALOG_2:
                            return false;
                    }
                }
            }

            return true;
        }
    }
}