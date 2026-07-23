namespace Bark.Script;

// 脚本引擎抽象基类：不依赖 Unity GameObject，由 ScriptModLoader 直接管理生命周期
public abstract class ScriptEngine
{
    public abstract bool Load(ScriptManifest manifest);
    public abstract void Enable();
    public abstract void Disable();
    public abstract void Unload();
    public abstract void CallWorldGenerated();
    public abstract void Dispose();
}