using WingsEmu.Game.Battle;

namespace WingsEmu.Game.Skills;

public class AngelElementBuffComponent : IAngelElementBuffComponent
{
    public ElementType? AngelElement { get; private set; }

    public void AddAngelElement(ElementType elementType)
    {
        AngelElement = elementType;
    }

    public void RemoveAngelElement()
    {
        AngelElement = null;
    }
}