using PhoenixLib.Events;
using PhoenixLib.Scheduler;
using System;
using System.Linq;
using System.Threading.Tasks;
using WingsAPI.Packets.Enums.Chat;
using WingsEmu.Core.Extensions;
using WingsEmu.DTOs.Maps;
using WingsEmu.Game._enum;
using WingsEmu.Game._i18n;
using WingsEmu.Game.Configurations;
using WingsEmu.Game.Core.SkillHandling;
using WingsEmu.Game.Core.SkillHandling.Event;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Managers;
using WingsEmu.Game.Mates;
using WingsEmu.Game.Networking;
using WingsEmu.Packets.Enums;
using WingsEmu.Packets.Enums.Battle;
using WingsEmu.Packets.Enums.Chat;

namespace NosEmu.Plugins.BasicImplementations.Skills;

public class ExtractEssenceSkillHandler : ISkillHandler
{
    private readonly PetMaxLevelConfiguration _config;
    private readonly IScheduler _scheduler;
    private readonly IGameLanguageService _gameLanguageService;

    public ExtractEssenceSkillHandler(PetMaxLevelConfiguration config, IScheduler scheduler, IDelayManager delayManager,
        IGameLanguageService gameLanguageService)
    {
        _config = config;
        _scheduler = scheduler;
        _gameLanguageService = gameLanguageService;
    }

    public long[] SkillId => new long[] { 1789 };

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

        if (mateEntity.LastDefence.AddSeconds(4) >= DateTime.UtcNow)
        {
            e.SkillInfo.IsCanceled = true;
            session.SendMsg(_gameLanguageService.GetLanguage(GameDialogKey.PET_IS_FIGHTING, session.UserLanguage), MsgMessageType.Middle);
            return;
        }

        if (mateEntity.Stars == 6)
        {
            e.SkillInfo.IsCanceled = true;
            return;
        }

        if (mateEntity.MateType == MateType.Partner)
        {
            return;
        }

        MaxPetLevelConfiguration infos = _config.Configurations.FirstOrDefault(s => s.Stars == mateEntity.Stars);

        if (infos == null)
        {
            e.SkillInfo.IsCanceled = true;
            return;
        }

        if (mateEntity.HeroLevel < infos.MaxLevel)
        {
            session.SendMsgi(MessageType.Default, Game18NConstString.CanOnlyExtractAtMaximumTrainingLevel);
            e.SkillInfo.IsCanceled = true;
            return;
        }

        _scheduler.Schedule(TimeSpan.FromMilliseconds(e.SkillInfo.CastTime * 100), async s =>
        {
            session.SendQnaiPacket($"guri 451 {mateEntity.PetSlot} {mateEntity.MonsterVNum} {mateEntity.Stars}", Game18NConstString.ConfirmExtractEssence);
        });
    }
}