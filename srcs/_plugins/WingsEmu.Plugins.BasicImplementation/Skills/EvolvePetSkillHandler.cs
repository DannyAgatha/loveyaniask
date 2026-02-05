using PhoenixLib.Events;
using PhoenixLib.Scheduler;
using System;
using System.Linq;
using System.Threading.Tasks;
using WingsAPI.Packets.Enums.Chat;
using WingsEmu.DTOs.Maps;
using WingsEmu.Game.Configurations;
using WingsEmu.Game.Core.SkillHandling;
using WingsEmu.Game.Core.SkillHandling.Event;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Mates;
using WingsEmu.Game.Networking;
using WingsEmu.Packets.Enums;
using WingsEmu.Packets.Enums.Battle;
using WingsEmu.Packets.Enums.Chat;

namespace NosEmu.Plugins.BasicImplementations.Skills;

public class EvolvePetSkillHandler : ISkillHandler
{
    private readonly IAsyncEventPipeline _eventPipeline;
    private readonly PetMaxLevelConfiguration _config;
    private readonly IScheduler _scheduler;

    public EvolvePetSkillHandler(IAsyncEventPipeline eventPipeline, PetMaxLevelConfiguration config, IScheduler scheduler)
    {
        _eventPipeline = eventPipeline;
        _config = config;
        _scheduler = scheduler;
    }

    public long[] SkillId => new long[] { 1790 };

    public async Task ExecuteAsync(IClientSession session, SkillEvent e)
    {
        if (!session.PlayerEntity.MapInstance.HasMapFlag(MapFlags.IS_MINILAND_MAP))
        {
            e.SkillInfo.IsCanceled = true;
            return;
        }

        if (e.Target is not IMateEntity mateEntity)
        {
            e.SkillInfo.IsCanceled = true;
            return;
        }

        if (mateEntity.MateType == MateType.Partner)
        {
            e.SkillInfo.IsCanceled = true;
            return;
        }

        if (mateEntity.LastDefence.AddSeconds(4) > DateTime.UtcNow)
        {
            e.SkillInfo.IsCanceled = true;
            return;
        }

        if (mateEntity.Stars == 6)
        {
            e.SkillInfo.IsCanceled = true;
            return;
        }

        MaxPetLevelConfiguration infos = _config.Configurations.FirstOrDefault(s => s.Stars == mateEntity.Stars);

        if (infos == null)
        {
            return;
        }

        if (mateEntity.HeroLevel < infos.MaxLevel)
        {
            e.SkillInfo.IsCanceled = true;
            session.SendMsgi(MessageType.Default, Game18NConstString.CanOnlyUpgradeAtMaximumTrainingLevel);
            return;
        }

        session.SendWopenPacket((byte)WindowType.EVOLVE_PET, mateEntity.PetSlot, mateEntity.MonsterVNum, mateEntity.Stars);
    }
}