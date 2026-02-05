using WingsEmu.Game.Battle;

namespace WingsEmu.Game.Skills;

public interface IAngelElementBuffComponent
{
    public ElementType? AngelElement { get; }
    public void AddAngelElement(ElementType elementType);
    public void RemoveAngelElement();
}