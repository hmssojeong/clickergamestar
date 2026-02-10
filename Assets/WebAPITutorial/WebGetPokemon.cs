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
            int end = pageNumber * pageSize; 

            var tasks = Enumerable.Range(start, pageSize).Select(async id =>
            {
                // GetFromJsonAsync 대신 GetStringAsync 사용
                string json = await _httpClient.GetStringAsync($"{_url}{id}");
                return JsonConvert.DeserializeObject<Pokemon>(json);
            });

            // 모든 요청이 완료될 때까지 비동기 대기
            Pokemon[] results = await Task.WhenAll(tasks);

            return results.ToList();

        }
        catch(Exception e)
        {
            Debug.Log(e.Message);
            return new List<Pokemon>();
        }
    }
}

public class Pokemon
{
    public int Id { get; set; }
    public string Name { get; set; }
    public Sprites sprites { get; set; }

    public class Sprites
    {
        public string Front_Default { get; set; }
    }
}
