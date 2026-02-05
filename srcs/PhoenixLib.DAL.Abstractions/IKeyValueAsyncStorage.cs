// NosEmu
// 


using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PhoenixLib.DAL
{
    public interface IKeyValueAsyncStorage<TObject, TKey>
    where TKey : notnull
    {
        /// <summary>
        ///     Gets all the objects stored within the cache
        /// </summary>
        /// <returns></returns>
        Task<IEnumerable<TObject>> GetAllAsync();

        /// <summary>
        ///     Clears cache
        /// </summary>
        /// <returns></returns>
        Task ClearAllAsync();


        /// <summary>
        ///     Gets all the objects that are contained in the given id enumerable
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        Task<IEnumerable<TObject>> GetByIdsAsync(IEnumerable<TKey> ids);

        /// <summary>
        ///     Gets the object with the given key from the cache
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Task<TObject> GetByIdAsync(TKey id);

        /// <summary>
        ///     Registers the object that are contained
        /// </summary>
        /// <param name="id"></param>
        /// <param name="obj"></param>
        /// <param name="lifeTime"></param>
        /// <returns></returns>
        Task RegisterAsync(TKey id, TObject obj, TimeSpan? lifeTime = null);

        /// <summary>
        ///     Asynchronously registers all the objects given as parameter assuming that these objects contains a key
        /// </summary>
        /// <param name="objs"></param>
        /// <param name="lifeTime"></param>
        Task RegisterAsync(IEnumerable<(TKey, TObject)> objs, TimeSpan? lifeTime = null);

        /// <summary>
        ///     Asynchronously removes the object and returns it
        /// </summary>
        /// <param name="id"></param>
        Task<TObject> RemoveAsync(TKey id);

        /// <summary>
        ///     Asynchronously removes the objects with the given keys
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        Task<IEnumerable<TObject>> RemoveAsync(IEnumerable<TKey> ids);
    }
}