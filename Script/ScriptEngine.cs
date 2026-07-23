using System.IO;

namespace Bark.Script;

// 脚本引擎抽象基类：不依赖 Unity GameObject，由 ScriptModLoader 直接管理生命周期
public abstract class ScriptEngine
{
    // 当前加载的模组清单（Load 时赋值）
    protected ScriptManifest Manifest { get; private set; } = null!;

    // 日志目录路径（从模组目录推导）
    protected string LogsDir => Path.GetFullPath(Path.Combine(Manifest.Directory, "..", "..", "Logs"));

    public virtual bool Load(ScriptManifest manifest)
    {
        Manifest = manifest;
        return true;
    }

    public abstract void Enable();
    public abstract void Disable();
    public abstract void Unload();

    // 向脚本侧发送事件：调用全局钩子函数（如 onPlayerJumpStart）
    public abstract void CallTriggerEvent(string eventName);

    public abstract void Dispose();
}