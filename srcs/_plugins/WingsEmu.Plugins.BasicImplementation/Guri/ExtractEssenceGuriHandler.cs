using System;
using System.Linq;
using System.Threading.Tasks;
using WingsAPI.Game.Extensions.Groups;
using WingsAPI.Game.Extensions.ItemExtension.Inventory;
using WingsAPI.Packets.Enums;
using WingsEmu.DTOs.Maps;
using WingsEmu.Game;
using WingsEmu.Game._Guri;
using WingsEmu.Game._Guri.Event;
using WingsEmu.Game._ItemUsage.Configuration;
using WingsEmu.Game.Buffs;
using WingsEmu.Game.Characters.Events;
using WingsEmu.Game.Configurations;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Items;
using WingsEmu.Game.Managers;
using WingsEmu.Game.Mates;
using WingsEmu.Game.Mates.Events;
using WingsEmu.Game.Networking;
using WingsEmu.Packets.Enums;

namespace NosEmu.Plugins.BasicImplementations.Guri;

public class ExtractEssenceGuriHandler : IGuriHandler
{
    private readonly PetMaxLevelConfiguration _config;
    private readonly IGameItemInstanceFactory _gameItemInstanceFactory;
    private readonly IRandomGenerator _randomGenerator;
    private readonly IDelayManager _delayManager;
    private readonly ISpPartnerConfiguration _spPartner;
    private readonly IEvtbConfiguration _evtbConfiguration;
    private readonly TrainerRatesConfiguration _trainerRatesConfiguration;

    public ExtractEssenceGuriHandler(PetMaxLevelConfiguration config, IGameItemInstanceFactory gameItemInstanceFactory, IRandomGenerator randomGenerator,
        ISpPartnerConfiguration spPartner, IEvtbConfiguration evtbConfiguration, TrainerRatesConfiguration trainerRatesConfiguration)
    {
        _config = config;
        _gameItemInstanceFactory = gameItemInstanceFactory;
        _randomGenerator = randomGenerator;
        _spPartner = spPartner;
        _evtbConfiguration = evtbConfiguration;
        _trainerRatesConfiguration = trainerRatesConfiguration;
    }

    public long GuriEffectId => 451;

    public async Task ExecuteAsync(IClientSession session, GuriEvent e)
    {
        if (!session.PlayerEntity.MapInstance.HasMapFlag(MapFlags.IS_MINILAND_MAP))
        {
            return;
        }

        IMateEntity mateEntity = session.PlayerEntity.MateComponent.GetMates().FirstOrDefault(s => s.PetSlot == e.Data);

        if (mateEntity == null)
        {
            return;
        }
        
        if (mateEntity.MateType == MateType.Partner)
        {
            return;
        }

        MaxPetLevelConfiguration infos = _config.Configurations.FirstOrDefault(s => s.Stars == mateEntity.Stars);

        if (infos == null)
        {
            return;
        }

        if (mateEntity.HeroLevel < infos.MaxLevel)
        {
            return;
        }

        bool twoEssence = false;
        if (session.PlayerEntity.BCardComponent.HasBCard(BCardType.Capture, (byte)AdditionalTypes.Capture.AdvancedExtractingEssence))
        {
            (int firstData, int secondData) = session.PlayerEntity.BCardComponent.GetAllBCardsInformation(BCardType.Capture,
                (byte)AdditionalTypes.Capture.AdvancedExtractingEssence, session.PlayerEntity.Level);
            if (_randomGenerator.RandomNumber() < firstData && mateEntity.Stars == secondData)
            {
                twoEssence = true;
            }
        }

        GameItemInstance essence = _gameItemInstanceFactory.CreateItem(infos.ItemVnum, twoEssence ? 2 : 1);

        await session.EmitEventAsync(new MateRemoveEvent
        {
            MateEntity = mateEntity
        });
        session.RefreshParty(_spPartner);
        await session.AddNewItemToInventory(essence, true);
        
        double bcardExperienceFirstData = session.PlayerEntity.BCardComponent.GetAllBCardsInformation(BCardType.SPCardUpgrade,
            (byte)AdditionalTypes.SPCardUpgrade.IncreaseSpecialistTrainerExperience, session.PlayerEntity.Level).firstData * 0.01;
        
        long experienceEarned = (int)(mateEntity.Stars * mateEntity.HeroLevel * 100 * (1 + bcardExperienceFirstData));
        int evtbIncreaseChance = _evtbConfiguration.GetValueForEventType(EvtbType.INCREASE_PET_TRAINER_EXPERIENCE);
        int experienceRate = _trainerRatesConfiguration.ExtractEssenceRate;
        
        experienceEarned += (long)(experienceEarned * (evtbIncreaseChance * 0.01));
        experienceEarned += experienceEarned * experienceRate;
        await session.EmitEventAsync(new AddExpEvent(experienceEarned, LevelType.SpJobLevel));
    }
}