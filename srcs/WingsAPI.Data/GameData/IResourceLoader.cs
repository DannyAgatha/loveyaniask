using System.Collections.Generic;
using System.Threading.Tasks;
using PhoenixLib.MultiLanguage;
using ProtoBuf;
using WingsEmu.Game._i18n;

namespace WingsAPI.Data.GameData;

public interface IResourceLoader<T>
{
    Task<IReadOnlyList<T>> LoadAsync();
}

public class GameDataTranslationDto
{
    public GameDataType DataType { get; set; }
    public RegionLanguageType Language { get; set; }
    public string Key { get; set; }
    public string Value { get; set; }
}

[ProtoContract]
public class GenericTranslationDto
{
    [ProtoMember(1)]
    public RegionLanguageType Language { get; set; }

    [ProtoMember(2)]
    public string Key { get; set; }

    [ProtoMember(3)]
    public string Value { get; set; }
}