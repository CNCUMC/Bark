using System.Diagnostics.CodeAnalysis;

namespace Bark.Constant;

[SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
[SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
[SuppressMessage("ReSharper", "UnusedMember.Global")]
public sealed class Slots
{
    public static readonly Slots MainHand = new(0, "mainhand");
    public static readonly Slots SecondaryHand = new(1, "secondaryhand");
    public static readonly Slots Mouth = new(2, "mouth");
    public static readonly Slots UpperBck = new(3, "upperback");
    public static readonly Slots MiddleBack = new(4, "middleback");
    public static readonly Slots LowerBack = new(5, "lowerback");

    private Slots(int id, string localeKey)
    {
        Id = id;
        LocaleKey = localeKey;
    }

    public int Id { get; }
    public string LocaleKey { get; }

    public static implicit operator int(Slots slot)
    {
        return slot?.Id ?? 0;
    }

    public override string ToString()
    {
        return LocaleKey;
    }

    public override bool Equals(object? obj)
    {
        return obj is Slots other && Id == other.Id;
    }

    public override int GetHashCode()
    {
        return Id.GetHashCode();
    }

    public static Slots FromId(int id)
    {
        return id switch
        {
            0 => MainHand,
            1 => SecondaryHand,
            2 => Mouth,
            3 => UpperBck,
            4 => MiddleBack,
            5 => LowerBack,
            _ => null!
        };
    }
}
