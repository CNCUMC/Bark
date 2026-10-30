using System;
using System.IO;
using System.Text;
using Bark.Event;
using Bark.Items;
using Bark.ScriptApi;
using Bark.Tile;
using Bark.Tool;
using Puerts;

namespace Bark.Script;

public class PuerJavaScript : ScriptEngine
{
    private bool _isLoaded;
    private ScriptEnv? _scriptEnv;

    public override bool Load(ScriptManifest manifest)
    {
        base.Load(manifest);

        try
        {
            _scriptEnv = new ScriptEnv(new BackendV8());

            InjectBarkApi();

            var script = File.ReadAllText(Manifest.EntryFile);

            foreach (var (name, line) in ScriptApiGuard.FindReservedOverrides(script,
                ScriptLanguage.JavaScript))
                LogUtil.Warning("script_engine.reserved_override", Manifest.Id, name,
                    line);

            ScriptCallContext.CurrentModId = Manifest.Id;
            try
            {
                _scriptEnv.Eval(script);
            }
            finally
            {
                ScriptCallContext.CurrentModId = null;
            }

            _isLoaded = true;

            CallLifecycleHook("onLoad");
        }
        catch (Exception ex)
        {
            LogUtil.Warning("script_engine.js_load_failed", Manifest.Id, ex.ToString());
            Dispose();
            return false;
        }

        return true;
    }

    private void InjectBarkApi()
    {
        if (_scriptEnv == null) return;

        var id = EscapeString(Manifest.Id);
        var version = EscapeString(Manifest.Version);
        var scriptName = EscapeString(Manifest.Name);

        var sb = new StringBuilder();

        foreach (var (name, _) in ApiRegistry.Proxies)
            sb.AppendLine(
                $"var {name} = CS.Bark.ScriptApi.ApiRegistry.GetProxy('{name}');");

        sb.AppendLine(
            $"var logApi = new CS.Bark.ScriptApi.LogApi('{scriptName}', '{id}');");
        sb.AppendLine("var Log = logApi;");
        sb.AppendLine("var Locale = logApi.Locale;");
        sb.AppendLine(
            $"var ScriptInfo = {{ Id: '{id}', Version: '{version}', Name: '{scriptName}' }};");

        _scriptEnv.Eval(sb.ToString());
    }

    private void CallLifecycleHook(string hookName)
    {
        if (_scriptEnv == null) return;

        ScriptCallContext.CurrentModId = Manifest.Id;
        try
        {
            _scriptEnv.Eval($"if (typeof {hookName} === 'function') {{ {hookName}(); }}");
        }
        catch (Exception ex)
        {
            LogUtil.Warning("script_mod_loader.hook_failed", Manifest.Id, hookName,
                ex.Message);
        }
        finally
        {
            ScriptCallContext.CurrentModId = null;
        }
    }

    public override void Enable()
    {
        if (!_isLoaded) return;
        CallLifecycleHook("onEnable");
    }

    public override void Disable()
    {
        if (!_isLoaded) return;
        CallLifecycleHook("onDisable");
    }

    public override void Unload()
    {
        if (!_isLoaded) return;
        CallLifecycleHook("onUnload");
        Dispose();
    }

    public override void CallTriggerEvent(string eventName, BarkEvent? eventData = null)
    {
        if (_scriptEnv == null) return;

        ScriptCallContext.CurrentModId = Manifest.Id;
        try
        {
            if (eventData != null)
            {
                EventScriptContext.CurrentEvent = eventData;
                _scriptEnv.Eval(
                    "var __barkEvent = CS.Bark.Script.EventScriptContext.CurrentEvent;");
            }
            else
            {
                _scriptEnv.Eval("var __barkEvent = null;");
            }

            _scriptEnv.Eval(
                $"if (typeof {eventName} === 'function') {{ {eventName}(__barkEvent); }}");
        }
        catch (Exception ex)
        {
            LogUtil.Warning("script_mod_loader.hook_failed", Manifest.Id, eventName,
                ex.Message);
        }
        finally
        {
            ScriptCallContext.CurrentModId = null;
            EventScriptContext.CurrentEvent = null;
        }
    }

    public override void ExecuteItemFile(string filePath, string? itemId,
        Item? item = null, string? action = null)
    {
        if (_scriptEnv == null || !File.Exists(filePath)) return;

        ItemScriptContext.CurrentItem = item;
        ItemScriptContext.CurrentAction = action;
        ScriptCallContext.CurrentModId = Manifest.Id;

        try
        {
            var escapedId = itemId != null
                ? EscapeString(itemId)
                : "null";
            _scriptEnv.Eval($"var __barkItemId = '{escapedId}';");
            _scriptEnv.Eval(
                "var __barkItem = CS.Bark.Items.ItemScriptContext.CurrentItem;");
            _scriptEnv.Eval(
                "var __barkAction = CS.Bark.Items.ItemScriptContext.CurrentAction;");
            _scriptEnv.Eval(
                "var __barkTriggerValue = CS.Bark.Items.ItemScriptContext.CurrentTriggerValue;");
            _scriptEnv.Eval(
                "var __barkTriggerThreshold = CS.Bark.Items.ItemScriptContext.CurrentTriggerThreshold;");
            _scriptEnv.Eval(
                "var __barkTriggerOperator = CS.Bark.Items.ItemScriptContext.CurrentTriggerOperator;");

            var script = File.ReadAllText(filePath);

            foreach (var (name, line) in ScriptApiGuard.FindReservedOverrides(script,
                ScriptLanguage.JavaScript))
                LogUtil.Warning("script_engine.reserved_override", Manifest.Id, name,
                    line);

            _scriptEnv.Eval(script);

            _scriptEnv.Eval(
                "if (typeof main === 'function') { main(__barkItemId, __barkItem, __barkAction, __barkTriggerValue, __barkTriggerThreshold, __barkTriggerOperator); }");
        }
        catch (Exception ex)
        {
            LogUtil.Warning("script_engine.js_exec_file_failed", Manifest.Id, filePath,
                ex.Message);
        }
        finally
        {
            ScriptCallContext.CurrentModId = null;
            ItemScriptContext.CurrentItem = null;
            ItemScriptContext.CurrentAction = null;
        }
    }

    public override void ExecuteTileFile(string filePath, string? tileId,
        TileScriptContext? context = null,
        string? action = null)
    {
        if (_scriptEnv == null || !File.Exists(filePath)) return;

        TileScriptContext.CurrentContext = context;
        TileScriptContext.CurrentAction = action;
        ScriptCallContext.CurrentModId = Manifest.Id;

        try
        {
            var escapedId = tileId != null
                ? EscapeString(tileId)
                : "null";
            _scriptEnv.Eval($"var __barkTileId = '{escapedId}';");
            _scriptEnv.Eval(
                "var __barkTileContext = CS.Bark.Tile.TileScriptContext.CurrentContext;");
            _scriptEnv.Eval(
                "var __barkAction = CS.Bark.Tile.TileScriptContext.CurrentAction;");

            var script = File.ReadAllText(filePath);

            foreach (var (name, line) in ScriptApiGuard.FindReservedOverrides(script,
                ScriptLanguage.JavaScript))
                LogUtil.Warning("script_engine.reserved_override", Manifest.Id, name,
                    line);

            _scriptEnv.Eval(script);

            _scriptEnv.Eval(
                "if (typeof main === 'function') { main(__barkTileId, __barkTileContext, __barkAction); }");
        }
        catch (Exception ex)
        {
            LogUtil.Warning("script_engine.js_exec_file_failed", Manifest.Id, filePath,
                ex.Message);
        }
        finally
        {
            ScriptCallContext.CurrentModId = null;
            TileScriptContext.CurrentContext = null;
            TileScriptContext.CurrentAction = null;
        }
    }

    public override void CallUpdate()
    {
        if (_scriptEnv == null || !_isLoaded) return;

        ScriptCallContext.CurrentModId = Manifest.Id;
        try
        {
            _scriptEnv.Eval("if (typeof onUpdate === 'function') { onUpdate(); }");
        }
        catch
        {
            // ignored
        }
        finally
        {
            ScriptCallContext.CurrentModId = null;
        }
    }

    public override void Dispose()
    {
        if (_scriptEnv != null)
        {
            try
            {
                _scriptEnv.Dispose();
            }
            catch (Exception ex)
            {
                LogUtil.Warning("script_engine.js_dispose_error", Manifest.Id,
                    ex.Message);
            }

            _scriptEnv = null;
        }

        _isLoaded = false;
    }

    private static string EscapeString(string value)
    {
        return value
            .Replace("\\", @"\\")
            .Replace("'", @"\'")
            .Replace("\n", @"\n")
            .Replace("\r", @"\r");
    }
}
