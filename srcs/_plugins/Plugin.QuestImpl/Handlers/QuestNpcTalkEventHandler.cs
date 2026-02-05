using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using PhoenixLib.Events;
using WingsAPI.Game.Extensions.ItemExtension.Inventory;
using WingsAPI.Game.Extensions.Quests;
using WingsAPI.Packets.Enums.BattlePass;
using WingsEmu.DTOs.Quests;
using WingsEmu.Game.Characters.Events;
using WingsEmu.Game.Entities;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Helpers.Damages;
using WingsEmu.Game.Items;
using WingsEmu.Game.Managers;
using WingsEmu.Game.Managers.StaticData;
using WingsEmu.Game.Networking;
using WingsEmu.Game.Quests;
using WingsEmu.Game.Quests.Event;
using WingsEmu.Packets.Enums;

namespace Plugin.QuestImpl.Handlers
{
    public class QuestNpcTalkEventHandler : IAsyncEventProcessor<QuestNpcTalkEvent>
    {
        private readonly IItemsManager _itemsManager;
        private readonly IQuestManager _questManager;
        private readonly IServerManager _serverManager;

        public QuestNpcTalkEventHandler(IItemsManager itemsManager, IServerManager serverManager, IQuestManager questManager)
        {
            _itemsManager = itemsManager;
            _serverManager = serverManager;
            _questManager = questManager;
        }

        public async Task HandleAsync(QuestNpcTalkEvent e, CancellationToken cancellation)
        {
            IClientSession session = e.Sender;
            CharacterQuest characterQuest = e.CharacterQuest;
            INpcEntity npcEntity = e.NpcEntity;
            bool isByBlueAlertNrun = e.IsByBlueAlertNrun;

            if (!session.PlayerEntity.IsInAoeZone(npcEntity, _serverManager.MaxNpcTalkRange))
            {
                return;
            }

            switch (characterQuest.Quest.QuestType)
            {
                case QuestType.DIALOG: //12
                case QuestType.DIALOG_2: //22
                case QuestType.DELIVER_ITEM_TO_NPC: //4
                case QuestType.GIVE_ITEM_TO_NPC: //14 (Same than before but without random values)
                case QuestType.GIVE_ITEM_TO_NPC_2: //24
                case QuestType.GIVE_NPC_GOLD: //18
                    HandleDeliver(session, characterQuest, npcEntity, isByBlueAlertNrun);
                    break;
                case QuestType.DIALOG_WHILE_WEARING: //15
                    HandleWearing(session, characterQuest, npcEntity);
                    break;
                case QuestType.DIALOG_WHILE_HAVING_ITEM: //16
                    HandleHaving(session, characterQuest, npcEntity);
                    break;
                case QuestType.WIN_RAID_AND_TALK_TO_NPC: //25
                    await HandleRaid(session, characterQuest, npcEntity);
                    break;
            }
        }

        public void HandleDeliver(IClientSession session, CharacterQuest characterQuest, INpcEntity npcEntity, bool isByBlueAlertNrun)
        {
            IEnumerable<QuestObjectiveDto> objectives = characterQuest.Quest.Objectives;
            foreach (QuestObjectiveDto objective in objectives)
            {
                if (npcEntity.NpcVNum != (characterQuest.Quest.QuestType is QuestType.DELIVER_ITEM_TO_NPC ? objective.Data1 : objective.Data0))
                {
                    continue;
                }

                CharacterQuestObjectiveDto questObjectiveDto = characterQuest.ObjectiveAmount[objective.ObjectiveIndex];
                switch (characterQuest.Quest.QuestType)
                {
                    case QuestType.DELIVER_ITEM_TO_NPC:
                    case QuestType.GIVE_ITEM_TO_NPC:

                        int amountLeft = questObjectiveDto.RequiredAmount - questObjectiveDto.CurrentAmount;
                        if (amountLeft == 0)
                        {
                            break;
                        }

                        int amountInPossession = session.PlayerEntity.CountItemWithVnum(characterQuest.Quest.QuestType is QuestType.DELIVER_ITEM_TO_NPC ? objective.Data0 : objective.Data1);
                        if (amountInPossession == 0)
                        {
                            break;
                        }

                        int amountToRemove = Math.Min(amountLeft, amountInPossession);
                        session.RemoveItemFromInventory(characterQuest.Quest.QuestType is QuestType.DELIVER_ITEM_TO_NPC ? objective.Data0 : objective.Data1, (short)amountToRemove);
                        questObjectiveDto.CurrentAmount += amountToRemove;

                        session.EmitEvent(new QuestObjectiveUpdatedEvent
                        {
                            CharacterQuest = characterQuest
                        });

                        session.RefreshQuestProgress(_questManager, characterQuest.QuestId);

                        if (session.PlayerEntity.IsQuestCompleted(characterQuest))
                        {
                            session.EmitEvent(new QuestCompletedEvent(characterQuest));
                        }

                        break;
                    case QuestType.GIVE_NPC_GOLD:
                        int totalGoldToGive = questObjectiveDto.RequiredAmount; // It is the way it's stored
                        if (session.PlayerEntity.Gold < totalGoldToGive)
                        {
                            continue;
                        }

                        questObjectiveDto.CurrentAmount += totalGoldToGive;
                        session.EmitEvent(new QuestObjectiveUpdatedEvent
                        {
                            CharacterQuest = characterQuest
                        });

                        session.PlayerEntity.RemoveGold(totalGoldToGive);
                        session.RefreshQuestProgress(_questManager, characterQuest.QuestId);

                        session.EmitEvent(new QuestCompletedEvent(characterQuest));
                        session.EmitEvent(new IncreaseBattlePassObjectiveEvent(MissionType.SpendXGoldToNpc, totalGoldToGive));
                        break;

                    case QuestType.DIALOG:
                    case QuestType.DIALOG_2:

                        // They have a special behavior
                        if (_questManager.IsNpcBlueAlertQuest(characterQuest.QuestId) && !isByBlueAlertNrun)
                        {
                            continue;
                        }

                        questObjectiveDto.CurrentAmount++;
                        session.EmitEvent(new QuestObjectiveUpdatedEvent
                        {
                            CharacterQuest = characterQuest
                        });

                        bool giveNextQuest = !_questManager.IsNpcBlueAlertQuest(characterQuest.QuestId) ||
                            _questManager.IsNpcBlueAlertQuest(characterQuest.QuestId) && !session.PlayerEntity.HasCompletedQuest(characterQuest.Quest.NextQuestId);
                        if (session.PlayerEntity.IsQuestCompleted(characterQuest))
                        {
                            session.EmitEvent(new QuestCompletedEvent(characterQuest, true, giveNextQuest: giveNextQuest));
                        }

                        break;

                    case QuestType.GIVE_ITEM_TO_NPC_2:

                        if (objective.Data0 != npcEntity.NpcVNum)
                        {
                            return;
                        }

                        amountLeft = questObjectiveDto.RequiredAmount - questObjectiveDto.CurrentAmount;
                        if (amountLeft == 0)
                        {
                            break;
                        }

                        questObjectiveDto.CurrentAmount++;
                        session.RefreshQuestProgress(_questManager, characterQuest.QuestId);
                        session.EmitEvent(new QuestObjectiveUpdatedEvent
                        {
                            CharacterQuest = characterQuest
                        });

                        if (session.PlayerEntity.IsQuestCompleted(characterQuest))
                        {
                            session.EmitEvent(new QuestCompletedEvent(characterQuest, true, giveNextQuest: _questManager.IsNpcBlueAlertQuest(characterQuest.QuestId)));
                        }

                        break;
                }
            }
        }

        public async Task HandleRaid(IClientSession session, CharacterQuest characterQuest, INpcEntity npcEntity)
        {
            IEnumerable<QuestObjectiveDto> objectives = characterQuest.Quest.Objectives;
            foreach (QuestObjectiveDto objective in objectives)
            {
                if (npcEntity.NpcVNum != objective.Data2)
                {
                    continue;
                }

                CharacterQuestObjectiveDto questObjectiveDto = characterQuest.ObjectiveAmount[objective.ObjectiveIndex];
                int amountLeft = questObjectiveDto.RequiredAmount - questObjectiveDto.CurrentAmount;
                if (amountLeft != 0)
                {
                    return;
                }

                await session.EmitEventAsync(new QuestCompletedEvent(characterQuest, true, ignoreNotCompletedQuest: true));
            }
        }

        public void HandleWearing(IClientSession session, CharacterQuest characterQuest, INpcEntity npcEntity)
        {
            IEnumerable<QuestObjectiveDto> objectives = characterQuest.Quest.Objectives;
            foreach (QuestObjectiveDto objective in objectives)
            {
                if (npcEntity.NpcVNum != objective.Data0)
                {
                    continue;
                }

                IGameItem gameItem = _itemsManager.GetItem(objective.Data1);
                if (gameItem == null)
                {
                    continue;
                }

                GameItemInstance inv = session.PlayerEntity.GetItemInstanceFromEquipmentSlot(gameItem.EquipmentSlot);
                if (inv == null || inv.ItemVNum != objective.Data1)
                {
                    continue;
                }

                CharacterQuestObjectiveDto questObjectiveDto = characterQuest.ObjectiveAmount[objective.ObjectiveIndex];
                questObjectiveDto.CurrentAmount++;
            }

            // Checks that all the requirements have been met
            if (!session.PlayerEntity.IsQuestCompleted(characterQuest))
            {
                characterQuest.ResetQuestProgress();
            }

            else
            {
                session.EmitEventAsync(new QuestObjectiveUpdatedEvent
                {
                    CharacterQuest = characterQuest
                });
                session.EmitEvent(new QuestCompletedEvent(characterQuest));
            }
        }

        public void HandleHaving(IClientSession session, CharacterQuest characterQuest, INpcEntity npcEntity)
        {
            IEnumerable<QuestObjectiveDto> objectives = characterQuest.Quest.Objectives;
            foreach (QuestObjectiveDto objective in objectives)
            {
                CharacterQuestObjectiveDto questObjectiveDto = characterQuest.ObjectiveAmount[objective.ObjectiveIndex];
                if (npcEntity.NpcVNum != objective.Data0 || session.PlayerEntity.CountItemWithVnum(objective.Data1) < questObjectiveDto.RequiredAmount)
                {
                    return;
                }
            }

            session.EmitEvent(new QuestCompletedEvent(characterQuest));
        }
    }
}