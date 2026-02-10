using System.Collections;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

public class WebGetTextTest : MonoBehaviour
{
    // HTTP 프로톨을 이용해서 웹 서버에게 데이터 작업을 요청할 수 있다.
    // 작업 요청은 크~게 4가지 약속이 있다.
    // 1. 데이터 내놔     : GET
    // 2. 데이터 줄게     : POST
    // 3. 데이터 수정해줘  : PUT
    // 4. 데이터 삭제해줘  : DELETE

    private async void Start()
    {
        string result = await GetWebText("https://www.google.com/search?q=%EB%8B%88%ED%8C%8C+%EB%B0%94%EC%9D%B4%EB%9F%AC%EC%8A%A4&newwindow=1&sca_esv=e5f3872605fd4577&hl=ko&biw=1745&bih=828&sxsrf=ANbL-n737xXOkNmF7JedZzNFMddaBZy5kA%3A1770695735197&ei=N6yKaZzdC7nK0-kPxc-ryA8&ved=0ahUKEwjc1LTSg86SAxU55TQHHcXnCvkQ4dUDCBE&uact=5&oq=%EB%8B%88%ED%8C%8C+%EB%B0%94%EC%9D%B4%EB%9F%AC%EC%8A%A4&gs_lp=Egxnd3Mtd2l6LXNlcnAiE-uLiO2MjCDrsJTsnbTrn6zsiqQyCxAuGIAEGLEDGIMBMgQQABgDMgQQABgDMgQQABgDMgQQABgDMgQQABgDMgQQABgDMgQQABgDMgsQABiABBixAxiDATIEEAAYAzIaEC4YgAQYsQMYgwEYlwUY3AQY3gQY3wTYAQFIgxVQvwdYwRRwBXgAkAEEmAGUAaABkAuqAQQwLjEyuAEDyAEA-AEBmAIJoAKCBcICERAuGIAEGLEDGNEDGIMBGMcBwgIgEC4YgAQYsQMY0QMYgwEYxwEYlwUY3AQY3gQY4ATYAQHCAgQQLhgDwgIIEC4YgAQYsQPCAggQABiABBixA8ICBRAAGIAEwgITEC4YAxiXBRjcBBjeBBjfBNgBAZgDAIgGAboGBggBEAEYFJIHAzQuNaAH-JoBsgcDMC41uAf0BMIHBTAuOC4xyAcYgAgA&sclient=gws-wiz-serp");
        Debug.Log(result);

        // 서버에게 데이터 내놔~ 하는 작업 비동기 이므로 코루틴을 이용했다.
        StartCoroutine(GetText());
    }

    private async UniTask<string> GetWebText(string url)
    {
        var txt = (await UnityWebRequest.Get(url).SendWebRequest()).downloadHandler.text;
        return txt;
    }


    IEnumerator GetText()
    {
        // URL이란 웹서버 어떤 “자원(페이지/이미지/파일/데이터/API)”이 있는 위치를 가리키는 주소
        UnityWebRequest www = UnityWebRequest.Get("https://www.google.com/search?q=%EB%8B%88%ED%8C%8C+%EB%B0%94%EC%9D%B4%EB%9F%AC%EC%8A%A4&newwindow=1&sca_esv=e5f3872605fd4577&hl=ko&biw=1745&bih=828&sxsrf=ANbL-n737xXOkNmF7JedZzNFMddaBZy5kA%3A1770695735197&ei=N6yKaZzdC7nK0-kPxc-ryA8&ved=0ahUKEwjc1LTSg86SAxU55TQHHcXnCvkQ4dUDCBE&uact=5&oq=%EB%8B%88%ED%8C%8C+%EB%B0%94%EC%9D%B4%EB%9F%AC%EC%8A%A4&gs_lp=Egxnd3Mtd2l6LXNlcnAiE-uLiO2MjCDrsJTsnbTrn6zsiqQyCxAuGIAEGLEDGIMBMgQQABgDMgQQABgDMgQQABgDMgQQABgDMgQQABgDMgQQABgDMgQQABgDMgsQABiABBixAxiDATIEEAAYAzIaEC4YgAQYsQMYgwEYlwUY3AQY3gQY3wTYAQFIgxVQvwdYwRRwBXgAkAEEmAGUAaABkAuqAQQwLjEyuAEDyAEA-AEBmAIJoAKCBcICERAuGIAEGLEDGNEDGIMBGMcBwgIgEC4YgAQYsQMY0QMYgwEYxwEYlwUY3AQY3gQY4ATYAQHCAgQQLhgDwgIIEC4YgAQYsQPCAggQABiABBixA8ICBRAAGIAEwgITEC4YAxiXBRjcBBjeBBjfBNgBAZgDAIgGAboGBggBEAEYFJIHAzQuNaAH-JoBsgcDMC41uAf0BMIHBTAuOC4xyAcYgAgA&sclient=gws-wiz-serp");
        yield return www.SendWebRequest();

        if (www.isNetworkError || www.isHttpError)
        {
            Debug.Log(www.error);
        }
        else
        {
            Debug.Log(www.downloadHandler.text);
        }
    }
}