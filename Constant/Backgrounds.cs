using System.Diagnostics.CodeAnalysis;

namespace Bark.Constant;

[SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
[SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
[SuppressMessage("ReSharper", "UnusedMember.Global")]
public sealed class Backgrounds
{
    public static readonly Backgrounds Fungal = new("fungalBackground", "fungal");
    public static readonly Backgrounds Grass = new("grassBackground", "grass");
    public static readonly Backgrounds Ice = new("iceBackground", "ice");
    public static readonly Backgrounds Rock = new("rockBackground", "rock");
    public static readonly Backgrounds Sand = new("sandBackground", "sand");
    public static readonly Backgrounds Soil = new("soilBackground", "soil");
    public static readonly Backgrounds Steel = new("steelBackground", "steel");
    public static readonly Backgrounds Vents = new("ventsBackground", "vents");
    public static readonly Backgrounds Wasteland = new("wastelandBackground", "wasteland");

    private Backgrounds(string id, string localeKey)
    {
        Id = id;
        LocaleKey = localeKey;
    }

    public string Id { get; }
    public string LocaleKey { get; }

    public static implicit operator string(Backgrounds background)
    {
        return background?.Id ?? "";
    }

    public override string ToString()
    {
        return LocaleKey;
    }

    public override bool Equals(object obj)
    {
        return obj is Backgrounds other && Id == other.Id;
    }

    public override int GetHashCode()
    {
        return Id.GetHashCode();
    }

    public static Backgrounds FromId(string id)
    {
        return id switch
        {
            "fungalBackground" => Fungal,
            "grassBackground" => Grass,
            "iceBackground" => Ice,
            "rockBackground" => Rock,
            "sandBackground" => Sand,
            "soilBackground" => Soil,
            "steelBackground" => Steel,
            "ventsBackground" => Vents,
            "wastelandBackground" => Wasteland,
            _ => null!
        };
    }
}