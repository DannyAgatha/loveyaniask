namespace NosEmu.Plugins.BasicImplementations.Factories;

public interface IGameObjectFactory<out TGameObject, in TDto>
{
    TGameObject CreateGameObject(TDto dto);
}