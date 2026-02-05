using System;
using System.Collections.Generic;
using WingsEmu.Game.Characters;
using WingsEmu.Game.Helpers.Damages;

namespace WingsEmu.Game._ECS.Systems;

public interface ICharacterSystem
{
    IPlayerEntity GetCharacterById(long id);
    IReadOnlyList<IPlayerEntity> GetCharacters();
    IReadOnlyList<IPlayerEntity> GetCharacters(Func<IPlayerEntity, bool> predicate);
    IReadOnlyList<IPlayerEntity> GetCharactersInRange(Position pos, short distance, Func<IPlayerEntity, bool> predicate);
    IReadOnlyList<IPlayerEntity> GetCharactersInRange(Position pos, short distance);
    IReadOnlyList<IPlayerEntity> GetClosestCharactersInRange(Position pos, short distance);
    IReadOnlyList<IPlayerEntity> GetAliveCharacters();
    IReadOnlyList<IPlayerEntity> GetAliveCharacters(Func<IPlayerEntity, bool> predicate);
    IReadOnlyList<IPlayerEntity> GetAliveCharactersInRange(Position pos, short distance, Func<IPlayerEntity, bool> predicate);
    IReadOnlyList<IPlayerEntity> GetAliveCharactersInRange(Position pos, short distance);
    void AddCharacter(IPlayerEntity character);
    void RemoveCharacter(IPlayerEntity entity);
}