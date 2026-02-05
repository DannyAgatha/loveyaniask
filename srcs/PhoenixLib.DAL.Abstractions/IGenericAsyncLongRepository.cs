// NosEmu
// 


namespace PhoenixLib.DAL
{
    /// <summary>
    ///     IGenericAsyncLongRepository permits to manage specialize an IGenericAsyncRepository for ILongDto objects (which are
    ///     objects with an long key Id)
    /// </summary>
    /// <typeparam name="TDto"></typeparam>
    public interface IGenericAsyncLongRepository<TDto> : IGenericAsyncRepository<TDto, long>
    where TDto : class, ILongDto
    {
    }
}