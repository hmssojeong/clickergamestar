using System;
using System.Collections.Generic;
using UnityEngine;

#if !UNITY_WEBGL || UNITY_EDITOR
using Firebase.Firestore;
#endif

[Serializable]
#if !UNITY_WEBGL || UNITY_EDITOR
[FirestoreData]
#endif
public class CurrencySaveData
{
    #if !UNITY_WEBGL || UNITY_EDITOR
    [FirestoreProperty]
#endif
    public Dictionary<string, double> Currencies { get; set; } = new();

    #if !UNITY_WEBGL || UNITY_EDITOR
    [FirestoreProperty]
#endif
    public string LastSaveTime { get; set; }

    // Firebase를 위한 빈 생성자
    public CurrencySaveData() { }

    public static CurrencySaveData Default
    {
        get
        {
            var data = new CurrencySaveData();
            data.LastSaveTime = DateTime.Now.ToString("o");

            for (int i = 0; i < (int)ECurrencyType.Count; i++)
            {
                ECurrencyType type = (ECurrencyType)i;
                data.Currencies[type.ToString()] = 0;
            }
            return data;
        }
    }
}