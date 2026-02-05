using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FluentValidation.Results;
using MoonSharp.Interpreter;
using PhoenixLib.Logging;
using Plugin.Raids.Scripting.Validator.Raid;
using WingsAPI.Scripting;
using WingsAPI.Scripting.Enum.Raid;
using WingsAPI.Scripting.LUA;
using WingsAPI.Scripting.Object.Raid;
using WingsAPI.Scripting.ScriptManager;

namespace Plugin.Raids.Scripting;

public sealed class RaidScriptManager : IRaidScriptManager
{
    private readonly Dictionary<SRaidType, SRaid> _cache = new();
    private readonly IScriptFactory _scriptFactory;
    private readonly ScriptFactoryConfiguration _scriptFactoryConfiguration;
    private readonly SRaidValidator _validator;

    public RaidScriptManager(IScriptFactory scriptFactory, ScriptFactoryConfiguration scriptFactoryConfiguration, SRaidValidator validator)
    {
        _scriptFactory = scriptFactory;
        _scriptFactoryConfiguration = scriptFactoryConfiguration;
        _validator = validator;
    }

    public SRaid GetScriptedRaid(SRaidType raidType) => _cache.GetValueOrDefault(raidType);

    public void Load()
    {
        IEnumerable<string> files = Directory.GetFiles(_scriptFactoryConfiguration.RaidsDirectory, "*.lua");
        foreach (string file in files)
        {
            try
            {
                SRaid raid = _scriptFactory.LoadScript<SRaid>(file);
                if (raid == null)
                {
                    Log.Warn($"Failed to load raid script {file}");
                    continue;
                }

                ValidationResult result = _validator.Validate(raid);
                if (!result.IsValid)
                {
                    throw new InvalidScriptException(result.Errors.First().ErrorMessage);
                }

                Log.Warn($"[RAID_SCRIPT_MANAGER] Loaded {Path.GetFileName(file)} for raid: {raid.RaidType}");
                _cache[raid.RaidType] = raid;
            }
            catch (InvalidScriptException e)
            {
                Log.Error($"[RAID_SCRIPT_MANAGER]InvalidScript: {file}", e);
            }
            catch (ScriptRuntimeException e)
            {
                Log.Error($"[RAID_SCRIPT_MANAGER][SCRIPT ERROR] {file}, {e.DecoratedMessage}", e);
            }
            catch (Exception e)
            {
                Log.Error($"[RAID_SCRIPT_MANAGER][SCRIPT ERROR] {file}", e);
            }
        }

        Log.Info($"Loaded {_cache.Count} raids from scripts");
    }
}