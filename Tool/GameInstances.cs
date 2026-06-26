namespace Bark.Tool;

// 游戏实例提供者 — 所有 *Util 类的统一入口
public static class GameInstances
{
    public static Body? Body => PlayerCamera.main?.body;
    public static PlayerCamera? PlayerCamera => PlayerCamera.main;
    public static WorldGeneration? World => WorldGeneration.world;
    public static ConsoleScript? Console => ConsoleScript.instance;
}