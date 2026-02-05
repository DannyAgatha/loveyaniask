namespace PhoenixLib.MultiLanguage
{
    /// <summary>
    ///     Permits multi language key/value based on a string key
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IStringBasedLanguageService<T> : ILanguageService<string>
    {
    }
}