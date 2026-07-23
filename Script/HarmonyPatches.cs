using System.Collections;
using Bark.ScriptApi;
using HarmonyLib;

namespace Bark.Script;

// Harmony 补丁：hook 世界生成完成事件
public static class HarmonyPatches
{
    // 包装 FinishWorldGeneration 协程，执行完毕后触发 OnWorldGenerated
    [HarmonyPatch(typeof(WorldGeneration), "FinishWorldGeneration")]
    public static class FinishWorldGenerationPatch
    {
        static IEnumerator Postfix(IEnumerator result)
        {
            // 先执行原始协程
            while (result.MoveNext())
                yield return result.Current;

            // 协程执行完毕，世界生成完成
            WorldApi.TriggerOnWorldGenerated();
        }
    }
}
