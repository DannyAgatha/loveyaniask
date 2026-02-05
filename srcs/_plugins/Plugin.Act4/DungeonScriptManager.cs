using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FluentValidation.Results;
using MoonSharp.Interpreter;
using PhoenixLib.Logging;
using Plugin.Act4.Scripting.Validator;
using WingsAPI.Scripting;
using WingsAPI.Scripting.Enum.Dungeon;
using WingsAPI.Scripting.LUA;
using WingsAPI.Scripting.Object.Dungeon;
using WingsAPI.Scripting.ScriptManager;

namespace Plugin.Act4;

public class DungeonScriptManager : IDungeonScriptManager
{
    private readonly Dictionary<SDungeonType, SDungeon> _cache = new();
    private readonly IScriptFactory _scriptFactory;
    private readonly ScriptFactoryConfiguration _scriptFactoryConfiguration;
    private readonly SDungeonValidator _validator;

    public DungeonScriptManager(IScriptFactory scriptFactory, ScriptFactoryConfiguration scriptFactoryConfiguration, SDungeonValidator validator)
    {
        _scriptFactory = scriptFactory;
        _scriptFactoryConfiguration = scriptFactoryConfiguration;
        _validator = validator;
    }

    public SDungeon GetScriptedDungeon(SDungeonType raidType) => _cache.GetValueOrDefault(raidType);

    public void Load()
    {
        IEnumerable<string> files = Directory.GetFiles(_scriptFactoryConfiguration.DungeonsDirectory, "*.lua");
        foreach (string file in files)
        {
            try
            {
                SDungeon dungeon = _scriptFactory.LoadScript<SDungeon>(file);
                if (dungeon == null)
                {
                    Log.Warn($"Failed to load raid script {file}");
                    continue;
                }

                ValidationResult result = _validator.Validate(dungeon);
                if (!result.IsValid)
                {
                    throw new InvalidScriptException(result.Errors.First().ErrorMessage);
                }

                Log.Warn($"[DUNGEON_SCRIPT_MANAGER] Loaded {Path.GetFileName(file)} for raid: {dungeon.DungeonType}");
                _cache[dungeon.DungeonType] = dungeon;
            }
            catch (InvalidScriptException e)
            {
                Log.Error($"[DUNGEON_SCRIPT_MANAGER][SCRIPT_ERROR] InvalidScript: {file}", e);
            }
            catch (ScriptRuntimeException e)
            {
                Log.Error($"[DUNGEON_SCRIPT_MANAGER][SCRIPT ERROR] {file}, {e.DecoratedMessage}", e);
            }
            catch (Exception e)
            {
                Log.Error($"[DUNGEON_SCRIPT_MANAGER][SCRIPT_ERROR] {file}", e);
            }
        }

        Log.Info($"Loaded {_cache.Count} dungeons from scripts");
    }
}