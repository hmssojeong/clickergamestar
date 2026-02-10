using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

public class WebGetStudentCSVTest : MonoBehaviour
{
    private async void Start()
    {
        string result = await GetWebText("https://raw.githubusercontent.com/mongilteacher/skku2_script_study/refs/heads/main/students.csv");
        Debug.Log(result);

        List<Person> persons = new List<Person>();
        // 1. 읽어온 CSV 파일을 파싱해서 (파싱 방법은 블로그 또는 llm 활용)
        // 2. Person 도메인 클래스에 넣고 persons에 추가
        // 3. List<Person> persons 순회하면서 출력하세요.
    }

    private async UniTask<string> GetWebText(string url)
    {
        var txt = (await UnityWebRequest.Get(url).SendWebRequest()).downloadHandler.text;
        return txt;
    }
}

public class Person
{
    public string Name;
    public int Age;
}