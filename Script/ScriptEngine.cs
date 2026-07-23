using System.Text;

namespace Bark.Script;

// 脚本引擎抽象基类：不依赖 Unity GameObject，由 ScriptModLoader 直接管理生命周期
public abstract class ScriptEngine
{
    public abstract bool Load(ScriptManifest manifest);
    public abstract void Enable();
    public abstract void Disable();
    public abstract void Unload();
    public abstract void CallWorldGenerated();

    // 向脚本侧发送事件：调用全局生命周期函数 + __barkTriggerEvent 桥接
    public abstract void CallTriggerEvent(string eventName);

    public abstract void Dispose();

    // 事件名 → 生命周期钩子名
    // "player_jump_start" → "onPlayerJumpStart"
    protected static string EventToHookName(string eventName)
    {
        var sb = new StringBuilder("on");
        var up = true;
        foreach (var ch in eventName)
        {
            if (ch == '_')
            {
                up = true;
                continue;
            }

            sb.Append(up ? char.ToUpperInvariant(ch) : ch);
            up = false;
        }

        return sb.ToString();
    }
}