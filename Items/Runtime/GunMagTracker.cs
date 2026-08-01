using System.Collections.Generic;
using System.Linq;

namespace Bark.Items.Runtime;

// 内存弹药状态追踪器，键为枪械 Item 实例（Unity GameObject 引用），
// 支持实例级状态独立追踪，不同枪械各自独立。
//
// 生命周期：
// - 装弹时 GetOrCreate() 创建/更新状态
// - 卸弹时 Remove() 清除状态
// - 枪械被销毁时，由 GunRuntimeManager 的 OnDestroy 回调清理
// - 热重载时 ClearAll() 清空所有状态
public static class GunMagTracker
{
    // 单个枪械的弹药运行状态
    public class MagState
    {
        // 当前插入的弹匣物品 ID（模板匹配），null 表示无弹匣或直装
        public string? MagItemId;

        // 弹匣内当前余弹数（直装枪时为已装填的散装弹药数）
        public int RoundsInMag;

        // 装入的弹药物品 ID，用于后续查询 CasingType 等弹药属性
        public string? AmmoItemId;

        // 下次抛壳时应生成的弹壳类型标签，由 Fire Postfix 设置、Update Transpiler 消费后清除
        public string? PendingCasingType;
    }

    // 枪械 Item 实例 → 弹药状态
    private static readonly Dictionary<Item, MagState> States = new();

    // 获取状态（不创建）
    public static MagState? Get(Item gunItem)
    {
        return States.GetValueOrDefault(gunItem);
    }

    // 获取或创建状态
    public static MagState GetOrCreate(Item gunItem)
    {
        if (States.TryGetValue(gunItem, out var state)) return state;
        state = new MagState();
        States[gunItem] = state;

        return state;
    }

    // 清除指定枪械的追踪状态（卸弹或枪械销毁时调用）
    public static void Remove(Item gunItem)
    {
        States.Remove(gunItem);
    }

    // 热重载时清空所有状态
    public static void ClearAll()
    {
        States.Clear();
    }

    // 清理已被销毁的 Item 的僵尸条目（定期调用）
    public static void CleanupDestroyed()
    {
        var toRemove = (from kv in States where kv.Key == null || kv.Key.gameObject == null select kv.Key!).ToList();

        foreach (var item in toRemove)
            States.Remove(item);
    }
}
