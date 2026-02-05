namespace PhoenixLib.Caching
{
    public interface IKeyValueCache<T> : ICachedRepository<string, T>
    {
    }
}