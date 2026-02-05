using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using PhoenixLib.Logging;
using PhoenixLib.MultiLanguage;
using Toolkit.Commands;
using WingsEmu.Game._i18n;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Toolkit.CommandHandlers;

public class CheckTranslationsCommandHandler
{
    private static readonly INamingConvention __NamingConvention = UnderscoredNamingConvention.Instance;
    private static readonly ISerializer __Serializer = new SerializerBuilder().WithNamingConvention(__NamingConvention).Build();
    private static readonly IDeserializer __Deserializer = new DeserializerBuilder().WithNamingConvention(__NamingConvention).Build();

    public static async Task<int> HandleAsync(CheckTranslationsCommand command)
    {
        Log.Info($"Checking translations file in {command.InputPath}");
        Dictionary<string, string> english = null;
        foreach (RegionLanguageType i in Enum.GetValues(typeof(RegionLanguageType)))
        {
            if (i == RegionLanguageType.RU)
            {
                continue;
            }

            string languageType = i.ToString().ToLowerInvariant();
            var newTmp = new Dictionary<string, string>();

            string languageDirectory = Path.Combine(command.InputPath, $"{languageType}");
            foreach (string translationFile in Directory.GetFiles(languageDirectory, "*.yml").Concat(Directory.GetFiles(languageDirectory, "*.yaml")))
            {
                try
                {
                    string fileContent = await File.ReadAllTextAsync(translationFile);
                    IDeserializer deserializer = __Deserializer;
                    Dictionary<string, string> tmp = deserializer.Deserialize<Dictionary<string, string>>(fileContent);

                    foreach ((string s, string value) in tmp)
                    {
                        if (string.IsNullOrEmpty(value))
                        {
                            continue;
                        }

                        if (value == $"#{s}" && english != null && english.TryGetValue(s, out string translated))
                        {
                            newTmp[s] = translated;
                            continue;
                        }

                        newTmp[s] = value;
                    }

                    if (i == RegionLanguageType.EN)
                    {
                        english = newTmp;
                    }

                    foreach (object enumValue in Enum.GetValues(typeof(GameDialogKey)))
                    {
                        var enm = (GameDialogKey)enumValue;
                        if (!newTmp.ContainsKey(enm.ToString()))
                        {
                            Log.Warn($"{translationFile}: {enm.ToString()} is missing");
                            continue;
                        }

                        newTmp[enm.ToString()] = $"#{enm.ToString()}";
                    }

                    var toSerialize = newTmp
                        .OrderBy(s => s.Key.ToString())
                        .ToDictionary(s => s.Key.ToString(), s => s.Value);

                    if (i == RegionLanguageType.EN)
                    {
                        english = toSerialize;
                    }

                    ISerializer serializer = __Serializer;
                    string content = serializer.Serialize(toSerialize);
                    string outputFile = $"{command.OutputPath}/{languageType.ToLowerInvariant()}/game-dialog-key.yaml";
                    Log.Info($"Updating translation in {outputFile}");
                    await File.WriteAllTextAsync(outputFile, content);
                }
                catch (Exception e)
                {
                    Log.Error($"[RESOURCE_LOADER] {translationFile} {languageType}", e);
                }
            }
        }

        return 0;
    }
}