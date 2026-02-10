using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

public class WebGetWeatherTest : MonoBehaviour
{
    private const string API_KEY = "d4a52d19cce0833c634e4c0aebcf2055";

    private async void Start()
    {
        float lat = 37.4049955f;
        float lon = 127.1060049f;

        string url =
            $"https://api.openweathermap.org/data/3.0/onecall?lat={{lat}}&lon={{lon}}&appid={{API key}}";

        Debug.Log(url);

;        string result = await GetWebText(url);
        Debug.Log(result);
    }

    private async UniTask<string> GetWebText(string url)
    {
        var txt = (await UnityWebRequest.Get(url).SendWebRequest()).downloadHandler.text;
        return txt;
    }
}
