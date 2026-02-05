using System;
using System.Collections.Generic;
using System.IO;
using MoonSharp.Interpreter;
using PhoenixLib.Logging;
using WingsAPI.Scripting;
using WingsAPI.Scripting.Attribute;
using WingsAPI.Scripting.LUA;
using WingsAPI.Scripting.Object.Timespace;
using WingsAPI.Scripting.ScriptManager;

namespace Plugin.TimeSpaces;

[ScriptObject]
public class DebuggableScriptGenerator
{
    public int TimeSpaceId { get; set; }
    public Closure GenerateMission { get; set; }
}

public class LuaTimeSpaceScriptManager : ITimeSpaceScriptManager
{
    private readonly Dictionary<long, ScriptTimeSpace> _cache = new();
    private readonly IScriptFactory _scriptFactory;
    private readonly ScriptFactoryConfiguration _scriptFactoryConfiguration;
    private readonly STimeSpaceValidator _validator;

    public LuaTimeSpaceScriptManager(IScriptFactory scriptFactory, ScriptFactoryConfiguration scriptFactoryConfiguration, STimeSpaceValidator validator)
    {
        _scriptFactory = scriptFactory;
        _scriptFactoryConfiguration = scriptFactoryConfiguration;
        _validator = validator;
    }

    public void Load()
    {
        IEnumerable<string> files = Directory.GetFiles(_scriptFactoryConfiguration.TimeSpacesDirectory, "*.lua");
        foreach (string file in files)
        {
            try
            {
                ScriptTimeSpace raid = _scriptFactory.LoadScript<ScriptTimeSpace>(file);
                if (raid == null)
                {
                    Log.Warn($"Failed to load timespace script {file}");
                    continue;
                }

                Log.Warn($"[TIMESPACE_SCRIPT_MANAGER] Loaded {Path.GetFileName(file)} for timespace: {raid.TimeSpaceId}");
                _cache[raid.TimeSpaceId] = raid;
            }
            catch (InvalidScriptException e)
            {
                Log.Error($"[TIMESPACE_SCRIPT_MANAGER] InvalidScript: {file}", e);
            }
            catch (ScriptRuntimeException e)
            {
                Log.Error($"[TIMESPACE_SCRIPT_MANAGER][SCRIPT ERROR] {file}, {e.DecoratedMessage}", e);
            }
            catch (Exception e)
            {
                Log.Error($"[TIMESPACE_SCRIPT_MANAGER][ERROR] {file}", e);
            }
        }

        Log.Info($"Loaded {_cache.Count} timespace from scripts");
    }

    public ScriptTimeSpace GetScriptedTimeSpace(long id)
    {
        if (!_cache.TryGetValue(id, out ScriptTimeSpace timeSpace))
        {
            return null;
        }

        return timeSpace;
    }
}