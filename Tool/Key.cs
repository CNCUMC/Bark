using System;
using System.Collections;
using System.Diagnostics.CodeAnalysis;
using UnityEngine;

namespace Bark.Tool;

[SuppressMessage("ReSharper", "UnusedMember.Global")]
[SuppressMessage("ReSharper", "UnusedType.Global")]
[SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
public static class Key
{
    private static Camera _mainCamera;
    private static bool _cameraSearched;

    public static Vector2 MouseWorldPosition()
    {
        if (!TryGetMainCamera(out var camera))
            return Vector2.zero;

        return camera.ScreenToWorldPoint(Input.mousePosition);
    }

    public static Vector2 LeftClickPosition()
    {
        return !Input.GetKeyDown(InputAction.LeftClick)
            ? Vector2.zero
            : MouseWorldPosition();
    }

    public static Vector2 RightClickPosition()
    {
        return !Input.GetKeyDown(InputAction.RightClick)
            ? Vector2.zero
            : MouseWorldPosition();
    }

    public static IEnumerator WaitForLeftClick(Action<Vector2> callback)
    {
        yield return new WaitUntil(() => Input.GetKeyDown(InputAction.LeftClick));
        callback?.Invoke(MouseWorldPosition());
    }

    public static IEnumerator WaitForRightClick(Action<Vector2> callback)
    {
        yield return new WaitUntil(() => Input.GetKeyDown(InputAction.RightClick));
        callback?.Invoke(MouseWorldPosition());
    }

    public static WaitForClickResult WaitForLeftClick()
    {
        return new WaitForClickResult(InputAction.LeftClick);
    }

    public static WaitForClickResult WaitForRightClick()
    {
        return new WaitForClickResult(InputAction.RightClick);
    }

    private static bool TryGetMainCamera(out Camera camera)
    {
        if (_mainCamera != null)
        {
            camera = _mainCamera;
            return true;
        }

        if (_cameraSearched)
        {
            camera = null;
            return false;
        }

        _mainCamera = Camera.main;
        _cameraSearched = true;

        if (_mainCamera != null)
        {
            camera = _mainCamera;
            return true;
        }

        camera = null;
        return false;
    }

    public static class InputAction
    {
        public const string LeftClick = "attack";
        public const string RightClick = "iteminteract";
    }

    public sealed class WaitForClickResult(string action) : CustomYieldInstruction
    {
        private readonly string _action = action ?? throw new ArgumentNullException(nameof(action));

        // ReSharper disable once UnusedAutoPropertyAccessor.Global
        public Vector2 Result { get; private set; }

        public override bool keepWaiting
        {
            get
            {
                if (field)
                    return false;

                if (!Input.GetKeyDown(_action))
                    return true;

                Result = MouseWorldPosition();
                field = true;
                return false;
            }
        }
    }
}