using System.Collections;
using Bark.ScriptApi;
using HarmonyLib;

namespace Bark.Script;

public static class HarmonyPatches
{
    [HarmonyPatch(typeof(WorldGeneration), "FinishWorldGeneration")]
    private static IEnumerator Postfix(IEnumerator result)
    {
        while (result.MoveNext())
            yield return result.Current;
        WorldApi.TriggerOnWorldGenerated();
    }
}