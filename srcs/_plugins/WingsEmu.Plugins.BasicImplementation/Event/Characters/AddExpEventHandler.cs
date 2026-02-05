using System.Threading;
using System.Threading.Tasks;
using PhoenixLib.Events;
using WingsEmu.Game.Algorithm;
using WingsEmu.Game.Characters;
using WingsEmu.Game.Characters.Events;
using WingsEmu.Game.Configurations;
using WingsEmu.Game.Extensions;
using WingsEmu.Packets.Enums;
using WingsEmu.Packets.Enums.Character;

namespace NosEmu.Plugins.BasicImplementations.Quests;

public class AddExpEventHandler : IAsyncEventProcessor<AddExpEvent>
{
    private readonly ICharacterAlgorithm _characterAlgorithm;
    private readonly GameMinMaxConfiguration _gameMinMaxConfiguration;

    public AddExpEventHandler(ICharacterAlgorithm characterAlgorithm, GameMinMaxConfiguration gameMinMaxConfiguration)
    {
        _characterAlgorithm = characterAlgorithm;
        _gameMinMaxConfiguration = gameMinMaxConfiguration;
    }

    public async Task HandleAsync(AddExpEvent e, CancellationToken cancellation)
    {
        IPlayerEntity character = e.Sender.PlayerEntity;
        long exp = e.Exp;
        LevelType levelType = e.LevelType;

        switch (levelType)
        {
            case LevelType.Level:
                HandleXp(character, exp);
                break;
            case LevelType.JobLevel:
                HandleJobXp(character, exp);
                break;
            case LevelType.SpJobLevel:
                HandleSpJobXp(character, exp);
                break;
            case LevelType.Heroic:
                HandleHeroicXp(character, exp);
                break;
        }

        character.Session.RefreshStatChar();
        character.Session.RefreshStat();
        character.Session.RefreshLevel(_characterAlgorithm);
    }

    private void HandleXp(IPlayerEntity character, long exp)
    {
        if (character.Level >= _gameMinMaxConfiguration.MaxLevel)
        {
            return;
        }

        character.LevelXp += exp;
        if (character.LevelXp < _characterAlgorithm.GetLevelXp(character.Level))
        {
            return;
        }

        character.Session.EmitEventAsync(new LevelUpEvent { LevelType = LevelType.Level });
    }

    private void HandleJobXp(IPlayerEntity character, long exp)
    {
        if (character.Class == ClassType.Adventurer && character.JobLevel > 19)
        {
            return;
        }

        if (character.JobLevel >= _gameMinMaxConfiguration.MaxJobLevel)
        {
            return;
        }

        character.JobLevelXp += exp;
        if (character.JobLevelXp < _characterAlgorithm.GetJobXp(character.JobLevel))
        {
            return;
        }

        character.Session.EmitEventAsync(new LevelUpEvent { LevelType = LevelType.JobLevel });
    }

    private void HandleSpJobXp(IPlayerEntity character, long exp)
    {
        if (character.Specialist == null)
        {
            return;
        }

        if (character.Specialist.SpLevel >= _gameMinMaxConfiguration.MaxSpLevel)
        {
            return;
        }

        character.Specialist.Xp += exp;
        if (character.Specialist.Xp < _characterAlgorithm.GetSpecialistJobXp(character.Specialist.SpLevel, character.Specialist.IsFunSpecialist()))
        {
            return;
        }

        character.Session.EmitEventAsync(new LevelUpEvent { LevelType = LevelType.SpJobLevel });
    }

    private void HandleHeroicXp(IPlayerEntity character, long exp)
    {
        if (character.HeroLevel >= _gameMinMaxConfiguration.MaxHeroLevel)
        {
            return;
        }

        character.HeroXp += exp;
        if (character.LevelXp < _characterAlgorithm.GetHeroLevelXp(character.HeroLevel))
        {
            return;
        }

        character.Session.EmitEventAsync(new LevelUpEvent { LevelType = LevelType.Heroic });
    }
}