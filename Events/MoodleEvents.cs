using Bark.Event;

namespace Bark.Events;

// Moodle 获取事件：玩家身上出现新 Moodle 时触发（补丁 MoodleRegistry.AddMoodle / AddAnimatedMoodle）
[ScriptEvent("onMoodleGet")]
public class MoodleGetEvent : BarkEvent
{
    // Moodle key（如 "bleeding"、"my_mod.bleeding"）
    public string MoodleKey { get; set; } = string.Empty;
    // Moodle 名称（可用于本地化查询）
    public string MoodleName { get; set; } = string.Empty;
    // 强度
    public int Intensity { get; set; }
    // 是否严重
    public bool Critical { get; set; }
    // 持续时间（秒），到期后自动消失
    public float HoldSeconds { get; set; }
}

// Moodle 遍历事件：Moodle 系统每帧处理活跃 Moodle 时触发（轮询，间隔 0.5s）
[ScriptEvent("onMoodleIterate")]
public class MoodleIterateEvent : BarkEvent
{
    // 当前所有活跃 Moodle 的 key 列表
    public string[] ActiveKeys { get; set; } = System.Array.Empty<string>();
}

// Moodle 失去事件：Moodle 到期或玩家身上 Moodle 移除时触发（轮询检测）
[ScriptEvent("onMoodleLose")]
public class MoodleLoseEvent : BarkEvent
{
    // 被移除的 Moodle key
    public string MoodleKey { get; set; } = string.Empty;
    // Moodle 名称
    public string MoodleName { get; set; } = string.Empty;
}
