using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using Cysharp.Threading.Tasks;

public class WebGetImageTest : MonoBehaviour
{
    public RawImage MyImage;

    private async void Start()
    {
        // StartCoroutine(GetTexture());

        // URL이란 웹서버 어떤 “자원(텍스트/이미지/사운/데이터/API)”이 있는 위치를 가리키는 주소
        // URL 구성
        // 프로토콜   : http(s):// 
        // 경로(주소) : placecats.com/neo_2/300/300  (함수 이름)
        // 쿼리      : ?fit=contain&position=right  (함수 매개변수)
        //          - ?로 시작하고, &로 구분한다. (?키1=값1&키2=값2&키3=값3)
        //          - fit=contain
        //          - position=right
        //          ㄴ 옵션인데.. 매번 다르므로 웹서버개발자와 이야기를 잘 하거나 문서를 잘 봐야한다.

        MyImage.texture = await GetWebTexture("https://placecats.com/neo_2/300/300?fit=contain&position=right\n");
    }

    private async UniTask<Texture> GetWebTexture(string url)
    {
        try
        {
            var texture =
                ((DownloadHandlerTexture)((await UnityWebRequestTexture.GetTexture(url).SendWebRequest())
                    .downloadHandler)).texture;
            return texture;
        }
        catch (Exception e)
        {
            Debug.LogError(e.Message);
            return null;
        }
    }

    IEnumerator GetTexture()
    {
        UnityWebRequest www = UnityWebRequestTexture.GetTexture("https://image.kmib.co.kr/online_image/2019/1217/611811110014040718_1.jpg");
        yield return www.SendWebRequest();

        if (www.isNetworkError || www.isHttpError)
        {
            Debug.Log(www.error);
        }
        else
        {
            Texture myTexture = ((DownloadHandlerTexture)www.downloadHandler).texture;
            MyImage.texture = myTexture;
        }
    }
}