using CsvHelper;
using CsvHelper.Configuration;
using CsvHelper.Configuration.Attributes;
using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Networking;

public class WebGetStudentCSVTest : MonoBehaviour
{
    private async void Start()
    {
        // 일종 스펙(기획) 데이터
        string result = await GetWebText("https://raw.githubusercontent.com/mongilteacher/skku2_script_study/refs/heads/main/students.csv");
        result = result.TrimStart('\uFEFF'); // 맨 앞에 숨겨짓 문자 삭제

        // CSV-Helper (어떻게 구현 됐냐보다는 API 문서를 참고해서 잘 가져다 써라)
        var config = new CsvConfiguration(CultureInfo.CurrentCulture);
        var stringReader = new StringReader(result);
        var csv = new CsvReader(stringReader, config);

        List<Person> persons = new();
        persons = csv.GetRecords<Person>().ToList();

        foreach (Person p in persons)
        {
            Debug.Log(p);
        }
    }

    private async UniTask<string> GetWebText(string url)
    {
        var txt = (await UnityWebRequest.Get(url).SendWebRequest()).downloadHandler.text;
        return txt;
    }
}

public class Person
{
    [Name("id")]
    public int Id { get; set; }

    [Name("name")]
    public string Name { get; set; }


    [Name("age")]
    public int Age { get; set; }

    public Person()
    {

    }

    public Person(int id, string name, int age)
    {
        Id = id;
        Name = name ?? throw new ArgumentNullException(nameof(name));
        Age = age;
    }

    public override string ToString()
    {
        return $"Person(Id={Id}, Name={Name}, Age={Age})";
    }
}