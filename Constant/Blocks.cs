using System.Diagnostics.CodeAnalysis;

namespace Bark.Constant;

[SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
[SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
[SuppressMessage("ReSharper", "UnusedMember.Global")]
public sealed class Blocks
{
    public static readonly Blocks Air = new(0, "air", 0f, "", "Rock");
    public static readonly Blocks LightRock = new(1, "lightrock", 100f, "rock", "Rock", "Bad");
    public static readonly Blocks Gravel = new(2, "gravel", 25f, "dirt", "Gravel");
    public static readonly Blocks ScrapPile = new(3, "scrappile", 60f, "scrapmetal", "Scrap", "Mediocre");
    public static readonly Blocks TrashPile = new(4, "trashpile", 20f, "trash", "Scrap", "Mediocre");
    public static readonly Blocks ConcreteTile = new(5, "concretetile", 800f, "concrete", "Concrete", "Mediocre");
    public static readonly Blocks SteelTile = new(6, "steeltile", 5000f, "steel", "Steel", "Mediocre", true);
    public static readonly Blocks Glass = new(7, "glass", 30f, "glass", "Glass", "Mediocre", noVariation: true);
    public static readonly Blocks Rubber = new(8, "rubber", 60f, "rubber", "Rubber", "Good");
    public static readonly Blocks Plastic = new(9, "plastic", 150f, "rubber", "Plastic");

    public static readonly Blocks HeatResistantAlloy =
        new(10, "heatresistantalloy", 15000f, "steel", "Steel", "Mediocre", true);

    public static readonly Blocks Wood = new(11, "wood", 150f, "wood", "Wood", noVariation: true);
    public static readonly Blocks Sand = new(12, "sand", 15f, "sand", "Sand", "Good");
    public static readonly Blocks Sandstone = new(13, "sandstone", 90f, "rock", "Rock", "Bad");
    public static readonly Blocks InfiniRock = new(14, "infinirock", 420133760f, "rock", "Rock", "Bad");
    public static readonly Blocks Clay = new(15, "clay", 25f, "sand", "Sand");
    public static readonly Blocks Soil = new(16, "soil", 32f, "dirt", "Gravel");
    public static readonly Blocks Granite = new(17, "granite", 200f, "rock", "Concrete", "Bad");
    public static readonly Blocks Marble = new(18, "marble", 150f, "rock", "Concrete", "Bad");
    public static readonly Blocks Limestone = new(19, "limestone", 135f, "rock", "Concrete", "Bad");
    public static readonly Blocks Bricks = new(20, "bricks", 650f, "concrete", "Concrete", "Bad", noVariation: true);
    public static readonly Blocks Scaffolding = new(21, "scaffolding", 200f, "steel", "Steel", "Mediocre", true, true);
    public static readonly Blocks ToxiRock = new(22, "toxirock", 250f, "rock", "Concrete", "Bad", toxicity: 2.5f);
    public static readonly Blocks GrassBlock = new(23, "grass", 35f, "rustle", "Grass", "Good");
    public static readonly Blocks Log = new(24, "log", 150f, "wood", "Wood", "Mediocre", noVariation: true);
    public static readonly Blocks Leaves = new(25, "leaves", 20f, "rustle", "Grass");
    public static readonly Blocks Snow = new(26, "snow", 15f, "sand", "Snow", "Good");
    public static readonly Blocks Ice = new(27, "ice", 50f, "glass", "Ice", "Mediocre", isSlippery: true);
    public static readonly Blocks ThinIce = new(28, "thinice", 1f, "glass", "Ice", "Mediocre", isSlippery: true);
    public static readonly Blocks PowderSnow = new(29, "powdersnow", 1f, "sand", "Snow", "Good");
    public static readonly Blocks HeavyRock = new(30, "heavyrock", 200f, "rock", "Rock", "Bad");
    public static readonly Blocks Fungus = new(31, "fungus", 50f, "gore2", "Grass");
    public static readonly Blocks MushroomBody = new(32, "mushroombody", 80f, "gore2", "Plastic", "Mediocre");
    public static readonly Blocks MushroomCap = new(33, "mushroomcap", 60f, "gore2", "Plastic", "Mediocre");
    public static readonly Blocks Copper = new(34, "copper", 2000f, "crystal", "Rock", "Bad");
    public static readonly Blocks Ilmenite = new(35, "ilmenite", 4000f, "rock", "Rock", "Bad");

    private Blocks(
        ushort id,
        string localeKey,
        float health,
        string hitSound,
        string stepSound,
        string sleepQuality = "Okay",
        bool isMetallic = false,
        bool noVariation = false,
        float toxicity = 0f,
        bool isSlippery = false)
    {
        Id = id;
        LocaleKey = localeKey;
        Health = health;
        HitSound = hitSound;
        StepSound = stepSound;
        SleepQuality = sleepQuality;
        IsMetallic = isMetallic;
        NoVariation = noVariation;
        Toxicity = toxicity;
        IsSlippery = isSlippery;
    }

    public ushort Id { get; }
    public string LocaleKey { get; }
    public float Health { get; }
    public string HitSound { get; }
    public string StepSound { get; }
    public bool IsMetallic { get; }
    public bool NoVariation { get; }
    public float Toxicity { get; }
    public bool IsSlippery { get; }
    public string SleepQuality { get; }

    public static implicit operator ushort(Blocks block)
    {
        return block?.Id ?? 0;
    }

    public override string ToString()
    {
        return LocaleKey;
    }

    public override bool Equals(object obj)
    {
        return obj is Blocks other && Id == other.Id;
    }

    public override int GetHashCode()
    {
        return Id.GetHashCode();
    }

    public static Blocks FromId(ushort id)
    {
        return id switch
        {
            0 => Air,
            1 => LightRock,
            2 => Gravel,
            3 => ScrapPile,
            4 => TrashPile,
            5 => ConcreteTile,
            6 => SteelTile,
            7 => Glass,
            8 => Rubber,
            9 => Plastic,
            10 => HeatResistantAlloy,
            11 => Wood,
            12 => Sand,
            13 => Sandstone,
            14 => InfiniRock,
            15 => Clay,
            16 => Soil,
            17 => Granite,
            18 => Marble,
            19 => Limestone,
            20 => Bricks,
            21 => Scaffolding,
            22 => ToxiRock,
            23 => GrassBlock,
            24 => Log,
            25 => Leaves,
            26 => Snow,
            27 => Ice,
            28 => ThinIce,
            29 => PowderSnow,
            30 => HeavyRock,
            31 => Fungus,
            32 => MushroomBody,
            33 => MushroomCap,
            34 => Copper,
            35 => Ilmenite,
            _ => null!
        };
    }
}