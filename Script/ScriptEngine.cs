using System.IO;
using Bark.Event;

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

    // 向脚本侧发送事件：调用全局钩子函数（如 onPlayerJumpStart），
    // 传入事件数据供脚本侧 onItemUse(event) 等访问 event.ItemId / event.Item
    public abstract void CallTriggerEvent(string eventName, BarkEvent? eventData = null);

    // 执行单个脚本文件（如物品动作脚本），失败时静默吞异常。
    // itemId: 物品 ID；item: 物品实例（可为 null）；action: 触发动作名，如 "use"/"attack"
    public abstract void ExecuteFile(string filePath, string? itemId, Item? item = null, string? action = null);

    // 每帧调用脚本侧的 onUpdate() 函数（脚本侧可选定义，未定义则跳过）
    public abstract void CallUpdate();

    public abstract void Dispose();
}