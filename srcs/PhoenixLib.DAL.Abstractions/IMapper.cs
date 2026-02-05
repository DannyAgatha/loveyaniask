// NosEmu
// 


using System.Collections.Generic;

namespace PhoenixLib.DAL
{
    /// <summary>
    ///     IMapper facilitate mapping between an entity and a dto
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    /// <typeparam name="TDto"></typeparam>
    public interface IMapper<TEntity, TDto>
    {
        TEntity Map(TDto input);
        List<TEntity> Map(List<TDto> input);
        IEnumerable<TEntity> Map(IEnumerable<TDto> input);
        IReadOnlyList<TEntity> Map(IReadOnlyList<TDto> input);
        TDto Map(TEntity input);
        List<TDto> Map(List<TEntity> input);
        IEnumerable<TDto> Map(IEnumerable<TEntity> input);
        IReadOnlyList<TDto> Map(IReadOnlyList<TEntity> input);
        void Map(TDto input, TEntity output);
        void Map(TEntity input, TDto output);
    }
}