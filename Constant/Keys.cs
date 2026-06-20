using System.Diagnostics.CodeAnalysis;
using UnityEngine;

namespace Bark.Constant;

[SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
[SuppressMessage("ReSharper", "UnusedMember.Global")]
public static class Keys
{
    public static KeyCode Jump = KeyBinds.GetBind("jump");
    public static KeyCode Up = KeyBinds.GetBind("up");
    public static KeyCode Left = KeyBinds.GetBind("left");
    public static KeyCode Right = KeyBinds.GetBind("right");
    public static KeyCode Down = KeyBinds.GetBind("down");
    public static KeyCode WoundView = KeyBinds.GetBind("woundview");
    public static KeyCode Throw = KeyBinds.GetBind("throw");
    public static KeyCode SwitchHands = KeyBinds.GetBind("switchhands");
    public static KeyCode ToggleInventory = KeyBinds.GetBind("toggleinventory");
    public static KeyCode Speed1 = KeyBinds.GetBind("speed1");
    public static KeyCode Speed2 = KeyBinds.GetBind("speed2");
    public static KeyCode Speed3 = KeyBinds.GetBind("speed3");
    public static KeyCode Ragdoll = KeyBinds.GetBind("ragdoll");
    public static KeyCode ExpandDesc = KeyBinds.GetBind("expanddesc");
    public static KeyCode Attack = KeyBinds.GetBind("attack");
    public static KeyCode ItemInteract = KeyBinds.GetBind("iteminteract");
    public static KeyCode Pause = KeyBinds.GetBind("pause");
    public static KeyCode Console = KeyBinds.GetBind("console");
    public static KeyCode Restart = KeyBinds.GetBind("restart");
    public static KeyCode Craft = KeyBinds.GetBind("craft");
    public static KeyCode Bark = KeyBinds.GetBind("bark");
    public static KeyCode Favourite = KeyBinds.GetBind("favourite");
    public static KeyCode QuickStash = KeyBinds.GetBind("quickstash");
}