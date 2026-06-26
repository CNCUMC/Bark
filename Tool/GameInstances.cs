using UnityEngine;

namespace Bark.Tool;

// 游戏实例提供者 — 所有 *Util 类的统一入口
public static class GameInstances
{
    public static Body? Body => PlayerCamera.main?.body;
    public static PlayerCamera? PlayerCam => PlayerCamera.main;
    public static WorldGeneration? World => WorldGeneration.world;
    public static ConsoleScript? Console => ConsoleScript.instance;

    public static bool IsInGame => PlayerCamera.main != null;
    public static bool IsWorldLoaded => WorldGeneration.world != null && WorldGeneration.world.worldExists;

    public static Vector2 PlayerPosition => Body != null ? (Vector2)Body.transform.position : Vector2.zero;

    public static bool TryGetBody(out Body? body)
    {
        body = Body;
        return body != null;
    }
}
