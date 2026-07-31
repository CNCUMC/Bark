using Newtonsoft.Json.Linq;

namespace Bark.Items.Templates;

// 物品模板基类。
// C# 端通过继承此类并实现 Name / BuildDefaults() 来定义模板，
// 在初始化阶段调用 new XxxTemplate().Register() 即可注册到 TemplateLoader。
// 脚本端可直接调用 TemplateLoader.Register / RegisterFromJson，无需继承此类。
public abstract class ItemTemplate
{
    // 模板名称，对应用户 JSON 中 template.type 的值（如 "gun"）
    public abstract string Name { get; }

    // 构建模板的默认属性 JObject，返回的 JSON 会作为物品的默认值被合并
    public abstract JObject BuildDefaults();

    // 注册到 TemplateLoader。通常在 Plugin.Awake() 或模组初始化时调用。
    public void Register()
    {
        TemplateLoader.Register(Name, BuildDefaults());
    }
}
