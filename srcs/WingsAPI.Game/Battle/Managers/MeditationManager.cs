using System;
using System.Collections.Generic;
using System.Linq;
using WingsEmu.Game.Entities;

namespace WingsEmu.Game.Battle;

public class MeditationManager : IMeditationManager
{
    private readonly Dictionary<long, List<(short, DateTime)>> _savedMeditationCaster = new();


    public void SaveMeditation(IBattleEntity caster, short meditationId, DateTime dateTime)
    {
        if (_savedMeditationCaster.ContainsKey(caster.Id))
        {
            _savedMeditationCaster[caster.Id].Add((meditationId, dateTime));
        }
        else
        {
            _savedMeditationCaster.Add(caster.Id, new List<(short, DateTime)> { (meditationId, dateTime) });
        }
    }

    public DateTime GetCastTime(IBattleEntity caster, short meditationId) => _savedMeditationCaster[caster.Id].FirstOrDefault(s => s.Item1 == meditationId).Item2;

    public List<(short, DateTime)> GetAllMeditations(IBattleEntity caster) => _savedMeditationCaster[caster.Id];

    public void RemoveAllMeditation(IBattleEntity caster)
    {
        _savedMeditationCaster.Remove(caster.Id);
    }

    public void RemoveMeditation(IBattleEntity caster, short meditationId)
    {
        foreach ((short, DateTime) s in _savedMeditationCaster[caster.Id].ToList())
        {
            if (s.Item1 == meditationId)
            {
                _savedMeditationCaster[caster.Id].Remove(s);
            }
        }
    }

    public bool HasMeditation(IBattleEntity caster) => _savedMeditationCaster.ContainsKey(caster.Id);
}