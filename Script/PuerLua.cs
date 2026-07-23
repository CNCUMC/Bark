using System;
using System.IO;
using Puerts;

namespace Bark.Script;

// PuerTS Lua 引擎包装器，管理脚本模组的生命周期
// 不依赖 Unity GameObject，避免场景切换时被意外销毁
public class PuerLua : ScriptEngine
{
    private bool _isLoaded;
    private ScriptManifest _manifest = null!;
    private ScriptEnv? _scriptEnv;

    // 加载并执行 Lua 模组，返回是否成功
    public override bool Load(ScriptManifest manifest)
    {
        _manifest = manifest;

        try
        {
            // 创建 Lua 引擎实例
            _scriptEnv = new ScriptEnv(new BackendLua());

            // 注入 bark.* API
            InjectBarkApi();

            // 执行入口脚本
            var script = File.ReadAllText(manifest.EntryFile);
            _scriptEnv.Eval(script);

            _isLoaded = true;

            // 调用 onLoad 生命周期钩子
            CallLifecycleHook("onLoad");
        }
        catch (Exception ex)
        {
            Plugin.Logger.LogWarning($"[Bark] Lua Load FAILED | id={manifest.Id} | {ex}");
            Dispose();
            return false;
        }

        return true;
    }

    // 注入 bark.* API 到 Lua 环境
    private void InjectBarkApi()
    {
        if (_scriptEnv == null) return;

        var id = EscapeString(_manifest.Id);
        var version = EscapeString(_manifest.Version);
        var scriptName = EscapeString(_manifest.Name);

        // 创建 C# ScriptApi 并包装为纯 Lua 表
        // events.on 回调存入 Lua 原生表，通过 __barkTriggerEvent 在 Eval 中触发
        // 并暴露 onPlayerJumpStart / onPlayerJumpOver / onPlayerDeath / onWorldGenerated 便利方法
        // 避免 Puerts 不允许给 C# 对象挂载新字段、委托跨上下文调用不可靠的问题
        var luaCode = """

                      local CS = require('csharp')
                      local _bark = CS.Bark.ScriptApi.ScriptApi('__ID__', '__VERSION__', '__NAME__')
                      local _eventCallbacks = {}

                      local function _addCallback(name, cb)
                          _eventCallbacks[name] = _eventCallbacks[name] or {}
                          table.insert(_eventCallbacks[name], cb)
                      end

                      bark = {
                          events = {
                              on = function(name, cb) _addCallback(name, cb) end
                          },
                          onWorldGenerated  = function(cb) _addCallback('world_generated', cb) end,
                          onPlayerJumpStart = function(cb) _addCallback('player_jump_start', cb) end,
                          onPlayerJumpOver  = function(cb) _addCallback('player_jump_over', cb) end,
                          onPlayerDeath     = function(cb) _addCallback('player_death', cb) end,
                          Inventor   = _bark.Inventor,
                          Item       = _bark.Item,
                          Limb       = _bark.Limb,
                          Locale     = _bark.Locale,
                          Log        = _bark.Log,
                          Player     = _bark.Player,
                          ScriptInfo = _bark.ScriptInfo,
                          Skill      = _bark.Skill,
                          World      = _bark.World,
                      }

                      function __barkTriggerEvent(name)
                          local cbs = _eventCallbacks[name]
                          if not cbs then return end
                          for _, cb in ipairs(cbs) do
                              local ok, err = pcall(cb)
                              if not ok then print('[Bark] event cb error: ' .. name .. ' ' .. tostring(err)) end
                          end
                      end

                      """.Replace("__ID__", id).Replace("__VERSION__", version).Replace("__NAME__", scriptName);

        _scriptEnv.Eval(luaCode);
    }

    // 调用生命周期钩子
    private void CallLifecycleHook(string hookName)
    {
        if (_scriptEnv == null) return;

        try
        {
            _scriptEnv.Eval($"if type({hookName}) == 'function' then {hookName}() end");
        }
        catch
        {
            // ignored
        }
    }

    // 激活模组（调用 onEnable）
    public override void Enable()
    {
        if (!_isLoaded) return;
        CallLifecycleHook("onEnable");
    }

    // 停用模组（调用 onDisable）
    public override void Disable()
    {
        if (!_isLoaded) return;
        CallLifecycleHook("onDisable");
    }

    // 卸载模组（调用 onUnload）
    public override void Unload()
    {
        if (!_isLoaded) return;
        CallLifecycleHook("onUnload");
        Dispose();
    }

    // 世界生成完成钩子
    public override void CallWorldGenerated()
    {
        CallTriggerEvent("world_generated");
    }

    // 向脚本侧发送事件通知：调用全局生命周期函数 + bark.events / bark.onXxx 回调
    public override void CallTriggerEvent(string eventName)
    {
        if (_scriptEnv == null) return;

        var hookName = EventToHookName(eventName);

        try
        {
            _scriptEnv.Eval(
                $"if type({hookName}) == 'function' then {hookName}() end " +
                $"if __barkTriggerEvent then __barkTriggerEvent('{eventName}') end");
        }
        catch
        {
            // ignored
        }
    }

    // 释放引擎资源
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
                Plugin.Logger.LogWarning($"[Bark] Lua Dispose error | {ex.Message}");
            }

            _scriptEnv = null;
        }

        _isLoaded = false;
    }

    // 转义字符串中的特殊字符（用于 PuerTS Eval 注入）
    private static string EscapeString(string value)
    {
        return value
            .Replace("\\", @"\\")
            .Replace("'", @"\'")
            .Replace("\n", @"\n")
            .Replace("\r", @"\r");
    }
}