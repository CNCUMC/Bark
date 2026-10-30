using Newtonsoft.Json.Linq;

namespace Bark.Items.Templates;

public abstract class ItemTemplate
{
    public abstract string Name { get; }
    public abstract JObject BuildDefaults();

    public void Register()
    {
        TemplateLoader.Register(Name, BuildDefaults());
    }
}
