namespace Bark.Event;

// 事件基类，所有事件类型必须继承此类
public abstract class Event
{
    // 事件触发时间（自动填充）
    public float Time { get; internal set; }
}

// 世界事件
public static class WorldEvents
{
    // 世界生成完成
    public class GeneratedEvent : Event
    {
    }
}

// 玩家事件
public static class PlayerEvents
{
    // 玩家跳跃
    public class JumpEvent : Event
    {
        public object Player { get; set; } = null!;
    }

    // 玩家死亡
    public class DeathEvent : Event
    {
        public object Player { get; set; } = null!;
    }

    // 玩家重生
    public class RespawnEvent : Event
    {
        public object Player { get; set; } = null!;
    }
}