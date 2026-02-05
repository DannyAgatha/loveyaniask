namespace PhoenixLib.DAL
{
    /// <summary>
    ///     IGenericAsyncLongRepository permits to manage specialize an IGenericAsyncRepository for ILongDto objects (which are
    ///     objects with an long key Id)
    /// </summary>
    /// <typeparam name="TDto"></typeparam>
    public interface IGenericAsyncIntRepository<TDto> : IGenericAsyncRepository<TDto, int>
    where TDto : class, IIntDto
    {
    }
}