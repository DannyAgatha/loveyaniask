using System;

namespace WingsEmu.Game._ECS;

public interface ITickProcessable
{
    Guid Id { get; }
    string Name { get; }

    /// <summary>
    /// </summary>
    /// <param name="date"></param>
    void ProcessTick(DateTime date);
}