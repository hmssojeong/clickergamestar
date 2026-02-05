using System;
using System.Collections.Generic;
using System.Data;
using UnityEngine;
using Firebase.Firestore;

[Serializable]
[FirestoreData]
public class CurrencySaveData
{
    [FirestoreProperty]
    public Dictionary<string, double> Currencies { get; set; } = new();

    [FirestoreProperty]
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