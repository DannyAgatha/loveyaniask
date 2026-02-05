using System;

namespace PhoenixLib.Caching
{
    public interface IUuidKeyCachedRepository<T> : ICachedRepository<Guid, T>
    {
    }
}