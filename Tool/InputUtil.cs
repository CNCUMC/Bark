using System;
using System.Collections;
using System.Diagnostics.CodeAnalysis;
using UnityEngine;

namespace Bark.Tool;

[SuppressMessage("ReSharper", "UnusedMember.Global")]
[SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
public static class InputUtil
{
    private static Camera? _mainCamera;
    private static bool _cameraSearched;

    public static Vector2 MouseWorldPosition()
    {
        if (!TryGetMainCamera(out var camera) || camera == null) return Vector2.zero;
        return camera.ScreenToWorldPoint(Input.mousePosition);
    }

    public static Vector2 LeftClickPosition()
    {
        return Input.GetKeyDown(Action.LeftClick) ? MouseWorldPosition() : Vector2.zero;
    }

    public static Vector2 RightClickPosition()
    {
        return Input.GetKeyDown(Action.RightClick) ? MouseWorldPosition() : Vector2.zero;
    }

    public static IEnumerator WaitForLeftClick(Action<Vector2> callback)
    {
        yield return new WaitUntil(() => Input.GetKeyDown(Action.LeftClick));
        callback(MouseWorldPosition());
    }

    public static IEnumerator WaitForRightClick(Action<Vector2> callback)
    {
        yield return new WaitUntil(() => Input.GetKeyDown(Action.RightClick));
        callback(MouseWorldPosition());
    }

    public static WaitForClickResult WaitForLeftClick()
    {
        return new WaitForClickResult(Action.LeftClick);
    }

    public static WaitForClickResult WaitForRightClick()
    {
        return new WaitForClickResult(Action.RightClick);
    }

    private static bool TryGetMainCamera(out Camera? camera)
    {
        if (_mainCamera != null)
        {
            camera = _mainCamera;
            return true;
        }

        if (_cameraSearched)
        {
            camera = null!;
            return false;
        }

        _mainCamera = Camera.main;
        _cameraSearched = true;
        if (_mainCamera != null)
        {
            camera = _mainCamera;
            return true;
        }

        camera = null!;
        return false;
    }

    public static class Action
    {
        public const string LeftClick = "attack";
        public const string RightClick = "iteminteract";
    }

    public sealed class WaitForClickResult(string action) : CustomYieldInstruction
    {
        public Vector2 Result { get; private set; }

        public override bool keepWaiting
        {
            get
            {
                if (field) return false;
                if (!Input.GetKeyDown(action)) return true;
                Result = MouseWorldPosition();
                field = true;
                return false;
            }
        }
    }
}