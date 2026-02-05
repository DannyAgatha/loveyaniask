using System.Collections.Generic;
using System.Threading.Tasks;

namespace PhoenixLib.MultiLanguage
{
    public interface ILanguageService<T> where T : notnull
    {
        /// <summary>
        ///     Will return the string by its key & region
        ///     Used for ChickenAPI mainly
        /// </summary>
        /// <param name="key"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        string GetLanguage(T key, RegionLanguageType type);

        /// <summary>
        ///     Will return the string by its key & region
        /// </summary>
        /// <param name="key"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        Task<string> GetLanguageAsync(T key, RegionLanguageType type);

        /// <summary>
        ///     Will return the string by its key & region
        /// </summary>
        /// <param name="key"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        Task<IDictionary<T, string>> GetLanguageAsync(ICollection<T> key, RegionLanguageType type);

        /// <summary>
        ///     Will register the key and value by its region type
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="type"></param>
        void SetLanguage(T key, string value, RegionLanguageType type);

        /// <summary>
        ///     Will register the key and value by its region type
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="type"></param>
        Task SetLanguageAsync(T key, string value, RegionLanguageType type);

        /// <summary>
        ///     Will register the key and value by its region type
        /// </summary>
        /// <param name="keyValues"></param>
        /// <param name="type"></param>
        Task SetLanguageAsync(IDictionary<T, string> keyValues, RegionLanguageType type);
    }
}