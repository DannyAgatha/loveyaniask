using System.Collections.Generic;

namespace WingsEmu.Game.Buffs;

public interface IBCardStackComponent
{
    void AddStackBCard((int, byte) key, int stack);
    void RemoveStackBCard((int, byte) key);
    bool TryDecreaseBCardStack((int, byte) key);
    bool HasStackBCard((int, byte) key);
}

public class BCardStackComponent : IBCardStackComponent
{
    private readonly IDictionary<(int, byte), int> _stackBCards = new Dictionary<(int, byte), int>();
    
    public void AddStackBCard((int, byte) key, int stack)
    {
        if (!_stackBCards.TryGetValue(key, out int _))
        {
            _stackBCards.Add(key, stack);
            return;
        }

        _stackBCards[key] = stack;
    }

    public void RemoveStackBCard((int, byte) key) => _stackBCards.Remove(key);

    public bool TryDecreaseBCardStack((int, byte) key)
    {
        if (!_stackBCards.TryGetValue(key, out int stacks))
        {
            return false;
        }

        if (stacks == 0)
        {
            return false;
        }

        _stackBCards[key]--;
        return true;
    }

    public bool HasStackBCard((int, byte) key) => _stackBCards.TryGetValue(key, out int _);
}