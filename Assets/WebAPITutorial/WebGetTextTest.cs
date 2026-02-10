using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

public class WebGetTextTest : MonoBehaviour
{
    // HTTP 프로토콜을 이용해서 웹 서버에게 데이터 작업을 요청할 수 있다.
    // 작업 요청은 크~게 4가지 약속이 있다.
    // 1. 데이터 내놔     : GET
    // 2. 내 데이터 줄게  : POST
    // 3. 데이터 수정해줘 : PUT
    // 4. 데이터 삭제해줘 : DELETE

    void Start()
    {
        StartCoroutine(GetText());
    }

    IEnumerator GetText()
    {
        // URL이란 웹서버 어떤 "자원(페이지/이미지/파일/데이터/API)"이 있는 위치를 가리키는 주소
        UnityWebRequest www = UnityWebRequest.Get("https://www.google.com/search?q=%EB%8B%88%ED%8C%8C+%EB%B0%94%EC%9D%B4%EB%9F%AC%EC%8A%A4&sca_esv=e5f3872605fd4577&sxsrf=ANbL-n5JmQXYfNzwxrQ774tirIqTOkjJng%3A1770694965091&ei=NamKaZGlBY7s1e8PjezAkQw&biw=1600&bih=831&ved=0ahUKEwjRk5njgM6SAxUOdvUHHQ02MMIQ4dUDCBE&uact=5&oq=%EB%8B%88%ED%8C%8C+%EB%B0%94%EC%9D%B4%EB%9F%AC%EC%8A%A4&gs_lp=Egxnd3Mtd2l6LXNlcnAiE-uLiO2MjCDrsJTsnbTrn6zsiqQyCxAuGIAEGLEDGIMBMgQQABgDMgQQABgDMgQQABgDMgQQABgDMgQQABgDMgQQABgDMgQQABgDMgQQABgDMgQQABgDMhoQLhiABBixAxiDARiXBRjcBBjeBBjfBNgBAUjgHFAAWIMbcAt4AZABBJgBsQGgAZwTqgEEMC4yMLgBA8gBAPgBAZgCF6AC3gyoAgDCAhEQLhiABBixAxjRAxiDARjHAcICCxAAGIAEGLEDGIMBwgINEC4YgAQY0QMYxwEYCsICCBAAGIAEGLEDwgIFEAAYgATCAiAQLhiABBixAxjRAxiDARjHARiXBRjcBBjeBBjgBNgBAcICDhAuGIAEGLEDGNEDGMcBwgIFEC4YgATCAgQQLhgDwgIIEC4YgAQYsQPCAhMQLhgDGJcFGNwEGN4EGN8E2AEBmAMA8QXshVh4GGKH1boGBggBEAEYFJIHBTEwLjEzoAeBhQKyBwQwLjEzuAfDDMIHBjAuMTYuN8gHP4AIAA&sclient=gws-wiz-serp");
        yield return www.SendWebRequest();

        if (www.isNetworkError || www.isHttpError)
        {
            Debug.Log(www.error);
        }
        else
        {
            // Show results as text
            Debug.Log(www.downloadHandler.text);
        }
    }
}

