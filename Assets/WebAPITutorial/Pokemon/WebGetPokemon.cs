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
    private const string _pokemonUrl = "https://pokeapi.co/api/v2/pokemon/";
    private const string _speciesUrl = "https://pokeapi.co/api/v2/pokemon-species/";
    private const string _typeUrl = "https://pokeapi.co/api/v2/type/";

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
                string json = await _httpClient.GetStringAsync($"{_pokemonUrl}{id}");
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
            string json = await _httpClient.GetStringAsync($"{_pokemonUrl}{nameOrId.ToLower()}");
            return JsonConvert.DeserializeObject<Pokemon>(json);
        }
        catch (Exception e)
        {
            Debug.Log(e.Message);
            return null;
        }
    }

    public async Task<PokemonSpecies> FetchPokemonSpeciesAsync(int id)
    {
        try
        {
            string json = await _httpClient.GetStringAsync($"{_speciesUrl}{id}");
            return JsonConvert.DeserializeObject<PokemonSpecies>(json);
        }
        catch (Exception e)
        {
            Debug.Log(e.Message);
            return null;
        }
    }

    public async Task<PokemonTypeData> FetchTypeDamageAsync(string typeName)
    {
        try
        {
            string json = await _httpClient.GetStringAsync($"{_typeUrl}{typeName.ToLower()}");
            return JsonConvert.DeserializeObject<PokemonTypeData>(json);
        }
        catch (Exception e)
        {
            Debug.Log(e.Message);
            return null;
        }
    }
}
