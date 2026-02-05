using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using PhoenixLib.Events;
using WingsEmu.Game;
using WingsEmu.Game._i18n;
using WingsEmu.Game.Battle;
using WingsEmu.Game.Configurations;
using WingsEmu.Game.Entities;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Mates;
using WingsEmu.Game.Mates.Events;
using WingsEmu.Game.Networking;
using WingsEmu.Packets.Enums.Chat;

namespace NosEmu.Plugins.BasicImplementations.Event.Mates
{
    public enum MateUpgradeResult
    {
        Succeed,
        NoLearn,
        Fail
    }

    public class MateCheckUpgradeProgressEventHandler : IAsyncEventProcessor<MateCheckUpgradeProgressEvent>
    {
        private readonly IGameLanguageService _gameLanguage;
        private readonly ITrainerConfiguration _trainerConfiguration;
        private readonly IRandomGenerator _randomGenerator;

        public MateCheckUpgradeProgressEventHandler(IRandomGenerator randomGenerator, IGameLanguageService gameLanguage, ITrainerConfiguration trainerConfiguration)
        {
            _randomGenerator = randomGenerator;
            _gameLanguage = gameLanguage;
            _trainerConfiguration = trainerConfiguration;
        }

        public async Task HandleAsync(MateCheckUpgradeProgressEvent e, CancellationToken cancellation)
        {
            IClientSession session = e.Sender;
            IMateEntity mateEntity = e.MateEntity;
            IMonsterEntity monsterDoll = e.MateDoll;
            bool isAttackProgress = e.IsAttackProgress;

            if (mateEntity == null)
            {
                return;
            }

            if (!mateEntity.IsAlive())
            {
                return;
            }

            if (monsterDoll == null)
            {
                return;
            }

            if (!monsterDoll.IsAlive())
            {
                return;
            }

            if (!monsterDoll.IsMateTrainer)
            {
                return;
            }

            if (isAttackProgress)
            {
                HandleAttack(session, mateEntity, monsterDoll);
                return;
            }

            HandleDefense(session, mateEntity, monsterDoll);
        }

        private void HandleDefense(IClientSession session, IMateEntity mateEntity, IMonsterEntity monsterDoll)
        {
            IReadOnlyList<TrainerConfigElement> configuration = _trainerConfiguration.GetDefenseConfigs(mateEntity.Defence);
            if (configuration == null)
            {
                return;
            }

            bool isStrong = _trainerConfiguration.IsStrongTrainer(monsterDoll.MonsterVNum);
            bool canLoseDefense = _trainerConfiguration.CanLoseDefense(monsterDoll.MonsterVNum);

            TrainerConfigElement configElement = configuration.FirstOrDefault(x => x.IsStrongTrainer == isStrong || x.CanLoseDefense == canLoseDefense);

            if (configElement == null)
            {
                return;
            }

            mateEntity.HitsFromDoll++;
            if (configElement.HitsRequired > mateEntity.HitsFromDoll)
            {
                return;
            }

            mateEntity.HitsFromDoll = 0;

            var randomBag = new RandomBag<MateUpgradeResult>(_randomGenerator);

            randomBag.AddEntry(MateUpgradeResult.Succeed, configElement.SuccessChance);

            if (configElement.FailChance > 0)
            {
                randomBag.AddEntry(MateUpgradeResult.Fail, 10000 - configElement.SuccessChance - configElement.FailChance);
            }

            if (configElement.NoLearningChance > 0)
            {
                randomBag.AddEntry(MateUpgradeResult.NoLearn, configElement.NoLearningChance);
            }

            MateUpgradeResult result = randomBag.GetRandom();

            switch (result)
            {
                case MateUpgradeResult.Succeed:
                    mateEntity.Defence++;
                    session.SendChatMessage(_gameLanguage.GetLanguageFormat(GameDialogKey.DOLL_MESSAGE_MATE_DEFENSE_UPGRADE, session.UserLanguage, mateEntity.Defence - 1, mateEntity.Defence), ChatMessageColorType.Green);
                    session.SendMsg(_gameLanguage.GetLanguageFormat(GameDialogKey.DOLL_MESSAGE_MATE_DEFENSE_UPGRADE, session.UserLanguage, mateEntity.Defence - 1, mateEntity.Defence), MsgMessageType.Middle);
                    break;
                case MateUpgradeResult.NoLearn:
                    // Change messages this to NoLearn
                    session.SendChatMessage(_gameLanguage.GetLanguage(GameDialogKey.DOLL_MESSAGE_MATE_DEFENSE_FAIL, session.UserLanguage), ChatMessageColorType.Red);
                    session.SendMsg(_gameLanguage.GetLanguage(GameDialogKey.DOLL_MESSAGE_MATE_DEFENSE_FAIL, session.UserLanguage), MsgMessageType.Middle);
                    break;
                case MateUpgradeResult.Fail:
                    mateEntity.Defence--;
                    session.SendChatMessage(_gameLanguage.GetLanguage(GameDialogKey.DOLL_MESSAGE_MATE_DEFENSE_FAIL, session.UserLanguage), ChatMessageColorType.Red);
                    session.SendMsg(_gameLanguage.GetLanguage(GameDialogKey.DOLL_MESSAGE_MATE_DEFENSE_FAIL, session.UserLanguage), MsgMessageType.Middle);
                    break;
            }
        }

        private void HandleAttack(IClientSession session, IMateEntity mateEntity, IMonsterData monsterDoll)
        {
            IReadOnlyList<TrainerConfigElement> configuration = _trainerConfiguration.GetAttackConfigs(mateEntity.Attack);
            if (configuration == null)
            {
                return;
            }

            bool isStrong = _trainerConfiguration.IsStrongTrainer(monsterDoll.MonsterVNum);
            bool canLoseAttack = _trainerConfiguration.CanLoseAttack(monsterDoll.MonsterVNum);

            TrainerConfigElement configElement = configuration.FirstOrDefault(x => x.IsStrongTrainer == isStrong || x.CanLoseAttack == canLoseAttack);

            if (configElement == null)
            {
                return;
            }

            mateEntity.HitsAgainstDoll++;
            if (configElement.HitsRequired > mateEntity.HitsAgainstDoll)
            {
                return;
            }

            mateEntity.HitsAgainstDoll = 0;

            var randomBag = new RandomBag<MateUpgradeResult>(_randomGenerator);

            randomBag.AddEntry(MateUpgradeResult.Succeed, configElement.SuccessChance);

            if (configElement.FailChance > 0)
            {
                randomBag.AddEntry(MateUpgradeResult.Fail, 10000 - configElement.SuccessChance - configElement.FailChance);
            }

            if (configElement.NoLearningChance > 0)
            {
                randomBag.AddEntry(MateUpgradeResult.NoLearn, configElement.NoLearningChance);
            }

            MateUpgradeResult result = randomBag.GetRandom();

            switch (result)
            {
                case MateUpgradeResult.Succeed:
                    mateEntity.Attack++;
                    session.SendChatMessage(_gameLanguage.GetLanguageFormat(GameDialogKey.DOLL_MESSAGE_MATE_ATTACK_UPGRADE, session.UserLanguage, mateEntity.Attack - 1, mateEntity.Attack), ChatMessageColorType.Green);
                    session.SendMsg(_gameLanguage.GetLanguageFormat(GameDialogKey.DOLL_MESSAGE_MATE_ATTACK_UPGRADE, session.UserLanguage, mateEntity.Attack - 1, mateEntity.Attack), MsgMessageType.Middle);
                    break;
                case MateUpgradeResult.NoLearn:
                    // Change messages this to NoLearn
                    session.SendChatMessage(_gameLanguage.GetLanguage(GameDialogKey.DOLL_MESSAGE_MATE_ATTACK_FAIL, session.UserLanguage), ChatMessageColorType.Red);
                    session.SendMsg(_gameLanguage.GetLanguage(GameDialogKey.DOLL_MESSAGE_MATE_ATTACK_FAIL, session.UserLanguage), MsgMessageType.Middle);
                    break;
                case MateUpgradeResult.Fail:
                    mateEntity.Defence--;
                    session.SendChatMessage(_gameLanguage.GetLanguage(GameDialogKey.DOLL_MESSAGE_MATE_ATTACK_FAIL, session.UserLanguage), ChatMessageColorType.Red);
                    session.SendMsg(_gameLanguage.GetLanguage(GameDialogKey.DOLL_MESSAGE_MATE_ATTACK_FAIL, session.UserLanguage), MsgMessageType.Middle);
                    break;
            }
        }
    }
}