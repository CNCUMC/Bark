using Bark.Event;
using Bark.Tile;

namespace Bark.Script;

public abstract class ScriptEngine
{
    protected ScriptManifest Manifest { get; private set; } = null!;

    public virtual bool Load(ScriptManifest manifest)
    {
        Manifest = manifest;
        return true;
    }

    public abstract void Enable();
    public abstract void Disable();
    public abstract void Unload();

    public abstract void CallTriggerEvent(string eventName, BarkEvent? eventData = null);

    public abstract void ExecuteItemFile(string filePath, string? itemId,
        Item? item = null, string? action = null);

    public abstract void ExecuteTileFile(string filePath, string? tileId,
        TileScriptContext? context = null,
        string? action = null);

    public abstract void CallUpdate();

    public abstract void Dispose();
}
