namespace PhoenixLib.DAL.EFCore.PGSQL
{
    public interface IGenericStringRepository<TEntity> : IGenericAsyncRepository<TEntity, string>
    where TEntity : class, IStringKeyEntity, new()
    {
    }
}