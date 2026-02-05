using System;
using System.Threading;
using System.Threading.Tasks;
using PhoenixLib.Events;
using WingsAPI.Packets.Enums;
using WingsEmu.DTOs.Bonus;
using WingsEmu.Game._i18n;
using WingsEmu.Game.Configurations;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Families;
using WingsEmu.Game.Families.Event;
using WingsEmu.Game.Networking;
using WingsEmu.Packets.Enums.Chat;

namespace Plugin.FamilyImpl
{
    public class FamilyAddExperienceEventHandler : IAsyncEventProcessor<FamilyAddExperienceEvent>
    {
        private readonly IFamilyManager _familyManager;
        private readonly IEvtbConfiguration _evtbConfiguration;

        public FamilyAddExperienceEventHandler(IFamilyManager familyManager, IEvtbConfiguration evtbConfiguration)
        {
            _familyManager = familyManager;
            _evtbConfiguration = evtbConfiguration;
        }

        public async Task HandleAsync(FamilyAddExperienceEvent e, CancellationToken cancellation)
        {
            IClientSession session = e.Sender;
            long experience = e.ExperienceGained;

            if (session.PlayerEntity.HaveStaticBonus(StaticBonusType.FxpBooster))
            {
                experience *= 2;
            }
            
            double increaseEventFactor = 1 + _evtbConfiguration.GetValueForEventType(EvtbType.INCREASE_FAMILY_XP_EARNED) * 0.01;
            experience = (long)(experience * increaseEventFactor);

            _familyManager.SendExperienceToFamilyServer(new ExperienceGainedSubMessage(session.PlayerEntity.Id, experience, e.FamXpObtainedFromType, DateTime.UtcNow));
            session.SendSayi(ChatMessageColorType.Yellow, Game18NConstString.RecivedActionPoints, 4, (int)experience);
        }
    }
}