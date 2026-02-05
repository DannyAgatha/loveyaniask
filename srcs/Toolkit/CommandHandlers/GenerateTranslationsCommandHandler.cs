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

public class GenerateTranslationsCommandHandler
{
    private static readonly INamingConvention __NamingConvention = UnderscoredNamingConvention.Instance;
    private static readonly ISerializer __Serializer = new SerializerBuilder().WithNamingConvention(__NamingConvention).Build();
    private static readonly IDeserializer __Deserializer = new DeserializerBuilder().WithNamingConvention(__NamingConvention).Build();

    public static async Task<int> HandleAsync(GenerateTranslationsCommand command)
    {
        Log.Info($"Updating translations file in {command.InputPath}");
        Dictionary<string, string> english = null;
        foreach (RegionLanguageType i in Enum.GetValues(typeof(RegionLanguageType)))
        {
            if (i == RegionLanguageType.RU)
            {
                continue;
            }

            var newTmp = new Dictionary<GameDialogKey, string>();

            foreach (GameDialogKey enumValue in Enum.GetValues(typeof(GameDialogKey)))
            {
                GameDialogKey enm = enumValue;
                string notTranslated = $"#{enm.ToString()}";
                if (newTmp.ContainsKey(enm))
                {
                    continue;
                }


                newTmp[enm] = notTranslated;
            }

            string languageType = i.ToString();
            string fileContent = await File.ReadAllTextAsync($"{command.InputPath}/{languageType.ToLowerInvariant()}/game-dialog-key.yaml");
            IDeserializer deserializer = __Deserializer;
            Dictionary<string, string> tmp = deserializer.Deserialize<Dictionary<string, string>>(fileContent);

            foreach ((string s, string value) in tmp)
            {
                if (!Enum.TryParse(s, out GameDialogKey key))
                {
                    continue;
                }

                if (string.IsNullOrEmpty(value))
                {
                    continue;
                }

                if (value == $"#{s}" && english != null && english.TryGetValue(s, out string translated))
                {
                    newTmp[key] = translated;
                    continue;
                }

                newTmp[key] = value;
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

        return 0;
    }
}