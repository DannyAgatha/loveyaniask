using System;

namespace WingsEmu.Game._ECS;

public interface IMapSystem
{
    string Name { get; }


    /// <summary>
    /// </summary>
    /// <param name="date"></param>
    /// <param name="isTickRefresh"></param>
    void ProcessTick(DateTime date, bool isTickRefresh = false);

    void PutIdleState();
    void Clear();
}