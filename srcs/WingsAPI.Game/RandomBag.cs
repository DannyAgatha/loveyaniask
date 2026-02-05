using System.Collections.Generic;

namespace WingsEmu.Game;

public class RandomBag<T>
{
    private readonly List<Entry> _entries = new();

    private readonly IRandomGenerator _random;
    private double _accumulator;

    public RandomBag(IRandomGenerator random) => _random = random;

    public void AddEntry(T item, double weight)
    {
        _accumulator += weight;
        _entries.Add(new Entry { Item = item, AccumulatedWeight = _accumulator });
    }

    public T GetRandom()
    {
        double rnd = _random.RandomNumber((int)_accumulator);

        foreach (Entry entry in _entries)
        {
            if (rnd >= entry.AccumulatedWeight)
            {
                continue;
            }

            return entry.Item;
        }

        return default; //should only happen when there are no entries
    }

    private struct Entry
    {
        public T Item { get; init; }
        public double AccumulatedWeight { get; init; }
    }
}