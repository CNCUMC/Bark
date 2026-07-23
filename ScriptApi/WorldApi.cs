using System;
using Bark.Tool;

namespace Bark.ScriptApi;

public class WorldApi
{
    // 世界生成完成事件（Harmony 补丁触发）
    public static event Action? OnWorldGenerated;
    public void PlaceBlock(int x, int y, ushort block)
    {
        WorldUtil.PlaceBlock(x, y, block);
    }

    public void FillBlocks(int startX, int startY, int endX, int endY, ushort block)
    {
        WorldUtil.FillBlocks(startX, startY, endX, endY, block);
    }

    public void PlaceItem(int x, int y, string item)
    {
        WorldUtil.PlaceItem(x, y, item);
    }

    public int Width => (int)WorldUtil.World.width;
    public int Height => (int)WorldUtil.World.height;

    // 内部触发方法（供 Harmony 补丁调用）
    internal static void TriggerOnWorldGenerated() => OnWorldGenerated?.Invoke();
}