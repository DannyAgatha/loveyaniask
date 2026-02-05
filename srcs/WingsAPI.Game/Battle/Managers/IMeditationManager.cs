using System;
using System.Collections.Generic;
using WingsEmu.Game.Entities;

namespace WingsEmu.Game.Battle;

public interface IMeditationManager
{
    void SaveMeditation(IBattleEntity caster, short meditationId, DateTime dateTime);
    DateTime GetCastTime(IBattleEntity caster, short meditationId);
    List<(short, DateTime)> GetAllMeditations(IBattleEntity caster);
    void RemoveMeditation(IBattleEntity caster, short meditationId);
    void RemoveAllMeditation(IBattleEntity caster);
    bool HasMeditation(IBattleEntity caster);
}