using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using UnityEngine;
using Newtonsoft.Json;

public class WebGetPokemon : MonoBehaviour
{
    private readonly HttpClient _httpClient;
    private const string _url = "https://pokeapi.co/api/v2/pokemon/";

    public WebGetPokemon()
    {
        _httpClient = new HttpClient();
    }

    public async Task<List<Pokemon>> FetchPokemonDataAsync(int pageNumber)
    {
        try
        {
            int pageSize = 20;
            int start = (pageNumber - 1) * pageSize + 1;

            int end = Mathf.Min(pageNumber * pageSize, 151);
            int count = end - start + 1;
            if (count <= 0) return new List<Pokemon>();

            var tasks = Enumerable.Range(start, count).Select(async id =>
            {
                string json = await _httpClient.GetStringAsync($"{_url}{id}");
                return JsonConvert.DeserializeObject<Pokemon>(json);
            });

            Pokemon[] results = await Task.WhenAll(tasks);
            return results.OrderBy(p => p.Id).ToList();
        }
        catch (Exception e)
        {
            Debug.Log(e.Message);
            return new List<Pokemon>();
        }
    }

    public async Task<List<Pokemon>> FetchAllGen1Async()
    {
        List<Pokemon> allPokemon = new List<Pokemon>();
        
        for (int page = 1; page <= 8; page++)
        {
            var batch = await FetchPokemonDataAsync(page);
            allPokemon.AddRange(batch);
            if (batch.Count < 20) break;
        }

        return allPokemon.OrderBy(p => p.Id).ToList();
    }

    public async Task<Pokemon> FetchSinglePokemonAsync(string nameOrId)
    {
        try
        {
            string json = await _httpClient.GetStringAsync($"{_url}{nameOrId.ToLower()}");
            return JsonConvert.DeserializeObject<Pokemon>(json);
        }
        catch (Exception e)
        {
            Debug.Log(e.Message);
            return null;
        }
    }
}

public class Pokemon
{
    public int Id { get; set; }
    public string Name { get; set; }
    public Sprites sprites { get; set; }
    public List<TypeSlot> Types { get; set; }

    public string PrimaryType
    {
        get
        {
            if (Types != null && Types.Count > 0)
                return Types[0].type.Name;
            return "normal";
        }
    }

    public class Sprites
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
}
