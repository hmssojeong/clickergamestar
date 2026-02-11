using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;

public class Pokemon
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string KoreanName { get; set; }  // species API에서 채워짐
    public int Height { get; set; }
    public int Weight { get; set; }
    public Sprites sprites { get; set; }
    public List<TypeSlot> Types { get; set; }
    public List<StatEntry> Stats { get; set; }

    public string PrimaryType
    {
        get
        {
            if (Types != null && Types.Count > 0)
                return Types[0].type.Name;
            return "normal";
        }
    }

    public string HeightFormatted => $"{Height / 10f} m";

    public string WeightFormatted => $"{Weight / 10f} kg";

    public class Sprites
    {
        public string Front_Default { get; set; }

        public OtherSprites Other { get; set; }

        public string ArtworkUrl
        {
            get
            {
                string url = Other?.OfficialArtwork?.Front_Default;
                return !string.IsNullOrEmpty(url) ? url : Front_Default;
            }
        }
    }

    public class OtherSprites
    {
        [JsonProperty("official-artwork")]
        public OfficialArtwork OfficialArtwork { get; set; }
    }


    public class  OfficialArtwork
    {
        public string Front_Default { get; set; }
    }

    public class TypeSlot
    {
        public int Slot { get; set; }
        public TypeInfo type { get; set; }
    }

    public class TypeInfo
    {
        public string Name { get; set; }
    }

    public class StatEntry
    {
        [JsonProperty("base_stat")]
        public int BaseStat { get; set; }
        public StatInfo stat { get; set; }
    }

    public class StatInfo
    {
        public string Name { get; set; }
    }
}

// 포켓몬 종 데이터 (/pokemon-species/{id}) — 설명 텍스트 포함
public class NameEntry
{
    public string Name { get; set; }
    public NamedResource Language { get; set; }
}

public class PokemonSpecies
{
    [JsonProperty("names")]
    public List<NameEntry> Names { get; set; }

    [JsonProperty("flavor_text_entries")]
    public List<FlavorTextEntry> FlavorTextEntries { get; set; }

    public string GetFlavorText(string lang = "en")
    {
        if (FlavorTextEntries == null) return "";

        foreach (var entry in FlavorTextEntries)
        {
            if (entry.Language?.Name == lang)
            {
                return entry.Flavor_Text
                    .Replace("\n", " ")
                    .Replace("\f", " ")
                    .Replace("\r", " ");
            }
        }
        return "";
    }

    public string GetLocalizedName(string lang = "ko")
    {
        if (Names == null) return "";
        foreach (var entry in Names)
        {
            if (entry.Language?.Name == lang)
                return entry.Name;
        }
        return "";
    }
}

public class FlavorTextEntry
{
    [JsonProperty("flavor_text")]
    public string Flavor_Text { get; set; }

    public NamedResource Language { get; set; }
    public NamedResource Version { get; set; }
}

public class PokemonTypeData
{
    [JsonProperty("damage_relations")]
    public DamageRelations DamageRelations { get; set; }
}

public class DamageRelations
{
    [JsonProperty("double_damage_from")]
    public List<NamedResource> DoubleDamageFrom { get; set; }

    [JsonProperty("double_damage_to")]
    public List<NamedResource> DoubleDamageTo { get; set; }

    [JsonProperty("half_damage_from")]
    public List<NamedResource> HalfDamageFrom { get; set; }

    [JsonProperty("half_damage_to")]
    public List<NamedResource> HalfDamageTo { get; set; }

    [JsonProperty("no_damage_from")]
    public List<NamedResource> NoDamageFrom { get; set; }

    [JsonProperty("no_damage_to")]
    public List<NamedResource> NoDamageTo { get; set; }
}

// 공통: 이름과 URL을 가진 리소스
public class NamedResource
{
    public string Name { get; set; }
    public string Url { get; set; }
}
