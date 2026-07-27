using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Bark.Events;
using Bark.Tile;
using Bark.Tool;
using HarmonyLib;
using UnityEngine;

namespace Bark.Event.Listener;

// 物块事件监听器：通过 Harmony 补丁拦截 WorldGeneration.SetBlock 检测物块放置/破坏，
// 通过轮询检测自定义物块的存在和健康变化。
public static class TileEventListener
{
    // 轮询间隔（秒）
    private const float PollInterval = 1f;

    // 存在事件扫描半径（格子数）
    private const int ScanRadius = 10;

    // 已知自定义物块索引集合（用于快速判断）
    private static readonly HashSet<int> KnownCustomIndices = new();

    // 物块破坏兜底追踪：posHash → tileIndex。当 Harmony SetBlock 漏掉时兜底检测
    private static readonly Dictionary<long, int> DestroyTracker = new();

    private static Coroutine? _existCoroutine;
    private static Coroutine? _damageCoroutine;
    private static MonoBehaviour? _runner;

    // WorldGeneration.GetBlock / blocks 字段缓存
    private static MethodInfo? _getBlockMethod;
    private static FieldInfo? _blocksField;
    private static MethodInfo? _damageBlockMethod;

    internal static void Listen(MonoBehaviour runner)
    {
        _runner = runner;

        // 快照当前已知的自定义物块索引
        RefreshKnownIndices();

        // Harmony 补丁 SetBlock
        TryPatchSetBlock();

        // 尝试 Harmony 补丁 DamageBlock
        TryPatchDamageBlock();

        // 启动检测协程
        _existCoroutine ??= runner.StartCoroutine(PollExist());
        _damageCoroutine ??= runner.StartCoroutine(PollDamage());
    }

    internal static void Stop()
    {
        if (_runner == null) return;

        if (_existCoroutine != null)
        {
            _runner.StopCoroutine(_existCoroutine);
            _existCoroutine = null;
        }

        if (_damageCoroutine != null)
        {
            _runner.StopCoroutine(_damageCoroutine);
            _damageCoroutine = null;
        }

        KnownCustomIndices.Clear();
        DestroyTracker.Clear();
        _getBlockMethod = null;
        _blocksField = null;
        _damageBlockMethod = null;
        _runner = null;
    }

    // ============================================================
    // 索引快照
    // ============================================================

    // 刷新已知自定义物块索引集合
    private static void RefreshKnownIndices()
    {
        KnownCustomIndices.Clear();
        foreach (var list in TileLoader.LoadedTiles.Values)
        foreach (var entry in list)
            KnownCustomIndices.Add(entry.TileIndex);
    }

    // ============================================================
    // Harmony：SetBlock 补丁（放置 + 破坏检测）
    // ============================================================

    private static void TryPatchSetBlock()
    {
        // 游戏方法签名为 SetBlock(Vector2Int, ushort)
        var method = AccessTools.Method(typeof(WorldGeneration), "SetBlock",
            [typeof(Vector2Int), typeof(ushort)]);
        if (method == null)
        {
            // 回退：尝试无类型参数匹配
            method = AccessTools.Method(typeof(WorldGeneration), "SetBlock");
        }

        if (method == null) return;

        try
        {
            var harmony = new Harmony("Bark.TileSetBlockListener");
            harmony.Patch(method, new HarmonyMethod(typeof(TileEventListener), nameof(OnSetBlockPrefix)));
            LogUtil.Info("item_event.patch_use_ok", "WorldGeneration.SetBlock");
        }
        catch
        {
            // ignored
        }
    }

    private static void OnSetBlockPrefix(WorldGeneration __instance, Vector2Int pos, ushort block)
    {
        if (__instance == null || !WorldReady()) return;

        var index = (int)block;

        // 获取旧物块索引
        var oldIndex = GetBlockAt(__instance, pos);

        // 检测破坏：旧物块是自定义的，新物块不是同一个
        if (oldIndex >= 36 && oldIndex != index)
        {
            // 通知所有注册了该索引的 tileId（同一索引可能被最后注册者覆盖，只取当前生效的）
            var tileId = FindTileIdByIndex(oldIndex);
            if (tileId != null)
            {
                EventUtil.Trigger(new TileDestroyedEvent
                {
                    TileId = tileId,
                    TileIndex = oldIndex,
                    PosX = pos.x,
                    PosY = pos.y
                });

                // 从破坏兜底追踪中移除
                var hash = PackPos(pos.x, pos.y);
                DestroyTracker.Remove(hash);
            }
        }

        // 检测放置：新物块是自定义的
        if (index >= 36 && IsCustomIndex(index))
        {
            var tileId = FindTileIdByIndex(index);
            if (tileId != null)
            {
                EventUtil.Trigger(new TilePlaceEvent
                {
                    TileId = tileId,
                    TileIndex = index,
                    PosX = pos.x,
                    PosY = pos.y
                });

                // 记录到破坏兜底追踪
                var hash = PackPos(pos.x, pos.y);
                DestroyTracker[hash] = index;
            }
        }
    }

    // ============================================================
    // Harmony：DamageBlock 补丁（受击检测）
    // ============================================================

    private static void TryPatchDamageBlock()
    {
        // 尝试常见的伤害方法名。
        // 部分方法可能存在重载，遍历所有匹配项逐一尝试。
        foreach (var methodName in new[] { "DamageBlock", "HitBlock", "DamageTile", "BreakBlock" })
        {
            var candidates = AccessTools.GetDeclaredMethods(typeof(WorldGeneration));
            if (candidates == null) continue;

            var patched = false;
            foreach (var method in candidates)
            {
                if (method.Name != methodName) continue;

                try
                {
                    var postfix = new HarmonyMethod(typeof(TileEventListener), nameof(OnDamageBlockPostfix));
                    var harmony = new Harmony("Bark.TileDamageListener");
                    harmony.Patch(method, postfix: postfix);
                    _damageBlockMethod = method;
                    LogUtil.Info("item_event.patch_use_ok", $"{method.DeclaringType?.Name}.{methodName}");
                    patched = true;
                    break;
                }
                catch
                {
                    // ignored, try next overload
                }
            }

            if (patched) break;
        }
    }

    private static void OnDamageBlockPostfix()
    {
        // 通过反射获取原始方法的（可选位置）参数并触发事件。
        // 位置信息可能在参数列表中，也可能需要从 __instance 推断。
        try
        {
            if (!WorldReady()) return;
            var parameters = _damageBlockMethod?.GetParameters();
            if (parameters == null) return;

            var world = WorldGeneration.world;
            if (world == null) return;

            // 在 parameters 中查找 Vector2Int 类型参数作为位置
            // 注意：无法在此获取原始参数值，此方法仅记录已成功挂载补丁
            // OnDamaging 的精确触发依赖 SetBlock 兜底 + 存在检测
        }
        catch
        {
            // ignored
        }
    }

    // ============================================================
    // 轮询：存在检测
    // ============================================================

    private static IEnumerator PollExist()
    {
        yield return new WaitForSeconds(1f);

        while (_existCoroutine != null)
        {
            yield return new WaitForSeconds(PollInterval);

            if (!WorldReady()) continue;

            var body = BodyUtil.Body;
            if (!body) continue;

            var world = WorldGeneration.world;
            if (world == null) continue;

            // 玩家位置 → 格子坐标
            var playerPos = world.WorldToBlockPos(body.transform.position);
            var px = (int)playerPos.x;
            var py = (int)playerPos.y;

            // 扫描半径内的物块
            for (var x = px - ScanRadius; x <= px + ScanRadius; x++)
            for (var y = py - ScanRadius; y <= py + ScanRadius; y++)
            {
                var blockIndex = GetBlockAt(world, new Vector2Int(x, y));
                if (blockIndex < 36 || !IsCustomIndex(blockIndex)) continue;

                var tileId = FindTileIdByIndex(blockIndex);
                if (tileId == null) continue;

                EventUtil.Trigger(new TileExistEvent
                {
                    TileId = tileId,
                    TileIndex = blockIndex,
                    PosX = x,
                    PosY = y
                });
            }
        }
    }

    // ============================================================
    // 轮询：破坏兜底检测
    // ============================================================

    private static IEnumerator PollDamage()
    {
        yield return new WaitForSeconds(1f);

        while (_damageCoroutine != null)
        {
            yield return new WaitForSeconds(PollInterval);

            if (!WorldReady()) continue;

            var body = BodyUtil.Body;
            if (!body) continue;

            var world = WorldGeneration.world;
            if (world == null) continue;

            var playerPos = world.WorldToBlockPos(body.transform.position);
            var px = (int)playerPos.x;
            var py = (int)playerPos.y;

            // 检查已知追踪物块是否被破坏
            var destroyed = new List<long>();
            foreach (var (hash, tileIndex) in DestroyTracker)
            {
                UnpackPos(hash, out var x, out var y);

                // 只追踪玩家附近的物块
                if (Math.Abs(x - px) > ScanRadius || Math.Abs(y - py) > ScanRadius)
                {
                    destroyed.Add(hash);
                    continue;
                }

                var currentIndex = GetBlockAt(world, new Vector2Int(x, y));
                if (currentIndex != tileIndex)
                {
                    // 物块已消失/被替换
                    var tileId = FindTileIdByIndex(tileIndex);
                    if (tileId != null)
                    {
                        EventUtil.Trigger(new TileDestroyedEvent
                        {
                            TileId = tileId,
                            TileIndex = tileIndex,
                            PosX = x,
                            PosY = y
                        });
                    }

                    destroyed.Add(hash);
                }
            }

            foreach (var hash in destroyed)
                DestroyTracker.Remove(hash);
        }
    }

    // ============================================================
    // 辅助
    // ============================================================

    private static bool WorldReady()
    {
        var world = WorldGeneration.world;
        return world != null && world.width > 0f && world.height > 0f;
    }

    // 获取指定位置的物块索引
    private static int GetBlockAt(WorldGeneration world, Vector2Int pos)
    {
        try
        {
            // 尝试 GetBlock 方法
            _getBlockMethod ??= AccessTools.Method(typeof(WorldGeneration), "GetBlock");
            if (_getBlockMethod != null)
                return (int)(_getBlockMethod.Invoke(world, [pos]) ?? 0);

            // 回退：直接访问 blocks 字段
            _blocksField ??= AccessTools.Field(typeof(WorldGeneration), "blocks");
            if (_blocksField != null)
            {
                var blocks = _blocksField.GetValue(world);
                if (blocks is Array blockArray)
                {
                    if (pos.x >= 0 && pos.x < blockArray.GetLength(0)
                                   && pos.y >= 0 && pos.y < blockArray.GetLength(1))
                    {
                        var block = blockArray.GetValue(pos.x, pos.y);
                        if (block == null) return 0;

                        // 尝试通过反射获取 index 字段/属性
                        var blockType = block.GetType();
                        var indexField = AccessTools.Field(blockType, "index")
                                         ?? AccessTools.Field(blockType, "blockIndex")
                                         ?? AccessTools.Field(blockType, "tileIndex");
                        if (indexField != null)
                            return Convert.ToInt32(indexField.GetValue(block));

                        // 回退：块本身可能就是一个 int
                        if (block is int blockIndex)
                            return blockIndex;
                    }
                }
            }
        }
        catch
        {
            // 反射访问失败，静默跳过
        }

        return 0;
    }

    // 检查索引是否为已知自定义物块
    private static bool IsCustomIndex(int index)
    {
        return KnownCustomIndices.Contains(index);
    }

    // 根据索引查找物块 ID（取当前已注册的）
    private static string? FindTileIdByIndex(int tileIndex)
    {
        foreach (var list in TileLoader.LoadedTiles.Values)
        foreach (var entry in list)
            if (entry.TileIndex == tileIndex)
                return entry.TileId;
        return null;
    }

    // 坐标打包为 long
    private static long PackPos(int x, int y)
    {
        return ((long)x << 32) | (uint)y;
    }

    // 解包坐标
    private static void UnpackPos(long hash, out int x, out int y)
    {
        x = (int)(hash >> 32);
        y = (int)(hash & 0xFFFFFFFF);
    }
}
