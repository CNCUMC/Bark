using System;
using System.Collections;
using CUCoreLib.Helpers;
using UnityEngine;

namespace Bark.Tool;

public static class InputUtil
{
    public static Vector2 LeftClickPosition() => Input.GetKeyDown(Action.LeftClick) ? CUCoreUtils.GetMousePosition() : Vector2.zero;
    public static Vector2 RightClickPosition() => Input.GetKeyDown(Action.RightClick) ? CUCoreUtils.GetMousePosition() : Vector2.zero;

    public static IEnumerator WaitForLeftClick(Action<Vector2> callback)
    {
        yield return new WaitUntil(() => Input.GetKeyDown(Action.LeftClick));
        callback(CUCoreUtils.GetMousePosition());
    }

    public static IEnumerator WaitForRightClick(Action<Vector2> callback)
    {
        yield return new WaitUntil(() => Input.GetKeyDown(Action.RightClick));
        callback(CUCoreUtils.GetMousePosition());
    }

    public static WaitForClickResult WaitForLeftClick() => new(Action.LeftClick);
    public static WaitForClickResult WaitForRightClick() => new(Action.RightClick);


    public static class Action
    {
        public const string LeftClick = "attack";
        public const string RightClick = "iteminteract";
    }

    public sealed class WaitForClickResult(string action) : CustomYieldInstruction
    {
        public override bool keepWaiting
        {
            get
            {
                if (field) return false;
                if (!Input.GetKeyDown(action)) return true;
                CUCoreUtils.GetMousePosition();
                field = true;
                return false;
            }
        }
    }
}